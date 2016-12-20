using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Wicture.DbRESTFul.Converters
{
    public interface ICSIValidator
    {
        bool Validate(object data);
    }

    public class DateTimeValidator : ICSIValidator
    {
        public bool Validate(object data)
        {
            return data != null && (data is DateTime || data.ToString().IsDateTime());
        }
    }

    public class IntValidator : ICSIValidator
    {
        public bool Validate(object data)
        {
            return data != null && data.ToString().IsInt();
        }
    }

    public class EmailValidator : ICSIValidator
    {
        private static Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

        public bool Validate(object data)
        {
            if (data == null) return false;
            return regex.IsMatch(data.ToString());
        }
    }

    public class DecimalValidator : ICSIValidator
    {
        public bool Validate(object data)
        {
            return data != null && data.ToString().IsDecimal();
        }
    }

    public class BoolValidator : ICSIValidator
    {
        public bool Validate(object data)
        {
            return data != null && (data.ToString().Equals("true", StringComparison.CurrentCultureIgnoreCase)
                || data.ToString().Equals("false", StringComparison.CurrentCultureIgnoreCase));
        }
    }

    public class RequiredValidator : ICSIValidator
    {
        public bool Validate(object data)
        {
            return data != null && !string.IsNullOrEmpty(data.ToString());
        }
    }

    public class RegexValidator : ICSIValidator
    {
        public Regex Regex { get; private set; }
        internal RegexValidator(string expression)
        {
            var pattern = expression.Replace("regex", "").TrimStart(':').Trim();
            Regex = new Regex(pattern);
        }

        public bool Validate(object data)
        {
            return data != null && Regex.IsMatch(data.ToString());
        }
    }

    public class MaxLengthValidator : ICSIValidator
    {
        public int Length { get; private set; }

        internal MaxLengthValidator(string expression)
        {
            Length = expression.Replace("maxLength", "").TrimStart(':').Trim().ToInt32();
        }

        public bool Validate(object data)
        {
            return data != null && data.ToString().Length <= Length;
        }
    }

    public class MinLengthValidator : ICSIValidator
    {
        public int Length { get; private set; }
        internal MinLengthValidator(string expression)
        {
            Length = expression.Replace("minLength", "").TrimStart(':').Trim().ToInt32();
        }

        public bool Validate(object data)
        {
            return data != null && data.ToString().Length >= Length;
        }
    }

    public class RangeValidator : ICSIValidator
    {
        public decimal Min { get; private set; }
        public decimal Max { get; private set; }
        public RangeValidator(string expression)
        {
            var items = expression.Replace("range", "").TrimStart(':').Trim().Split(',');
            if (items.Length == 2)
            {
                Min = items[0].ToDecimal();
                Max = items[1].ToDecimal();
            }
            else
            {
                throw new ArgumentException("非法参数：" + expression);
            }
        }

        public bool Validate(object data)
        {
            if (data == null) return false;

            var value = data.ToString().ToDecimal();

            return value >= Min && value <= Max;
        }
    }

    public class InValidator : ICSIValidator
    {
        public string[] Items { get; private set; }

        public InValidator(string expression)
        {
            Items = expression.Replace("in", "").TrimStart(':').Trim().Split(',');
        }

        public bool Validate(object data)
        {
            return data != null && Items.Contains(data.ToString());
        }
    }

    public class MinValidator : ICSIValidator
    {
        public decimal Min { get; private set; }
        internal MinValidator(string expression)
        {
            Min = expression.Replace("min", "").TrimStart(':').Trim().ToDecimal();
        }

        public bool Validate(object data)
        {
            return data != null && data.ToString().IsDecimal() && data.ToString().ToDecimal() >= Min;
        }
    }

    public class MaxValidator : ICSIValidator
    {
        public decimal Max { get; private set; }
        internal MaxValidator(string expression)
        {
            Max = expression.Replace("max", "").TrimStart(':').Trim().ToDecimal();
        }

        public bool Validate(object data)
        {
            return data != null && data.ToString().IsDecimal() && data.ToString().ToDecimal() <= Max;
        }
    }
}
