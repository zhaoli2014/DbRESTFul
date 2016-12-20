using Dapper;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wicture.DbRESTFul
{
    public partial class DbRESTFulRepository : IPermissionChecker
    {
        private readonly SQLBuilder builder;
        protected readonly ConnectionManager connectionManager;
        private IdentityInfo currentUser;

        public DbRESTFulRepository()
        {
            connectionManager = new ConnectionManager();
            builder = new SQLBuilder(connectionManager.ReadConnectionString);
        }

        public DbRESTFulRepository(ConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;
            builder = new SQLBuilder(connectionManager.ReadConnectionString);
        }

        public IdentityInfo CurrentUser
        {
            get
            {
                if (currentUser == null)
                {
                    currentUser = new IdentityInfo();
                }
                return currentUser;
            }
            set
            {
                if (value != currentUser && value != null)
                {
                    currentUser = value;
                    connectionManager.UseGateway(currentUser.GatewayBag);
                }
            }
        }

        public HttpContext HttpContext { get; set; }

        public object Query(string tableName, string parameters)
        {
            OperationHandler.OnOperating(new OperationArgs(CurrentUser.Id, TableOperation.Select, ObjectType.TableOrView, tableName, parameters));

            using (var cnn = connectionManager.ReadConnection)
            {
                var countSqlSB = new StringBuilder();
                var pagination = new Pagination();
                var sql = builder.BuildSelectSQL(tableName, parameters, countSqlSB, pagination);
                var data = SqlMapper.Query(cnn, sql);
                object result = null;

                if (countSqlSB.Length > 0)
                {
                    var count = SqlMapper.Query<int>(cnn, countSqlSB.ToString()).FirstOrDefault();
                    pagination.count = count;
                    result = new PaginationListViewModel<object>() { items = data, pagination = pagination };
                }
                else
                {
                    result = data;
                }

                OperationHandler.OnOperated(new OperationArgs(CurrentUser.Id, TableOperation.Select, ObjectType.TableOrView, tableName, parameters, result, true));

                return data;
            }

        }

        public int? Insert(string tableName, string parameters)
        {
            OperationHandler.OnOperating(new OperationArgs(CurrentUser.Id, TableOperation.Insert, ObjectType.TableOrView, tableName, parameters));

            using (var cnn = connectionManager.WriteConnection)
            {
                var sql = builder.BuildInsertSQL(tableName, parameters);
                var result = SqlMapper.Execute(cnn, sql);

                OperationHandler.OnOperated(new OperationArgs(CurrentUser.Id, TableOperation.Insert, ObjectType.TableOrView, tableName, parameters, result));
                return result;
            }
        }

        public int Update(string tableName, string where, string parameters)
        {
            OperationHandler.OnOperating(new OperationArgs(CurrentUser.Id, TableOperation.Update, ObjectType.TableOrView, tableName, parameters));

            using (var cnn = connectionManager.WriteConnection)
            {
                object data;
                var sql = builder.BuildUpdateSQL(tableName, where, parameters, out data);
                var result = SqlMapper.Execute(cnn, sql, data);
                OperationHandler.OnOperated(new OperationArgs(CurrentUser.Id, TableOperation.Update, ObjectType.TableOrView, tableName, parameters, result));
                return result;
            }
        }

        public int Delete(string tableName, string parameters)
        {
            OperationHandler.OnOperating(new OperationArgs(CurrentUser.Id, TableOperation.Delete, ObjectType.TableOrView, tableName, parameters));
            using (var cnn = connectionManager.WriteConnection)
            {
                var sql = builder.BuildDeleteSQL(tableName, parameters);
                var result = SqlMapper.Execute(cnn, sql);
                OperationHandler.OnOperated(new OperationArgs(CurrentUser.Id, TableOperation.Delete, ObjectType.TableOrView, tableName, parameters, result));
                return result;
            }
        }


        #region Permission TODO

        public bool AddOrUpdatePermission(int userId, string tableName, bool canList, bool canInsert, bool canUpdate, bool canDelete)
        {
            using (var cnn = connectionManager.WriteConnection)
            {
                var data = SqlMapper.Query<dynamic>(cnn, "select * from UserTablePermission where `UserId`=" + userId + " and `TableName`='" + tableName + "';");
                if (data == null || data.Count() == 0)
                {
                    SqlMapper.Query(cnn, string.Format("insert into UserTablePermission(`UserId`, `TableName`, `List`, `Insert`, `Update`, `Delete`) values ({0}, '{1}', {2}, {3}, {4}, {5});", userId, tableName, (canList ? 1 : 0), (canInsert ? 1 : 0), (canUpdate ? 1 : 0), (canDelete ? 1 : 0)));
                    return true;
                }
                else
                {
                    SqlMapper.Query(cnn, string.Format("update UserTablePermission set  `List` = {0}, `Insert` = {1}, `Update` = {2}, `Delete` = {3} where `Id` = {4};", (canList ? 1 : 0), (canInsert ? 1 : 0), (canUpdate ? 1 : 0), (canDelete ? 1 : 0), data.First().Id));
                    return true;
                }
            }
        }

        public bool DeletePermission(int userId, string tableName)
        {
            using (var cnn = connectionManager.WriteConnection)
            {
                SqlMapper.Query(cnn, string.Format("delete from UserTablePermission where `UserId` = {0} and `TableName` = '{1}';", userId, tableName));
                return true;
            }
        }

        public object ListPermission(int? userId, string tableName)
        {
            using (var cnn = connectionManager.ReadConnection)
            {
                var sql = "select * from UserTablePermission where 1=1";
                if (userId > 0)
                {
                    sql += " and `UserId`=" + userId;
                }
                if (!string.IsNullOrWhiteSpace(tableName))
                {
                    sql += " and `TableName`='" + tableName + "'";
                }
                return SqlMapper.Query(cnn, sql);
            }
        }


        #endregion

        public bool HasPermission(string obj, object userId, TableOperation operation, ObjectType type = ObjectType.TableOrView)
        {
            if (string.IsNullOrEmpty("obj"))
            {
                throw new ArgumentNullException("obj");
            }

            using (var cnn = connectionManager.ReadConnection)
            {
                var param = new { obj = obj, userId = userId, type = (int)type };
                var flag = SqlMapper.Query<int>(cnn, "SELECT flag FROM apiPermission WHERE object = @obj AND type = @type AND roleId = (SELECT roleId FROM user WHERE id = @userId)", param).FirstOrDefault();
                return flag > 0 && flag.Has(operation);
            }
        }
        
        public object Execute(string procedureName, string parameters)
        {
            OperationHandler.OnOperating(new OperationArgs(CurrentUser.Id, TableOperation.Execute, ObjectType.StoreProcedure, procedureName, parameters));

            if (string.IsNullOrEmpty(procedureName))
            {
                throw new ArgumentNullException("procedureName");
            }

            using (var cnn = connectionManager.WriteConnection)
            {
                object param = null;
                if (!string.IsNullOrEmpty(parameters))
                {
                    var obj = JsonConvert.DeserializeObject(parameters) as JObject;
                    param = DynamicParametersBuilder.Build(obj);
                }

                var result = SqlMapper.Query(cnn, procedureName, param, null, false, null, System.Data.CommandType.StoredProcedure);

                OperationHandler.OnOperated(new OperationArgs(CurrentUser.Id, TableOperation.Execute, ObjectType.StoreProcedure, procedureName, parameters, result));

                return result;
            }
        }

        protected List<JObject> BuildTree<Tkey>(JArray data, string childrenKey = "children", string idKey = "id", string parentIdKey = "parentId", string childrenCountKey = "childrenCount")
        {
            var source = data.OfType<JObject>().ToList();

            while (true)
            {
                var last = source.LastOrDefault();
                var parent = source.FirstOrDefault(o => o.Value<Tkey>(idKey).Equals(last.Value<Tkey>(parentIdKey)));

                if (parent == null)
                {
                    break;
                }

                var siblings = source.Where(o => o.Value<Tkey>(parentIdKey).Equals(last.Value<Tkey>(parentIdKey)));
                var count = siblings.Count();
                parent[childrenKey] = JToken.FromObject(new List<JObject>(siblings));
                parent[childrenCountKey] = count;
                source.RemoveRange(source.Count - count, count);
            }

            return source;
        }
    }
}

