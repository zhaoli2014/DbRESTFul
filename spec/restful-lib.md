# RestFul 类库说明
RestFul依赖于MVC4.0，通过WebAPI形式提供服务

## 初始化应用
新建MVC4.0项目，在`WebApiConfig.Register`方式中调用RestFul类库中的`Config.InitAPI(config)`方法，函数签名如下：
```cs
    void InitAPI(HttpConfiguration config, string apiPrefix = "api")
```
此InitAPI方法中，大致处理行为包含：
1. 注册异常处理的过滤器，异常将通过`AjaxResult`进行返回
2. 配置[路由规则](#route)
3. JSON序列化时，将时间以`yyyy-MM-dd HH:mm:ss`形式返回

### 路由规则<a name="route"></a>
路由规则如下：**(其它业务逻辑不能与此路由冲突)**
```cs
config.Routes.MapHttpRoute(
    name: "DbRestfulApi",
    routeTemplate: "<apiPrefix>/{version}/{controller}/{tableName}",
    defaults: new { controller = "Query", tableName = RouteParameter.Optional }
);
```

## RestfulAPI调用
调用时须传入当前用户的`token`信息，以验证用户权限
调用路径为`<apiPrefix>/v1/{controller}/<tableName>`
具体调用方式请参考文档： [query.md](query.html)