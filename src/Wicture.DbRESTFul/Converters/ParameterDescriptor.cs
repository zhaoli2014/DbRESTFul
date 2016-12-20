using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wicture.DbRESTFul.Converters
{
    internal static class ParameterParser
    {
        public static OperationParameter GetOperationParameter(JObject token)
        {
            var result = new OperationParameter();

            if (token != null)
            {
                foreach (var item in token)
                {
                    Parse(item.Key, item.Value, result);
                }
            }

            return result;
        }

        private static void Parse(string key, JToken value, OperationParameter result)
        {
            if (key == PaginationParameter.Key)
            {
                var param = value as JArray;
                if (param == null || param.Count != 2)
                {
                    throw new ParameterParseException("The pagination shuold have page index and page size parameters.");
                }
                result.PaginationParameter = new PaginationParameter()
                {
                    Size = param[0].Value<int>(),
                    Index = param[1].Value<int>()
                };
            }
            else if (key == OrderParameter.Key)
            {
                List<IColumnObject> colObjs = new List<IColumnObject>();
                var cols = value as JArray;
                if (cols != null && cols.Count > 0)
                {
                    for (var i = 0; i < cols.Count; i++)
                    {
                        var col = cols[i];
                        if (col is JValue)
                        {
                            colObjs.Add(new SimpleColumnObjectInOrder() { Name = col.Value<string>(), IsDesc = false });
                        }
                        else
                        {
                            var complexCol = col as JObject;
                            if (complexCol == null || string.IsNullOrEmpty(complexCol.Value<string>("$name")))
                            {
                                throw new Exception("Order指令中复杂字段对象必须包含$name属性");
                            }
                            else
                            {
                                var item = new ComplexColumnObjectInOrder()
                                {
                                    Name = complexCol["$name"].ToString()
                                };
                                if (!string.IsNullOrEmpty(complexCol.Value<string>("$aggregation")))
                                {
                                    item.Aggregation = complexCol.Value<string>("$aggregation").ToAggregationType();
                                    if (item.Aggregation == null)
                                    {
                                        throw new Exception("聚合指令无法识别！");
                                    }
                                }
                                if (!string.IsNullOrEmpty(complexCol.Value<string>("$desc")))
                                {
                                    item.IsDesc = complexCol.Value<bool>("$desc");
                                }
                                colObjs.Add(item);
                            }
                        }
                    }
                    result.OrderParameter = new OrderParameter(colObjs);
                }
            }
            else if (key == OperationParameter.WhereParameterKey)
            {
                var conditions = value as JObject;
                if (conditions.Count != 1)
                {
                    throw new Exception("Where条件语法错误，只能使用一个组合查询组合或者一个基本条件指令！");
                }
                else if (conditions.Count == 1)
                {
                    var conKey = (conditions as IDictionary<string, JToken>).Keys.First();
                    if (BaseConditionParameter.IsConditionParameter(conKey))
                    {
                        result.WhereParameter = BaseConditionParameter.TranslateByDictionary(conKey, conditions[conKey], false);
                    }
                    else if (LinkConditionParameter.IsLinkConditionParameter(conKey))
                    {
                        result.WhereParameter = LinkConditionParameter.TranslateByDictionary(conKey, conditions[conKey], false);
                    }
                    else
                    {
                        throw new Exception("Where条件语法错误，条件指令未能识别！");
                    }
                }
            }
            else if (key == OperationParameter.GroupByParameterKey)
            {
                var cols = value as JArray;
                if (cols == null)
                {
                    throw new Exception("GroupBy语法错误，内容为字段字符串数组！");
                }
                var list = cols.Select(j => j.ToString()).ToList();
                result.GroupByParameter = list;
            }
            else if (key == OperationParameter.HavingParameterKey)
            {
                var conditions = value as JObject;
                if (conditions.Count != 1)
                {
                    throw new Exception("Having条件语法错误，只能使用一个组合查询组合或者一个基本条件指令！");
                }
                else if (conditions.Count == 1)
                {
                    var conKey = (conditions as IDictionary<string, JToken>).Keys.First();
                    if (BaseConditionParameter.IsConditionParameter(conKey))
                    {
                        result.HavingParameter = BaseConditionParameter.TranslateByDictionary(conKey, conditions[conKey], true);
                    }
                    else if (LinkConditionParameter.IsLinkConditionParameter(conKey))
                    {
                        result.HavingParameter = LinkConditionParameter.TranslateByDictionary(conKey, conditions[conKey], true);
                    }
                    else
                    {
                        throw new Exception("Having条件语法错误，条件指令未能识别！");
                    }
                }
            }
            else if (key == OperationParameter.IncludesParameterKey)
            {
                var colObjs = value as JArray;
                if (colObjs == null)
                {
                    throw new Exception("Includes字段筛选语法错误，内容只能是字段对象数组！");
                }
                if (colObjs.Count > 0)
                {
                    if (result.ExcludesColumnParameter != null)
                        throw new Exception("Includes不能与Excludes同时使用！");
                    var cols = new List<IColumnObject>();
                    for (var i = 0; i < colObjs.Count; i++)
                    {
                        var colObj = colObjs[i];
                        if (colObj is JValue)
                        {
                            cols.Add(new SimpleColumnObject() { Name = colObj.Value<string>() });
                        }
                        else
                        {
                            var colDic = colObj as JObject;
                            if (colDic == null || string.IsNullOrEmpty(colDic.Value<string>("$name")))
                                throw new Exception("Includes中字段对象定义错误，如果是复杂字段对象，必须包含$name属性！");
                            var cc = new ComplexColumnObject();
                            cc.Name = colDic.Value<string>("$name");
                            if (!string.IsNullOrEmpty(colDic.Value<string>("$distinct")))
                                cc.IsDistinct = colDic.Value<bool>("$distinct");
                            if (!string.IsNullOrEmpty(colDic.Value<string>("$aggregation")))
                            {
                                cc.Aggregation = colDic.Value<string>("$aggregation").ToAggregationType();
                                if (cc.Aggregation == null)
                                    throw new Exception("Includes字段对象中存在未能识别的$aggregation指令！");
                            }
                            if (!string.IsNullOrEmpty(colDic.Value<string>("$alias")))
                                cc.Alias = colDic.Value<string>("$alias");
                            cols.Add(cc);
                        }
                    }
                    result.IncludesColumnParameter = cols;
                }
            }
            else if (key == OperationParameter.ExcludesParameterKey)
            {
                var colObjs = value as JArray;
                if (colObjs == null)
                {
                    throw new Exception("Excludes字段筛选语法错误，内容只能是字段对象数组！");
                }
                if (colObjs.Count > 0)
                {
                    if (result.IncludesColumnParameter != null)
                        throw new Exception("Excludes不能与Includes同时使用！");
                    var cols = new List<SimpleColumnObject>();
                    for (var i = 0; i < colObjs.Count; i++)
                    {
                        var colObj = colObjs[i];
                        if (colObj is JArray)
                        {
                            cols.Add(new SimpleColumnObject() { Name = colObj.Value<string>() });
                        }
                        else
                        {
                            throw new Exception("Excludes后面的数组只能是字段名称，不支持复杂的字段对象");
                        }
                    }
                    result.ExcludesColumnParameter = cols;
                }
            }
            else
            {
                throw new ParameterParseException("Unknown parameter.");
            }
        }
    }
}