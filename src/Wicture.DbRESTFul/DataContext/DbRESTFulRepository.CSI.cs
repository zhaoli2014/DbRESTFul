using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Wicture.DbRESTFul.Converters;
using Wicture.DbRESTFul.Resources;

namespace Wicture.DbRESTFul
{
    public partial class DbRESTFulRepository
    {
        private static readonly Regex reg = new Regex(@"\[.*?\@.*?\]");
        private static readonly Regex pReg = new Regex(@"\@[a-zA-Z_]+\w*");

        /// <summary>
        /// Entry for the Configured Code Invocation.
        /// </summary>
        /// <param name="invokeName">The name of the csi code item.</param>
        /// <param name="parameters">The json format parameters.</param>
        /// <returns>The invocation result.</returns>
        public dynamic Invoke(string invokeName, object parameters)
        {
            CSIItem codeItem = GetCodeItemWithName(invokeName);
            using (var connection = codeItem.queryOnly ? connectionManager.ReadConnection : connectionManager.WriteConnection)
            {
                IDbTransaction transaction = null;
                connection.Open();

                if (codeItem.requiredTransaction)
                {
                    transaction = connection.BeginTransaction();
                }

                try
                {
                    var context = new InvokeContext
                    {
                        Repository = this,
                        CodeItem = codeItem,
                        Connection = connection,
                        Transaction = transaction
                    };
                    return InvokeInternal(context, parameters);
                }
                catch (Exception ex)
                {
                    if (codeItem.requiredTransaction)
                    {
                        transaction.Rollback();
                    }

                    throw new CSIInvocationException($"Invoke {invokeName} failed with parameters: {Environment.NewLine}{JsonConvert.SerializeObject(parameters)}", ex);
                }
            }
        }

        /// <summary>
        /// Invoke the Configured Code with specified connection.
        /// </summary>
        /// <param name="connection">The specified connection.</param>
        /// <param name="invokeName">The name of the csi code item.</param>
        /// <param name="parameters">The invocation parameters. Could be JObject, JArrary and string (json format).</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns>The invocation result.</returns>
        public dynamic InvokeWithConnection(IDbConnection connection, string invokeName, object parameters, IDbTransaction transaction = null)
        {
            CSIItem codeItem = GetCodeItemWithName(invokeName);
            var context = new InvokeContext
            {
                Repository = this,
                CodeItem = codeItem,
                Connection = connection,
                Transaction = transaction
            };

            return InvokeInternal(context, parameters, true);
        }

        private dynamic InvokeInternal(InvokeContext context, object parameters, bool withoutTransactionHandling = false)
        {
            if (parameters == null) { parameters = new JObject(); }
            // Converted param could be JObject or JArray.
            JToken convertedParam = ProcessParameters(parameters);
            context.Param = convertedParam;

            // Resolve middle wares for BeforeBuildCommandParam.
            MiddleWareFactory.Resolve(context, ActivePosition.BeforeBuildCommandParam);

            var commandParam = context.Param == null ? null : DynamicParametersBuilder.Build(context.Param);
            context.CommandParam = commandParam;

            // Fire the operating event.
            context.FireOperatingEvent();

            dynamic result = null;
            if (context.CodeItem.code is JObject)
            {
                JObject data = new JObject();
                var exportResultSet = context.CodeItem.resultSet != null ? context.CodeItem.resultSet.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : null;
                var resultIndex = 0;
                foreach (var code in context.CodeItem.code as JObject)
                {
                    var multipleResults = exportResultSet == null || exportResultSet.Length <= resultIndex || exportResultSet[resultIndex++].Equals("M", StringComparison.CurrentCultureIgnoreCase);
                    // Resolve middle wares for BeforeInvoke.
                    MiddleWareFactory.Resolve(context, ActivePosition.BeforeInvoke);
                    var itemResult = InvokeCodeInternal<dynamic>(context.CodeItem.code[code.Key].Value<string>(), context);
                    data.Add(code.Key, JToken.FromObject(multipleResults ? itemResult : itemResult.FirstOrDefault()));
                }

                result = data;
            }
            else
            {
                var multipleResults = string.IsNullOrEmpty(context.CodeItem.resultSet) || context.CodeItem.resultSet.Equals("M", StringComparison.CurrentCultureIgnoreCase);
                // Resolve middle wares for BeforeInvoke.
                MiddleWareFactory.Resolve(context, ActivePosition.BeforeInvoke);
                var data = InvokeCodeInternal<dynamic>(context.CodeItem.code.Value<string>(), context);
                result = multipleResults ? data : data.FirstOrDefault();
            }

            if (!withoutTransactionHandling && context.CodeItem.requiredTransaction && context.Transaction != null)
            {
                context.Transaction.Commit();
            }

            context.Result = result;

            // Resolve middle wares for AfterInvoke.
            MiddleWareFactory.Resolve(context, ActivePosition.AfterInvoke);

            context.FireOperatedEvent();

            return context.Result;
        }

        /// <summary>
        /// Invoke the specified code directly.
        /// </summary>
        /// <param name="code">The specific code.</param>
        /// <param name="parameters">The invocation parameters. Could be JObject, JArrary and string (json format).</param>
        /// <param name="queryOnly">Indicates if using readOnly database connection.</param>
        /// <param name="requiredTransaction">Indicates if using transaction.</param>
        /// <param name="multipleResults">Indicates the result set is array or single object.</param>
        /// <returns>The invocation result.</returns>
        public dynamic InvokeCode<T>(string code, object parameters, bool queryOnly = true, bool requiredTransaction = false, bool multipleResults = true)
        {
            if (typeof(IEnumerable).IsAssignableFrom(typeof(T)))
            {
                throw new Exception("Generic type '{0}' should not be an IEnumerable.".FormatWith(typeof(T)));
            }

            using (var connection = queryOnly ? connectionManager.ReadConnection : connectionManager.WriteConnection)
            {
                IDbTransaction transaction = null;
                connection.Open();

                if (requiredTransaction)
                {
                    transaction = connection.BeginTransaction();
                }

                return InvokeCodeWithConnection<T>(connection, code, parameters, queryOnly, transaction, multipleResults);
            }
        }

        /// <summary>
        /// Invoke the specified code with connection directly.
        /// </summary>
        /// <param name="conn">The specific connection.</param>
        /// <param name="code">The specific code.</param>
        /// <param name="parameters">The invocation parameters. Could be JObject, JArrary and string (json format).</param>
        /// <param name="queryOnly">Indicates if using readOnly database connection.</param>
        /// <param name="transaction">The specific transaction.</param>
        /// <param name="multipleResults">Indicates the result set is array or single object.</param>
        /// <returns>The invocation result.</returns>
        public dynamic InvokeCodeWithConnection<T>(IDbConnection conn, string code, object parameters, bool queryOnly = true, IDbTransaction transaction = null, bool multipleResults = true)
        {
            if (typeof(IEnumerable).IsAssignableFrom(typeof(T)))
            {
                throw new ArgumentException("Generic type '{0}' should not be an IEnumerable.".FormatWith(typeof(T)));
            }

            try
            {
                var context = new InvokeContext
                {
                    Repository = this,
                    CodeItem = CSIItem.Build(code, queryOnly, transaction != null),
                    Connection = conn,
                    Transaction = transaction
                };

                // Converted param could be JObject or JArray.
                JToken convertedParam = ProcessParameters(parameters);
                context.Param = convertedParam;

                var commandParam = convertedParam == null ? null : DynamicParametersBuilder.Build(convertedParam);
                context.CommandParam = commandParam;
                context.FireOperatingEvent(ObjectType.CodeInvocation);

                var result = InvokeCodeInternal<T>(code, context);

                if (transaction != null)
                {
                    transaction.Commit();
                }

                context.Result = multipleResults ? (dynamic)result : result.FirstOrDefault();
                context.FireOperatedEvent(ObjectType.CodeInvocation);

                return context.Result;
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }

                throw new CSIInvocationException($"Invoke code with connection was failed. {Environment.NewLine}code: `{code}` {Environment.NewLine}parameters: {JsonConvert.SerializeObject(parameters)}", ex);
            }
        }

        /// <summary>
        /// Invoke the specified code directly.
        /// </summary>
        /// <param name="code">The specific code.</param>
        /// <param name="parameters">The invocation parameters. Could be JObject, JArrary and string (json format).</param>
        /// <param name="queryOnly">Indicates if using readOnly database connection.</param>
        /// <param name="requiredTransaction">Indicates if using transaction.</param>
        /// <param name="multipleResults">Indicates the result set is array or single object.</param>
        /// <returns>The invocation result.</returns>
        public dynamic InvokeCode(string code, object parameters, bool queryOnly = true, bool requiredTransaction = false, bool multipleResults = true)
        {
            return InvokeCode<dynamic>(code, parameters, queryOnly, requiredTransaction, multipleResults);
        }

        /// <summary>
        /// Invoke the specified code with connection directly.
        /// </summary>
        /// <param name="conn">The specific connection.</param>
        /// <param name="code">The specific code.</param>
        /// <param name="parameters">The invocation parameters. Could be JObject, JArrary and string (json format).</param>
        /// <param name="queryOnly">Indicates if using readOnly database connection.</param>
        /// <param name="transaction">The specific transaction.</param>
        /// <param name="multipleResults">Indicates the result set is array or single object.</param>
        /// <returns>The invocation result.</returns>
        public dynamic InvokeCodeWithConnection(IDbConnection conn, string code, object parameters, bool queryOnly = true, IDbTransaction transaction = null, bool multipleResults = true)
        {
            return InvokeCodeWithConnection<dynamic>(conn, code, parameters, queryOnly, transaction, multipleResults);
        }

        private static IEnumerable<T> InvokeCodeInternal<T>(string code, InvokeContext context)
        {
            string sql = ProcessOptionalParameters(code, context);

            // Array Parameters is mainly used for insertion and update,
            // Which can only call the SqlMapper.Execute.
            IEnumerable<T> result;
            if (context.CommandParam is List<DynamicParameters>)
            {
                int efforts = SqlMapper.Execute(context.Connection, sql, context.CommandParam, context.Transaction, context.Repository.connectionManager.CommandTimeout);

                T data;
                try
                {
                    data = (T)Convert.ChangeType(efforts, typeof(T));
                    result = new List<T> { data };
                }
                catch
                {
                    throw new ArgumentException("Array parameters only support build-in primary return type: (int, decimal, double, string. etc.)");
                }
            }
            else
            {
                result = SqlMapper.Query<T>(context.Connection, sql, context.CommandParam, context.Transaction, true, context.Repository.connectionManager.CommandTimeout);
            }

            return result;
        }

        private static string ProcessOptionalParameters(string code, InvokeContext context)
        {
            var matches = reg.Matches(code);
            var sql = code;
            foreach (var match in matches)
            {
                var str = match.ToString();
                var containedAll = true;

                if (context.CommandParam != null)
                {
                    // Get first CommandParam if it's an array.
                    var commandParam = context.CommandParam is List<DynamicParameters>
                        ? (context.CommandParam as List<DynamicParameters>).FirstOrDefault()
                        : context.CommandParam as DynamicParameters;

                    foreach (var subMath in pReg.Matches(str))
                    {
                        if (!containedAll)
                            break;
                        var exist = false;

                        foreach (var p in commandParam.ParameterNames)
                        {
                            if (subMath.ToString() == "@" + p)
                            {
                                exist = true;
                                break;
                            }
                        }

                        containedAll = containedAll && exist;
                    }
                }
                else
                {
                    containedAll = false;
                }

                sql = containedAll
                    ? sql.Replace(str, str.Replace("[", "").Replace("]", ""))
                    : sql.Replace(str, "");
            }

            return sql;
        }

        private static JToken ProcessParameters(object parameters)
        {
            JToken token = null;

            try
            {
                if (parameters != null)
                {
                    token = parameters is JToken
                        ? parameters as JToken
                        : (parameters is string) ? JToken.Parse(parameters.ToString()) : JToken.FromObject(parameters);
                }

                ProcessPaginationParameters(token);

                return token;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                {
                    throw;
                }

                throw new ArgumentException($"The expected parameters should be JObject, JArrary and string (json format).", ex);
            }
        }

        private static void ProcessPaginationParameters(JToken param)
        {
            // Only if the parameter is jObject need to process.
            if (param != null && param is JObject)
            {
                var jObj = param as JObject;
                try
                {
                    var pageSize = jObj.Value<int>("pageSize");
                    var pageIndex = jObj.Value<int>("pageIndex");
                    if (pageSize > 0 && pageIndex == 0)
                    {
                        throw new Exception("'pageIndex' must be greater than 0");
                    }
                    // Only if the pageSize and pageIndex parameters set with validated values.
                    else if (pageSize > 0 && pageIndex > 0)
                    {
                        jObj["pageSize"] = pageSize; // Make sure it is int type.
                        jObj["pageStart"] = pageSize * (pageIndex - 1);
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Parameter for pagination is incorrect.", ex);
                }
            }
        }

        private static CSIItem GetCodeItemWithName(string invokeName)
        {
            if (!ServiceResourceManager.CSIs.ContainsKey(invokeName))
            {
                throw new CSINotFoundException("The csi is not found with name: " + invokeName);
            }

            return CSIItem.Clone(ServiceResourceManager.CSIs[invokeName]);
        }
    }
}

