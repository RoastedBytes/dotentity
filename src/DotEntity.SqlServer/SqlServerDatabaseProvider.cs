﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using DotEntity.Providers;
using DotEntity.SqlServer.Internals.System;

namespace DotEntity.SqlServer
{
    public class SqlServerDatabaseProvider : IDatabaseProvider
    {
        public IDbConnection Connection => new SqlConnection(DotEntityDb.ConnectionString);

        public string ProviderName => "System.Data.SqlClient";

        public string DatabaseName { get; } //not required here

        public bool IsDatabaseVersioned(string tableName)
        {
            var t = EntitySet<InformationSchema.Tables>.Where(x => x.TABLE_NAME == tableName)
                .OrderBy(x => x.TABLE_CATALOG)
                .Select(1, 1);
            return t.Any();
        }

        public SqlServerDatabaseProvider()
        {
            DotEntityDb.MapTableNameForType<InformationSchema.Tables>("INFORMATION_SCHEMA.TABLES");
            DotEntityDb.MapTableNameForType<InformationSchema.Columns>("INFORMATION_SCHEMA.COLUMNS");
        }

        public IDatabaseTableGenerator DatabaseTableGenerator { get; set; }

        public IQueryGenerator QueryGenerator { get; set; }

        public ITypeMapProvider TypeMapProvider { get; set; }

        private int _maximumParameterPerQuery = 2100;

        public int MaximumParametersPerQuery
        {
            get => 2100;
            set => _maximumParameterPerQuery = value;
        }

        public string SafeEnclose(string objectName)
        {
            return $"[{objectName}]";
        }
    }
}
