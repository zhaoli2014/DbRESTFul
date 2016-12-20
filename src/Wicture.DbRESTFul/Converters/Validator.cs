using System;
using System.Collections.Generic;
using System.Linq;

namespace Wicture.DbRESTFul.Converters
{
    public class Validator
    {
        public static List<ICSIValidator> Parse(List<string> validators)
        {
            var result = new List<ICSIValidator>();

            foreach (var item in validators)
            {
                if (item == "required") result.Add(new RequiredValidator());
                else if (item == "datetime") result.Add(new DateTimeValidator());
                else if (item == "email") result.Add(new EmailValidator());
                else if (item == "int") result.Add(new IntValidator());
                else if (item == "decimal") result.Add(new DecimalValidator());
                else if (item == "bool") result.Add(new BoolValidator());
                else if (item.StartsWith("regex")) result.Add(new RegexValidator(item));
                else if (item.StartsWith("maxLength")) result.Add(new MaxLengthValidator(item));
                else if (item.StartsWith("minLength")) result.Add(new MinLengthValidator(item));
                else if (item.StartsWith("range")) result.Add(new RangeValidator(item));
                else if (item.StartsWith("in")) result.Add(new InValidator(item));
                else if (item.StartsWith("min")) result.Add(new MinValidator(item));
                else if (item.StartsWith("max")) result.Add(new MaxValidator(item));
                else
                {
                    throw new Exception("Unsupported validator:" + item);
                }
            }

            return result;
        }

        public static void Check(string name, object data, List<ICSIValidator> validators)
        {
            var requiredValidator = validators.FirstOrDefault(v => v is RequiredValidator);
            if (requiredValidator != null && (data == null || data.Equals("")))
            {
                throw new CSIValidationException(name, data, requiredValidator);
            }

            if (data == null) return;

            foreach (var validator in validators)
            {
                if (validator != requiredValidator && !validator.Validate(data))
                {
                    throw new CSIValidationException(name, data, validator);
                }
            }
        }
    }
}
