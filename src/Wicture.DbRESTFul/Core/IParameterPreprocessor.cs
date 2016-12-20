using System;

namespace Wicture.DbRESTFul
{
    /// <summary>
    /// 实现该接口的类的，且方法具有<typeparamref name="PreprocessMethodAttribute"/> 的方法，签名满足 JToken Method(JToken token)
    /// </summary>
    public interface IParameterPreprocessor
    {
    }

    /// <summary>
    /// 标记参数预处理方法，注意，方法签名必须满足 JToken Method(JToken token)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class PreprocessMethodAttribute : Attribute
    {
    }
}
