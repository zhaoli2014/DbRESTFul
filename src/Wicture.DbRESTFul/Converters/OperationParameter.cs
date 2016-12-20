using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Wicture.DbRESTFul.Converters
{
    internal class OperationParameter
    {
        public static readonly string WhereParameterKey = "$where";
        public static readonly string GroupByParameterKey = "$groupby";
        public static readonly string HavingParameterKey = "$having";
        public static readonly string IncludesParameterKey = "$includes";
        public static readonly string ExcludesParameterKey = "$excludes";

        public IConditionParameter WhereParameter { get; set; }

        public IList<string> GroupByParameter { get; set; }

        public IConditionParameter HavingParameter { get;set; }

        public IList<IColumnObject> IncludesColumnParameter { get; set; }
        public IList<SimpleColumnObject> ExcludesColumnParameter { get; set; }

        public OrderParameter OrderParameter { get; set; }
        public PaginationParameter PaginationParameter { get; set; }
    }

    public interface IColumnObject { }

    public interface IConditionRightSideObject
    {
    }

    public static class TranslateStringExtension
    {
        public static string ToMySQLString(this string val, bool inLike = false)
        {
            if (!string.IsNullOrEmpty(val))
            {
                val = val.Replace("'", @"\'").Replace("\"", "\\\"").Replace("\\", "\\\\");
                if (inLike)
                {
                    val = val.Replace("%", "\\%").Replace("_", "\\_");
                }
            }
            return val;
        }
    }

    public class ValueObject : IConditionRightSideObject
    {
        public object Value { get; set; }

        public bool InLike { get; set; }

        public override string ToString()
        {
            if (Value == null)
                return "null";
            if (Value is int || Value is long || Value is float || Value is double || Value is byte || Value is decimal ||
                Value is uint || Value is ulong || Value is ushort || Value is short || Value is SByte)
                return Value.ToString();
            if (Value is DateTime)
                return "'" + Value.DateFormat("yyyy-MM-dd HH:mm:ss") + "'";
            return "'" + Value.ToString().ToMySQLString(InLike) + "'";
        }

    }

    public class SimpleColumnObject : IColumnObject, IConditionRightSideObject
    {
        public string Name { get; set; }

        public bool IsStringFormat { get; set; }

        public override string ToString()
        {
            return IsStringFormat ? Name : "`" + Name + "`";
        }
    }

    public class SimpleColumnObjectInOrder : SimpleColumnObject
    {
        public bool IsDesc { get; set; }

        public override string ToString()
        {
            return base.ToString() + (IsDesc ? " DESC" : " ASC");
        }
    }

    public class ComplexColumnObject : SimpleColumnObject
    {
        public string Alias { get; set; }
        public bool IsDistinct { get; set; }
        public AggregationType? Aggregation { get; set; }

        public override string ToString()
        {
            if (Aggregation == null)
            {
                return base.ToString() + (string.IsNullOrWhiteSpace(Alias) ? "" : (" AS `" + Alias + "`"));
            }
            else
            {
                return Aggregation.ToAggregationString() + "(" + base.ToString() + ")" + (string.IsNullOrWhiteSpace(Alias) ? "" : (" AS `" + Alias + "`"));
            }
        }
    }

    public class ComplexColumnObjectInOrder : ComplexColumnObject
    {
        public bool IsDesc { get; set; }

        public override string ToString()
        {
            return base.ToString() + (IsDesc ? " DESC" : " ASC");
        }
    }

    public interface IConditionParameter
    {

    }

    public class BaseConditionParameter : IConditionParameter
    {
        private static readonly List<Tuple<ConditionType, string, string>> typeMapping;

        public static BaseConditionParameter TranslateByDictionary(string type, object dicObj, bool canUseAggregation)
        {
            var dic = dicObj as IDictionary<string, JToken>;
            if (dic == null)
                throw new Exception("条件指令内容错误！");
            IColumnObject firstCO = null;
            IConditionRightSideObject secondCRSO = null;
            object value = null;
            if (dic.ContainsKey("$key"))
            {
                //复杂形式
                if (!dic.ContainsKey("$value"))
                {
                    throw new Exception("条件指令复杂形式必须同时设置$key与$value值");
                }
                if (!(dic["$key"] is IDictionary<string, object>))
                {
                    throw new Exception("条件指令复杂形式$key必须为字段对象！");
                }
                var firstDic = dic["$key"] as IDictionary<string, object>;
                if (!firstDic.ContainsKey("$name"))
                {
                    throw new Exception("条件指令复杂形式$key对应的字段对象必须包含$name！");
                }
                
                if (canUseAggregation)
                {
                    var cco = new ComplexColumnObject()
                    {
                        Name = firstDic["$name"].ToString()
                    };
                    if(firstDic.ContainsKey("$aggregation") && firstDic["$aggregation"] != null){
                         cco.Aggregation  = firstDic["$aggregation"].ToString().ToAggregationType();
                        if(cco.Aggregation == null)
                            throw new Exception("聚合指令无法识别！");
                    }
                    if (firstDic.ContainsKey("$distinct") && firstDic["$distinct"] != null)
                        cco.IsDistinct = firstDic["$distinct"].ToString().ToLower().Equals("false");
                    firstCO = cco;
                }
                else
                {
                    firstCO = new SimpleColumnObject()
                    {
                        Name = firstDic["$name"].ToString()
                    };
                }

                value = dic["$value"];
                
            }
            else
            {
                //简单形式
                if (dic.Keys.Count != 1)
                {
                    throw new Exception("条件指令简单形式对应的对象中不支持多字段的判断！");
                }
                firstCO = new SimpleColumnObject() { Name = dic.Keys.First() };
                value = dic.First().Value;
            }

            if (value is IDictionary<string, object>)
            {
                var secondDic = value as IDictionary<string, object>;
                if (canUseAggregation)
                {
                    var cco = new ComplexColumnObject()
                    {
                        Name = secondDic["$name"].ToString()
                    };
                    if (secondDic.ContainsKey("$aggregation") && secondDic["$aggregation"] != null)
                    {
                        cco.Aggregation = secondDic["$aggregation"].ToString().ToAggregationType();
                        if (cco.Aggregation == null)
                            throw new Exception("聚合指令无法识别！");
                    }
                    if (secondDic.ContainsKey("$distinct") && secondDic["$distinct"] != null)
                        cco.IsDistinct = secondDic["$distinct"].ToString().ToLower().Equals("false");
                    secondCRSO = cco;
                }
                else
                {
                    secondCRSO = new SimpleColumnObject()
                    {
                        Name = secondDic["$name"].ToString()
                    };
                }
            }
            else
            {
                secondCRSO = new ValueObject() { Value = value };
            }

            return new BaseConditionParameter(type, firstCO, secondCRSO);
        }

        static BaseConditionParameter()
        {
            typeMapping = new List<Tuple<ConditionType, string, string>>()
            {
                new Tuple<ConditionType, string, string>(ConditionType.Equal, "$eq", "{0} = {1}"),
                new Tuple<ConditionType, string, string>(ConditionType.NotEqual, "$neq", "{0} <> {1}"),
                new Tuple<ConditionType, string, string>(ConditionType.GreaterThan, "$gt", "{0} > {1}"),
                new Tuple<ConditionType, string, string>(ConditionType.GreaterThanEqual, "$gte", "{0} >= {1}"),
                new Tuple<ConditionType, string, string>(ConditionType.LessThan, "$lt", "{0} < {1}"),
                new Tuple<ConditionType, string, string>(ConditionType.LessThanEqual, "$lte", "{0} <= {1}"),
                new Tuple<ConditionType, string, string>(ConditionType.In, "$in", "{0} IN ({1})"),
                new Tuple<ConditionType, string, string>(ConditionType.NotIn, "$nin", "{0} NOT IN ({1})"),
                new Tuple<ConditionType, string, string>(ConditionType.Between, "$btn", "{0} BETWEEN {1} AND {2}"),
                new Tuple<ConditionType, string, string>(ConditionType.NotBetween, "$nbtn", "{0} NOT BETWEEN {1} AND {2}"),
                new Tuple<ConditionType, string, string>(ConditionType.Like, "$lk", "{0} LIKE '%{1}%'"),
                new Tuple<ConditionType, string, string>(ConditionType.LikeLeft, "$lkl", "{0} LIKE '{1}%'"),
                new Tuple<ConditionType, string, string>(ConditionType.LikeRight, "$lkr", "{0} LIKE '%{1}'"),
                new Tuple<ConditionType, string, string>(ConditionType.NotLike, "$nlk", "{0} NOT LIKE '%{1}%'"),
                new Tuple<ConditionType, string, string>(ConditionType.NotLikeLeft, "$nlkl", "{0} NOT LIKE '{1}%'"),
                new Tuple<ConditionType, string, string>(ConditionType.NotLikeRight, "$nlkr", "{0} NOT LIKE '%{1}'"),
            };
        }

        public static bool IsConditionParameter(string key)
        {
            return typeMapping.Any(a => a.Item2 == key);
        }

        private string stringFormat = string.Empty;
        public BaseConditionParameter(string type, IColumnObject first, IConditionRightSideObject second)
        {
            var item = typeMapping.First(t => t.Item2 == type);

            if (item == null)
            {
                throw new ParameterParseException(string.Format("Unknown condition type [{0}].", type));
            }

            ConditionType = item.Item1;
            stringFormat = item.Item3;
            First = first;
            Second = second;
        }

        public ConditionType ConditionType { get; private set; }

        public IColumnObject First { get; set; }
        public IConditionRightSideObject Second { get; set; }

        public override string ToString()
        {
            var item = typeMapping.First(t => t.Item1 == ConditionType);

            if (ConditionType == ConditionType.Between || ConditionType == ConditionType.NotBetween)
            {
                if (Second as ValueObject == null || (Second as ValueObject).Value as ArrayList == null)
                    throw new Exception("Between或者NotBetween值的语法错误！");
                var data = (Second as ValueObject).Value as ArrayList;
                if (data.Count != 2)
                {
                    throw new ParameterParseException(string.Format("Between or NotBetween condittion requires 2 values, but actual values count is: {0}", data.Count));
                }
                return string.Format(item.Item3, First, new ValueObject() { Value = data[0] }, new ValueObject() { Value = data[1] });
            }
            else if (ConditionType == ConditionType.In || ConditionType == ConditionType.NotIn)
            {
                if (Second as ValueObject == null || (Second as ValueObject).Value as ArrayList == null)
                    throw new Exception("In或者NotIn值的语法错误！");
                var arrayString = string.Join(",", ((Second as ValueObject).Value as ArrayList).ToArray().Select(e => new ValueObject() { Value = e }));
                return string.Format(item.Item3, First, arrayString);
            }
            else if (item.Item2.Contains("lk"))
            {
                if (Second as ValueObject == null)
                    throw new Exception("Like值的语法错误！");
                (Second as ValueObject).InLike = true;
                return string.Format(item.Item3, First.ToString().TrimSingleQuotes(), Second.ToString().TrimSingleQuotes());
            }

            return string.Format(item.Item3, First, Second);
        }
    }

    public class LinkConditionParameter : IConditionParameter
    {
        public LinkConditionType LinkCondition { get; set; }

        public List<IConditionParameter> Parameters { get; set; }

        public static bool IsLinkConditionParameter(string key)
        {
            return key.Equals("$or") || key.Equals("$and") || key.Equals("$not");
        }

        public static IConditionParameter TranslateByDictionary(string type, object list, bool canUseAggregation)
        {
            var lcp = new LinkConditionParameter();
            lcp.LinkCondition = type.ToLinkConditionType().Value;
            lcp.Parameters = new List<IConditionParameter>();
            var vList = list as JArray;
            if (vList == null)
            {
                throw new Exception("组合条件内容不正确！");
            }

            for (var i = 0; i < vList.Count; i++)
            {
                var item = vList[i] as JObject;
                if (item == null || item.Count != 1)
                {
                    throw new Exception("条件组合内容不正确！");
                }

                var key = (item as IDictionary<string, JToken>).Keys.First();
                if (IsLinkConditionParameter(key))
                {
                    lcp.Parameters.Add(TranslateByDictionary(key, item[key], canUseAggregation));
                }
                else if(BaseConditionParameter.IsConditionParameter(key))
                {
                    lcp.Parameters.Add(BaseConditionParameter.TranslateByDictionary(key, item[key], canUseAggregation));
                }
                else
                {
                    throw new Exception("无法识别条件指令！");
                }
            }
            return lcp;
        }

        public override string ToString()
        {
            if (Parameters == null || Parameters.Count == 0)
                throw new Exception("条件组合连接词必须包含至少一个条件！");
            switch (LinkCondition)
            {
                case LinkConditionType.Not:
                    if (Parameters.Count != 1)
                        throw new Exception("Not连接词有且只有一个条件！");
                    else
                    {
                        return "NOT (" + Parameters[0].ToString() + ")";
                    }
                case LinkConditionType.And:
                    if (Parameters.Count <= 1)
                        throw new Exception("And连接词最少包含两个条件！");
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        for (var i = 0; i < Parameters.Count; i++)
                        {
                            sb.Append("(");
                            sb.Append(Parameters[i].ToString());
                            sb.Append(")");
                            if (i < Parameters.Count - 1)
                            {
                                sb.Append(" AND ");
                            }
                        }
                        return sb.ToString();
                    }
                case LinkConditionType.Or:
                    if (Parameters.Count <= 1)
                        throw new Exception("Or连接词最少包含两个条件！");
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        for (var i = 0; i < Parameters.Count; i++)
                        {
                            sb.Append("(");
                            sb.Append(Parameters[i].ToString());
                            sb.Append(")");
                            if (i < Parameters.Count - 1)
                            {
                                sb.Append(" OR ");
                            }
                        }
                        return sb.ToString();
                    }
                default:
                    return string.Empty;
            }
        }
    }

    public class OrderParameter
    {
        public static readonly string Key = "$orderby";

        public List<IColumnObject> Columns { get; set; }

        public string Name { get; set; }

        public OrderParameter(List<IColumnObject> columns)
        {
            Columns = columns;
        }

        public override string ToString()
        {
            if (Columns == null || Columns.Count < 1)
                return string.Empty;
            StringBuilder sb = new StringBuilder("ORDER BY ");
            for (var i = 0; i < Columns.Count; i++)
            {
                sb.Append(Columns[i].ToString());
                if (i < Columns.Count - 1)
                {
                    sb.Append(", ");
                }
            }
            return sb.ToString();
        }
    }


    public class PaginationParameter
    {
        public static readonly string Key = "$page";

        public int Size { get; set; }

        private int index = 1;
        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                if (value <= 0)
                    value = 1;
                index = value;
            }
        }

        public override string ToString()
        {
            return string.Format("LIMIT {0} OFFSET {1}", Size, (Index - 1) * Size);
        }
    }

    public enum CombinationType
    {
        And,
        Or
    }

    public enum ConditionType
    {
        Equal,
        In,
        NotEqual,
        NotIn,
        GreaterThan,
        LessThan,
        GreaterThanEqual,
        LessThanEqual,
        Between,
        NotBetween,
        Like,
        LikeRight,
        LikeLeft,
        NotLike,
        NotLikeRight,
        NotLikeLeft,
    }


    public enum OrderType
    {
        Ascending,
        Descending
    }

    public enum ScopeType
    {
        Include,
        Exclude
    }

    public enum LinkConditionType
    {
        And,
        Or,
        Not
    }

    public static class LinkConditionTypeExtention
    {
        public static LinkConditionType? ToLinkConditionType(this string val)
        {
            if (string.IsNullOrWhiteSpace(val))
                return null;
            val = val.Trim().ToLower();
            switch (val)
            {
                case "$or":
                    return LinkConditionType.Or;
                case "$and":
                    return LinkConditionType.And;
                case "$not":
                    return LinkConditionType.Not;
            }
            return null;
        }
    }

    public enum AggregationType
    {
        Avg,
        Max,
        Min,
        Count,
        Sum
    }

    public static class ValueStringExtension
    {
        public static string TrimSingleQuotes(this string val)
        {
            if (string.IsNullOrWhiteSpace(val))
                return val;
            if (val.StartsWith("'"))
            {
                val = val.Substring(1);
                if (val.EndsWith("'"))
                    val = val.Substring(0, val.Length - 1);
            }
            return val;
        }
    }

    public static class AggregationTypeExtention
    {
        public static AggregationType? ToAggregationType(this string val)
        {
            if (string.IsNullOrWhiteSpace(val))
                return null;
            val = val.Trim().ToLower();
            switch (val)
            {
                case "$avg":
                    return AggregationType.Avg;
                case "$max":
                    return AggregationType.Max;
                case "$min":
                    return AggregationType.Min;
                case "$count":
                    return AggregationType.Count;
                case "$sum":
                    return AggregationType.Sum;
                default:
                    return null;
            }
        }

        public static string ToAggregationString(this AggregationType? val)
        {
            switch (val)
            {
                case AggregationType.Avg:
                    return "AVG";
                case AggregationType.Max:
                    return "MAX";
                case AggregationType.Min:
                    return "MIN";
                case AggregationType.Count:
                    return "Count";
                case AggregationType.Sum:
                    return "Sum";
                default:
                    return null;
            }
        }

    }
}