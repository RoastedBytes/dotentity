﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DotEntity.Extensions;

namespace DotEntity.MySql
{
    public class MySqlTableGenerator : DefaultDatabaseTableGenerator
    {
        public override string GetFormattedDbTypeForType(Type type, int maxLength = 0)
        {
            Throw.IfInvalidDataTypeMapping(!TypeMap.TryGetValue(type, out string dbTypeString), type);
            var typeBuilder = new StringBuilder(dbTypeString);
            if (maxLength > 0)
                typeBuilder.Append($"({maxLength})");
            typeBuilder.Append(type.IsNullable() ? " NULL" : " NOT NULL");
            return typeBuilder.ToString();
        }

        public override string GetCreateTableScript(Type type)
        {
            var tableName = DotEntityDb.GetTableNameForType(type);
            var properties = type.GetDatabaseUsableProperties();

            var builder = new StringBuilder($"CREATE TABLE {tableName.ToEnclosed()}{Environment.NewLine}(");
            var keyColumn = type.GetKeyColumnName();

            //is key column nullable
            var propertyInfos = properties as PropertyInfo[] ?? properties.ToArray();
            Throw.IfKeyTypeNullable(propertyInfos.First(x => x.Name == keyColumn).PropertyType, keyColumn);

            foreach (var property in propertyInfos)
            {
                var pType = property.PropertyType;
                var fieldName = property.Name;
                var dbFieldType = GetFormattedDbTypeForType(pType);
                var identityString = "";
                //do we have key attribute here?
                if (fieldName == keyColumn && pType == typeof(int))
                {
                    identityString = " AUTO_INCREMENT";
                }
                builder.Append($"\t {fieldName.ToEnclosed()} {dbFieldType}{identityString},");
                builder.Append(Environment.NewLine);
            }
            builder.Append($"PRIMARY KEY ({keyColumn.ToEnclosed()}));");
            return builder.ToString();
        }

        public override string GetDropConstraintScript(Relation relation)
        {
            var fromTable = DotEntityDb.GetTableNameForType(relation.SourceType);
            var toTable = DotEntityDb.GetTableNameForType(relation.DestinationType);
            var constraintName = GetForeignKeyConstraintName(fromTable, toTable, relation.SourceColumnName,
                relation.DestinationColumnName);
            var builder = new StringBuilder($"ALTER TABLE {toTable.ToEnclosed()}{Environment.NewLine}");
            builder.Append($"DROP FOREIGN KEY {constraintName.ToEnclosed()};");
            return builder.ToString();
        }
    }
}