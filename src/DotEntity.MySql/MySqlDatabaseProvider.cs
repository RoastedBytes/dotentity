﻿using System.Data;
using DotEntity.Providers;
using System.Linq;
using DotEntity.MySql.Internals.System;
using MySql.Data.MySqlClient;

namespace DotEntity.MySql
{
    public class MySqlDatabaseProvider : IDatabaseProvider
    {
        public IDbConnection Connection => new MySqlConnection(DotEntityDb.ConnectionString);

        public string ProviderName => "MySql.Data";

        public string DatabaseName { get; }

        public MySqlDatabaseProvider(string databaseName)
        {
            DatabaseName = databaseName;
            QueryGenerator = new MySqlQueryGenerator();
            TypeMapProvider = new MySqlTypeMapProvider();
            DotEntityDb.MapTableNameForType<InformationSchema.Tables>("INFORMATION_SCHEMA.TABLES");
            DotEntityDb.MapTableNameForType<InformationSchema.Columns>("INFORMATION_SCHEMA.COLUMNS");
        }

        public bool IsDatabaseVersioned(string versionTableName)
        {
            versionTableName = versionTableName.ToLower();
            var t = EntitySet<InformationSchema.Tables>
                .Where(x => x.TABLE_NAME == versionTableName && x.TABLE_SCHEMA == DatabaseName)
                .Select();
            return t.Any();
        }

        private IDatabaseTableGenerator _databaseTableGenerator;
        public IDatabaseTableGenerator DatabaseTableGenerator
        {
            get
            {
                _databaseTableGenerator = _databaseTableGenerator ?? new MySqlTableGenerator();
                return _databaseTableGenerator;
            }
            set => _databaseTableGenerator = value;
        }

        public IQueryGenerator QueryGenerator { get; set; }
        public ITypeMapProvider TypeMapProvider { get; set; }
        public int MaximumParametersPerQuery { get; set; }
    }
}