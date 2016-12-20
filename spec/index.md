# DbRESTFul 功能简介

`DbRESTFul`支持从关系数据库暴露成RESTFul API服务。Web前端，或者App端可以直接通过`http`或者`https`请求来访问，为快速开发单页面（SAP）网站和移动应用（Android、iOS及Windows  ）提供高效的后端服务框架。它采用ASP.NET Web API实现，数据库访问层采用Dapper。

- 目前只支持MySQL数据库，因为数据库的访问层基于Dapper的实现，理论上可以支持其它更多的主流数据库，如：SQL Server, Oracle, DB2等。
- 它支持标准常用的单表或者视图的`CURD`操作。
- 支持排序，分页，包含与排除字段查询。
- 支持对存储过程的调用。
- 复杂逻辑的SQL调用，可以通过后台SQL配置文件的方式实现。
- 支持后端的数据验证，以保证数据的合法性。
- 支持数据字段级别的数据访问权限控制。
- 支持读写分离，以保证数据库访问的高效性。

`DbRESTFul`是一个独立的ASP.NET MVC项目，可以直接部署在IIS上，并通过:`http://localhost:<port>/api/`来向外提供服务。


# 目录

* [`query`](query.html) - 单表或者单视图`CURD`操作
* [`procedure`](procedure.html) - 存储过程调用
* [`invoke`](invoke.html) - 配置过程调用
* [`permission`](permission.html) - 访问权限控制
* [`rw-control`](rw-control.html) - 读写分离
* [`http/https`](https.html) - `SSL`支持
* [`dbrestful.js`](dbrestful.js.html) - `SSL`支持

# TODO

 - 将所有表及其字段先读出来，用于异常处理（TableExsit），和Include与Exclude支持。
 - 分析`parameters`的结构
 - 测试驱动
 - 支持访问控制
 - 支持批量操作
 - 支持更多数据库
 - 支持存储过程
 - 支持事务
 - 优化api的url，`http://host<:port>/api/<tableName>/<parameter>`
 - 优化，StringBuilder.Append
 - 开放源码
 - 字段类型判断（如string与datetime）
 - 字符串特殊字符转义
 - 查询都应该仅包含部分字段

# 版本
`0.0.1`