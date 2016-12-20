using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using Wicture.DbRESTFul.Converters;

namespace Wicture.DbRESTFul
{
    internal class SQLBuilder
    {
        public string[] TableNames { get; set; }

        private string connectionString;

        public SQLBuilder(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// 生成Insert查询SQL
        /// </summary>
        public string BuildInsertSQL(string tableName, string parameters)
        {
            if (string.IsNullOrEmpty(parameters))
            {
                throw new ArgumentNullException("parameters");
            }

            var objArray = JsonConvert.DeserializeObject(parameters) as JArray;

            if (objArray == null)
            {
                throw new ParameterParseException("Unable to deserialize the parameters.");
            }

            string sql = null;

            foreach (var obj in objArray)
            {
                sql += string.Format("INSERT INTO {0} ", tableName);
                string cols = "(";
                string colsParams = "(";

                foreach (var item in obj as JObject)
                {
                    var v = item.Value.ToObject(typeof(object));
                    cols += "`" + item.Key.ToString() + "`,";
                    colsParams += new ValueObject() { Value = v } + ",";
                }

                sql += cols.Remove(cols.Length - 1) + ") VALUES " + colsParams.Remove(colsParams.Length - 1) + ");";

            }

            return sql;
        }

        /// <summary>
        /// 生成Select查询SQL
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="parameters"></param>
        /// <param name="countSql">查询总数的SQL，只需要传入新建的StringBuilder对象即可</param>
        public string BuildSelectSQL(string tableName, string parameters, StringBuilder countSql = null, Pagination pagination = null)
        {
            if (string.IsNullOrEmpty(parameters))
            {
                return "SELECT * FROM " + tableName;
            }

            var condition = string.Empty;

            var jToken = JObject.Parse(parameters);
            var param = ParameterParser.GetOperationParameter(jToken);
            var selectSQL = GetSelectSQL(param, tableName);

            var hasGroupBy = false;

            if (param.WhereParameter != null) condition += " WHERE " + param.WhereParameter;
            if (param.GroupByParameter != null)
            {
                hasGroupBy = true;

                condition += " GROUP BY " + string.Join(",", param.GroupByParameter.Select(e => "`" + e + "`"));
                if (param.HavingParameter != null) condition += " HAVING " + param.HavingParameter;
            }
            var countStr = condition;
            if (param.OrderParameter != null) condition += " " + param.OrderParameter;
            if (param.PaginationParameter != null)
            {
                condition += " " + param.PaginationParameter;
                if (pagination != null)
                {
                    pagination.index = param.PaginationParameter.Index;
                    pagination.size = param.PaginationParameter.Size;
                }
                if (countSql != null)
                {
                    if (hasGroupBy)
                        countSql.Clear().Append("SELECT COUNT(*) FROM (").Append(selectSQL).Append(countStr).Append(") AS TMPTABLE;");
                    else
                        countSql.Clear().Append("SELECT COUNT(*) FROM ").Append(tableName).Append(countStr);
                }
            }

            return selectSQL + condition;
        }

        /// <summary>
        /// 生成Update查询SQL
        /// </summary>
        public string BuildUpdateSQL(string tableName, string where, string parameters, out object parsedData)
        {
            var condition = string.Empty;
            if (!string.IsNullOrWhiteSpace(where))
            {
                var jToken = JObject.Parse(where);
                var param = ParameterParser.GetOperationParameter(jToken);

                if (param.WhereParameter != null) condition += " WHERE " + param.WhereParameter;
            }
            var obj = JsonConvert.DeserializeObject(parameters) as JObject;
            if (obj == null)
            {
                throw new ParameterParseException("Unable to deserialize the parameters.");
            }

            var data = new ExpandoObject() as IDictionary<string, Object>;

            foreach (var item in obj)
            {
                data.Add(item.Key, (item.Value as JValue).Value);
            }

            var dynamicParameter = new DynamicParameters(data);
            List<string> paramNames = GetParamNames(dynamicParameter);

            var b = new StringBuilder();
            b.Append("UPDATE `").Append(tableName).Append("` SET ");
            b.Append(string.Join(",", paramNames.Select(p => "`" + p + "`= @" + p)));
            b.Append(condition);

            parsedData = data;

            return b.ToString();
        }

        /// <summary>
        /// 生成Delete查询SQL 
        /// </summary>
        public string BuildDeleteSQL(string tableName, string parameters)
        {
            if (string.IsNullOrEmpty(parameters))
            {
                return "DELETE FROM " + tableName;
            }

            var condition = string.Empty;
            var jToken = JObject.Parse(parameters);
            var param = ParameterParser.GetOperationParameter(jToken);

            if (param.WhereParameter != null) condition += " WHERE " + param.WhereParameter;

            var b = new StringBuilder();
            b.Append("DELETE FROM ").Append(tableName).Append(condition);

            return b.ToString();
        }

        private string GetSelectSQL(OperationParameter op, string tableName)
        {
            if (op == null || (op.ExcludesColumnParameter == null && op.IncludesColumnParameter == null))
            {
                return string.Format("SELECT * FROM {0}", tableName);
            }

            if (op.ExcludesColumnParameter != null)
            {
                var sql = @"SELECT `COLUMN_NAME` 
                        FROM `INFORMATION_SCHEMA`.`COLUMNS` 
                        WHERE TABLE_SCHEMA = '{0}' AND `TABLE_NAME`='{1}' {2};";

                var fields = "AND `COLUMN_NAME` NOT IN(" + string.Join(",", op.ExcludesColumnParameter.Select(e => { e.IsStringFormat = true; return "'" + e.ToString() + "'"; })) + ")";

                using (var cnn = new SqlConnection(connectionString))
                {
                    var columns = SqlMapper.Query<string>(cnn, string.Format(sql, cnn.Database, tableName, fields));

                    return string.Format("SELECT {0} FROM {1}", string.Join(",", columns.Select(e => "`" + e + "`")), tableName);
                }
            }
            else
            {
                var hasDistinct = op.IncludesColumnParameter.Any(e => (e is ComplexColumnObject) && (e as ComplexColumnObject).IsDistinct);
                return string.Format(hasDistinct ? "SELECT DISTINCT {0} FROM {1}" : "SELECT {0} FROM {1}", string.Join(",", op.IncludesColumnParameter.Select(e => e.ToString())), tableName);
            }
        }


        private List<string> GetParamNames(object o)
        {
            if (o is DynamicParameters)
            {
                return (o as DynamicParameters).ParameterNames.ToList();
            }

            List<string> paramNames = new List<string>();
            foreach (var prop in o.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public))
            {
                paramNames.Add(prop.Name);

                //var attribs = prop.GetCustomAttributes(typeof(IgnorePropertyAttribute), true);
                //var attr = attribs.FirstOrDefault() as IgnorePropertyAttribute;
                //if (attr == null || (attr != null && !attr.Value))
                //{
                //}
            }

            return paramNames;
        }
    } 
}