﻿/**
 * Copyright(C) 2017-2021  Sojatia Infocrafts Private Limited
 * 
 * This file (QueryCache.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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

 * You can release yourself from the requirements of the AGPL license by purchasing
 * a commercial license (dotEntity or dotEntity Pro). Buying such a license is mandatory as soon as you
 * develop commercial activities involving the dotEntity software without
 * disclosing the source code of your own applications. The activites include:
 * shipping dotEntity with a closed source product, offering paid services to customers
 * as an Application Service Provider.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/licensing
 */
using System.Collections.Generic;

namespace DotEntity
{
    public class QueryCache : IWrappedDisposable
    {
        internal QueryCache()
        {
            
        }

        private bool _disposed = false;
        public void Dispose()
        {
            _disposed = true;
        }

        public bool IsDisposed()
        {
            return _disposed;
        }

        internal string CachedQuery { get; set; }

        internal object[] ParameterValues { get; set; }

        private IList<QueryInfo> _queryInfos;
        internal IList<QueryInfo> QueryInfo
        {
            get
            {
                if (_queryInfos == null)
                    return null;
                for (var i = 0; i < ParameterValues.Length; i++)
                {
                    var qi = _queryInfos[i];
                    if (!qi.SupportOperator && !qi.IsPropertyValueAlsoProperty)
                    {
                        qi.PropertyValue = ParameterValues[i];
                    }
                }
                return _queryInfos;
            }
            set => _queryInfos = value;
        }
    }
}