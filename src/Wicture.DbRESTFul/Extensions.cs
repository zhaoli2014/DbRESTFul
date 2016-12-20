using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class Extensions
    {
        public static bool Has(this int value, System.Enum type)
        {
            try
            {
                return ((value & (int)(object)type) == (int)(object)type);
            }
            catch
            {
                return false;
            }
        }

        public static string FormatWith(this string str, params object[] args)
        {
            return string.IsNullOrEmpty(str) ? string.Empty : string.Format(str, args);
        }

        public static bool IsDecimal(this string str)
        {
            decimal test;
            return decimal.TryParse(str, out test);
        }

        public static bool IsInt(this string str)
        {
            int test;
            return int.TryParse(str, out test);
        }

        public static bool IsDateTime(this string str)
        {
            DateTime test;
            return DateTime.TryParse(str, out test);
        }

        public static bool IsDateTime(this string str, out DateTime test)
        {
            return DateTime.TryParse(str, out test);
        }

        public static decimal ToDecimal(this string str)
        {
            decimal test;
            return decimal.TryParse(str, out test) ? test : 0;
        }

        public static Int32 ToInt32(this string str)
        {
            Int32 test;
            Int32.TryParse(str, out test);
            return test;
        }

        public static string DateFormat(this object date, string format = "yyyy/MM/dd")
        {
            if (date == null) { return string.Empty; }
            DateTime d;
            if (!date.ToString().IsDateTime(out d))
            {
                return date.ToString();
            }
            else
            {
                return d.ToString(format);
            }
        }

        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            if (source == null || value == null) { return false; }
            if (value == "") { return true; }
            return (source.IndexOf(value, comparisonType) >= 0);
        }

        public static bool Contains(this string[] source, string value, StringComparison comparisonType)
        {
            return System.Linq.Enumerable.Contains(source, value, new CompareText(comparisonType));
        }

        private class CompareText : IEqualityComparer<string>
        {
            private StringComparison m_comparisonType { get; set; }
            public int GetHashCode(string t) { return t.GetHashCode(); }
            public CompareText(StringComparison comparisonType) { this.m_comparisonType = comparisonType; }
            public bool Equals(string x, string y)
            {
                if (x == y) { return true; }
                if (x == null || y == null) { return false; }
                else { return x.Equals(y, m_comparisonType); }
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            foreach (T element in source)
            {
                action(element);
            }
        }


        public static void Catch(this Action action, Action<Exception> cleanup)
        {
            if (action == null)
            {
                return;
            }

            try
            {
                var task = Task.Run(action);
                task.Wait();
            }
            catch (AggregateException e)
            {
                if (cleanup != null)
                {
                    Catch(() => cleanup(e.InnerException),
                        default(Action<Exception>));
                }
            }
        }

        public static int GetUnixTime(this DateTime date, bool useUtc = true)
        {
            return useUtc
                ? Convert.ToInt32((date - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToUniversalTime().AddHours(8)).TotalSeconds)
                : Convert.ToInt32((date - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
        }

        public static long GetUnixLongTime(this DateTime date, bool useUtc = true)
        {
            return useUtc
                ? Convert.ToInt64((date - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToUniversalTime().AddHours(8)).TotalMilliseconds)
                : Convert.ToInt64((date - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds);
        }

        public static void MergeInto(this JContainer left, JToken right)
        {
            foreach (var rightChild in right.Children<JProperty>())
            {
                var rightChildProperty = rightChild;
                var leftProperty = left.SelectToken(rightChildProperty.Name);

                if (leftProperty == null)
                {
                    // no matching property, just add 
                    left.Add(rightChild);
                }
                else
                {
                    var leftObject = leftProperty as JObject;
                    if (leftObject == null)
                    {
                        // replace value
                        var leftParent = (JProperty)leftProperty.Parent;
                        leftParent.Value = rightChildProperty.Value;
                    }

                    else
                        // recurse object
                        MergeInto(leftObject, rightChildProperty.Value);
                }
            }
        }

        public static JToken Merge(this JToken left, JToken right, bool clone = true)
        {
            if (left.Type != JTokenType.Object || right.Type != JTokenType.Object)
            {
                throw new Exception("Merge can be only used for JObject.");
            }

            var leftClone = clone ? (JContainer)left.DeepClone() : (JContainer)left;

            MergeInto(leftClone, right);

            return leftClone;
        }

        public static string EncryptWithMD5(this string source, string salt)
        {
            string md5Password = Encrypt(source);

            if (salt != null && salt != "0")
            {
                md5Password += salt;
                return Encrypt(md5Password);
            }

            return md5Password;
        }


        private static string Encrypt(string source)
        {
            MD5 md5 = MD5.Create();
            byte[] data = Encoding.UTF8.GetBytes(source);
            byte[] md5Data = md5.ComputeHash(data);
            string str = "";

            for (int i = 0; i < md5Data.Length; i++)
            {
                str += md5Data[i].ToString("x").PadLeft(2, '0');
            }

            return str;
        }
    }
}