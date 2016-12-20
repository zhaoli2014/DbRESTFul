using Microsoft.AspNetCore.Http;

namespace Wicture.DbRESTFul.Gateway
{
    /// <summary>
    /// 数据库网关路由接口，调用者可以自己决定
    /// </summary>
    public interface IDbGateway
    {
        /// <summary>
        /// 调用者将根据业务逻辑，返回数据库连接信息
        /// </summary>
        /// <param name="context">当前请求上下文</param>
        /// <param name="userInfo">当前用户信息</param>
        /// <returns></returns>
        DatabaseConnection Process(HttpContext context, IdentityInfo userInfo);
    }
}
