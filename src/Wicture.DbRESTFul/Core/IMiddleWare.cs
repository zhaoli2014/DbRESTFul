using Newtonsoft.Json.Linq;

namespace Wicture.DbRESTFul
{
    /// <summary>
    ///  中间件调用时机
    /// </summary>
    public enum ActivePosition
    {
        /// <summary>
        /// 中间件在参数构建之前调用
        /// </summary>
        BeforeBuildCommandParam,

        /// <summary>
        /// 中间件在CSI调用之前调用
        /// </summary>
        BeforeInvoke,

        /// <summary>
        /// 中间件在CSI调用之后调用
        /// </summary>
        AfterInvoke
    }

    public interface IMiddleWare
    {
        // 中间件调用时机
        ActivePosition ActivePosition { get; }
        // 中间件名称
        string Name { get; }

        /// <summary>
        /// 处理方法
        /// </summary>
        /// <param name="context">调用上下文</param>
        /// <param name="config">CSI中的配置数据</param>
        void Resolve(InvokeContext context, JToken config);

        /// <summary>
        /// 数据校验
        /// </summary>
        /// <param name="context">调用上下文</param>
        /// <param name="config">CSI中的配置数据</param>
        /// <returns>错误信息，如果有空，则表示没有错误</returns>
        string Validate(InvokeContext context, JToken config);
    }
}
