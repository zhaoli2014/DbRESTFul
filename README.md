﻿DbRESTFul项目支持从关系数据库直接暴露成RESTFul API服务。它采用ASP.NET Web API实现，数据库访问层采用Dapper。
目前只支持MySQL数据库，但基于Dapper的实现，理论上可以支持其它主流数据库。
支持基本CURD操作
支持排序，分页，包含与排除字段查询
DbRESTFUl中一个独立的项目，可以直接host，并通过:http://localhost:<port>/api