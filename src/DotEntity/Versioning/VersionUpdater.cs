﻿/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (VersionUpdater.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
 * 
 * dotEntity is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 
 * dotEntity is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU Affero General Public License for more details.
 
 * You should have received a copy of the GNU Affero General Public License
 * along with dotEntity.If not, see<http://www.gnu.org/licenses/>.

 * You can release yourself from the requirements of the license by purchasing
 * a commercial license.Buying such a license is mandatory as soon as you
 * develop commercial software involving the dotEntity software without
 * disclosing the source code of your own applications.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/legal/commercial
 */
using System;
using System.Linq;
using DotEntity.Constants;
using DotEntity.Reflection;
using DotEntity.Utils;

namespace DotEntity.Versioning
{
    public class VersionUpdater
    {
        public void RunUpgrade()
        {
            DotEntityDb.MapTableNameForType<DotEntityVersion>(Configuration.VersionTableName);

            //do we have versioning table
            if (!DotEntityDb.Provider.IsDatabaseVersioned(Configuration.VersionTableName))
            {
                //we'll have to setup the version table first
                using (var transaction = EntitySet.BeginTransaction())
                {
                    DotEntity.Database.CreateTable<DotEntityVersion>(transaction);
                    transaction.Commit();

                    if (!transaction.Success)
                    {
                        throw new Exception("Couldn't setup version");
                    }
                }
            }

            //first get all the versions from database
            var appliedDatabaseVersions = EntitySet<DotEntityVersion>.Select().ToList();
            var availableVersions = TypeFinder.ClassesOfType<IDatabaseVersion>()
                .Select(x => (IDatabaseVersion)Instantiator.GetInstance(x));

            using (var transaction = EntitySet.BeginInternalTransaction())
            {
                foreach (var availableVersion in availableVersions)
                {
                    if (appliedDatabaseVersions.Any(x => x.VersionKey == availableVersion.VersionKey)) //already applied this one
                        continue;

                    //upgrade this
                    availableVersion.Upgrade(transaction);
                    var newVersion = new DotEntityVersion()
                    {
                        VersionKey = availableVersion.VersionKey
                    };
                    //insert the version
                    EntitySet<DotEntityVersion>.Insert(newVersion, transaction);
                }
                (transaction as DotEntityTransaction)?.CommitInternal();
            }
            
        }

        public void RunDowngrade(string versionKey = null)
        {
            DotEntityDb.MapTableNameForType<DotEntityVersion>(Configuration.VersionTableName);
            Throw.IfDbNotVersioned(!DotEntityDb.Provider.IsDatabaseVersioned(Configuration.VersionTableName));
           
            //first get all the versions from database
            var appliedDatabaseVersions = EntitySet<DotEntityVersion>.Select().ToList();

            if (versionKey != null)
            {
                if (appliedDatabaseVersions.All(x => x.VersionKey != versionKey))
                {
                    return; // do nothing. we don't have that version
                }
            }

            var availableVersions = TypeFinder.ClassesOfType<IDatabaseVersion>()
                .Select(x => (IDatabaseVersion) Instantiator.GetInstance(x)).Reverse(); //in reverse order


            using (var transaction = EntitySet.BeginInternalTransaction())
            {
                foreach (var availableVersion in availableVersions)
                {
                    if (versionKey == availableVersion.VersionKey)
                        break; //stop here. everything done
                    var vKey = availableVersion.VersionKey;
                    if (appliedDatabaseVersions.Any(x => versionKey != null && x.VersionKey == vKey)) //already applied this one
                        continue;

                    availableVersion.Downgrade(transaction);
                    //remove the version
                    EntitySet<DotEntityVersion>.Delete(x => x.VersionKey == vKey, transaction);
                }
                (transaction as DotEntityTransaction)?.CommitInternal();
            }

        }
    }
}