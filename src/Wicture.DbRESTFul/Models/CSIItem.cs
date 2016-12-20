using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Wicture.DbRESTFul
{
    public class CSIItem
    {
        public string name { get; set; }

        public JToken code { get; set; }

        public string resultSet { get; set; }

        public bool queryOnly { get; set; }

        public bool requiredTransaction { get; set; }

        public JObject middleWares { get; set; }

        public static CSIItem Clone(CSIItem codeItem)
        {
            var result = new CSIItem
            {
                name = codeItem.name,
                requiredTransaction = codeItem.requiredTransaction,
                resultSet = codeItem.resultSet,
                queryOnly = codeItem.queryOnly,
                middleWares = codeItem.middleWares
            };

            result.code = codeItem.code is JValue
                ? new string(codeItem.code.ToString().ToCharArray())
                : JsonConvert.DeserializeObject<JToken>(codeItem.code.ToString());

            return result;
        }

        public static CSIItem Build(string code, bool queryOnly = true, bool requiredTransaction = false)
        {
            var result = new CSIItem
            {
                name = "Dynamic",
                requiredTransaction = requiredTransaction,
                resultSet = "M",
                queryOnly = queryOnly,
                code = code
            };

            return result;
        }
    }
}