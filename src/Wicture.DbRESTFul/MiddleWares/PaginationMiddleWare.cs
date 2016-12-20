using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wicture.DbRESTFul.MiddleWares
{
    /// <summary>
    /// 分页中间件，对查询结果做自动分页支持。
    /// </summary>
    public class PaginationMiddleWare : IMiddleWare
    {
        public string Name { get { return "pagination"; } }
        public ActivePosition ActivePosition { get { return ActivePosition.AfterInvoke; } }

        public string Validate(InvokeContext context, JToken config)
        {
            var message = string.Empty;
            if (!(config is JObject))
            {
                message = "config type should be an object.";
            }

            if (!(context.CodeItem.code is JValue))
            {
                var prefix = string.IsNullOrEmpty(message) ? string.Empty : Environment.NewLine;
                message += prefix + "pagination middle ware only support single query code.";
            }

            if (!(context.Param is JObject))
            {
                var prefix = string.IsNullOrEmpty(message) ? string.Empty : Environment.NewLine;
                message += prefix + "parameter type should be an object.";
            }
            else
            {
                var setting = context.Param as JObject;
                if (string.IsNullOrEmpty(setting.Value<string>("pageSize"))
                    || string.IsNullOrEmpty(setting.Value<string>("pageIndex"))
                    || string.IsNullOrEmpty(setting.Value<string>("pageStart")))
                {
                    var prefix = string.IsNullOrEmpty(message) ? string.Empty : Environment.NewLine;
                    message = prefix + "pageSize, pageIndex or pageStart is not set.";
                }
            }

            return message;
        }

        /// <summary>
        /// Code could be:
        ///     "SELECT u.`name`, u.score FROM user AS u WHERE u.id > @userId limit @pageStart, @pageSize",
        /// Need to convert:
        ///     "SELECT count(u.`name`) as count, @pageSize as size, @pageIndex as `index` FROM user AS u WHERE u.id > @userId"
        /// </summary>
        /// <param name="context"></param>
        /// <param name="config">
        /// {
        ///     "size": "pageSize",
        ///     "count": "totalCount",
        ///     "page": "pageIndex"
        /// }
        /// </param>
        public void Resolve(InvokeContext context, JToken config)
        {
            var setting = config as JObject;
            var countPropertyName = string.IsNullOrEmpty(setting.Value<string>("count")) ? "count" : setting.Value<string>("count");
            var sizePropertyName = string.IsNullOrEmpty(setting.Value<string>("size")) ? "size" : setting.Value<string>("size");
            var pagePropertyName = string.IsNullOrEmpty(setting.Value<string>("page")) ? "page" : setting.Value<string>("page");

            var paginationSql = ConvertPaginationCode(context.CodeItem.code.Value<string>(), countPropertyName, sizePropertyName, pagePropertyName);
            IEnumerable<object> pagination = context.Repository.InvokeCodeWithConnection(context.Connection, paginationSql, context.Param);

            context.Result = new { items = context.Result, pagination = pagination.FirstOrDefault() };
        }

        /// <summary>
        /// Convert query code to pagination code.
        /// </summary>
        /// <param name="code">
        /// Code could be like:
        ///     "SELECT u.`name`, u.score FROM user AS u WHERE u.id > @userId ORDER  BY u.id LIMIT @pageStart, @pageSize",
        /// </param>
        /// <returns>
        /// Converted code could be like:
        ///     "SELECT count(u.`name`) as count, @pageSize as size, @pageIndex as `index` FROM user AS u WHERE u.id > @userId"
        /// </returns>
        private string ConvertPaginationCode(string code, string countPropertyName, string sizePropertyName, string pagePropertyName)
        {
            if (ConfigurationManager.Settings.CSI.DatabaseType == DatabaseType.MySQL)
            {
                // Get code copy for `FROM ... WHERE ....`,
                var fromAndWhere = GetFromAndWhereForMySQL(code);

                return $"SELECT count(1) AS `{countPropertyName}`, @pageSize AS `{sizePropertyName}`, @pageIndex AS `{pagePropertyName}` {fromAndWhere}";
            }
            else
            {
                var fromAndWhere = GetFromAndWhereForSQLServer(code);
                return $"SELECT count(1) AS {countPropertyName}, @pageSize AS {sizePropertyName}, @pageIndex AS {pagePropertyName} {fromAndWhere} ";
            }
        }

        /// <summary>
        /// 去除字符串头尾的圆括号对
        /// </summary>
        private string trimParentheses(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            if (str.StartsWith("(") && str.EndsWith(")"))
                return trimParentheses(str.Substring(1, str.Length - 2));
            else
                return str;
        }

        /// <summary>
        /// 判断在某位置紧接的前面是否有奇数的反斜杠，如\\\'，在'前就有奇数的\
        /// </summary>
        private bool hasOddBackslashBefore(string str, int index)
        {
            var count = 0;
            for (var i = index - 1; i > -1; i--)
            {
                if (str[i] == '\\')
                {
                    count++;
                }
                else
                    break;
            }
            return count % 2 == 1;
        }

        /// <summary>
        /// 获取主查询语句片断，去除子查询及字符串。去除之前执行了Trim()
        /// </summary>
        private string GetValidMainSqlClip(string str)
        {
            str = trimParentheses(str.Trim());
            var validChars = new char[str.Length];
            var quote = ' ';
            var parenthesesCount = 0;
            for (var i = 0; i < str.Length; i++)
            {
                validChars[i] = ' ';
                if ((str[i] == '\"' || str[i] == '\'') && !hasOddBackslashBefore(str, i))
                {
                    if (quote == ' ')
                        quote = str[i];
                    else
                        quote = ' ';
                    continue;
                }
                if (quote == ' ')
                {
                    if (str[i] == '(')
                        parenthesesCount++;
                    else if (str[i] == ')')
                        parenthesesCount--;
                    else if(parenthesesCount == 0)
                        validChars[i] = str[i];
                }
            }
            return new string(validChars);
        }

        private string GetFromAndWhereForMySQL(string code)
        {
            string str = code.Trim();

            string validMainSqlClip = GetValidMainSqlClip(str).ToUpperInvariant();

            var keywords = "ORDER,LIMIT";
            var minIndex = keywords.Split(',').Select(kw => validMainSqlClip.IndexOf(" " + kw + " ")).Where(i => i > 0).Min();
            var startIndex = validMainSqlClip.IndexOf(" FROM ");

            return str.Substring(startIndex, minIndex - startIndex).Trim();
        }

        private string GetFromAndWhereForSQLServer(string code)
        {
            string str = code.Trim();

            string validMainSqlClip = GetValidMainSqlClip(str).ToUpperInvariant();

            var keywords = "ORDER,OFFSET";
            var minIndex = keywords.Split(',').Select(kw => validMainSqlClip.IndexOf(" " + kw + " ")).Where(i => i > 0).Min();
            var startIndex = validMainSqlClip.IndexOf(" FROM ");

            return str.Substring(startIndex, minIndex - startIndex).Trim();
        }
    }
}
