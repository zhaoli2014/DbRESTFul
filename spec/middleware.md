配置SQL功能中间件（Middleware）
===================



--------------------------------------
配置SQL功能中间件（Middleware）,即client端通过`http/https`直接调用后端配置好的`CSI`时，需要的辅助功能中间件。该功能适用于复杂的业务场景，开发人员无法通过复杂的`SQL`配置文件去实现时，后台通过传进来的名称与参数，结合预定义的功能组件，实现复杂的业务功能。Middleware是CSI配置的核心功能之一。

* * *
[TOC]
* * *




## 配置中间件
中间件是CSI的辅助功能，通常，该配置存放在CSI的“middleWares”下，是以object形式的json对象。

```json
{
   ......
   "middleWares": {
      "pagination": {
          "size": "pageSize",
          "count": "totalCount",
          "page": "pageIndex"
      }
   }
   ......
}
```

## Middleware JSON配置说明
### Middleware Schema
Middleware的详细定义，请参考`Wicture.DbRESTFul`项目中`Schema`目录下的`middleware-schema.json`文件。它的主要结构如下：
```json
{
    ......
   "middleWares": {
      "pagination": {
          "size": "pageSize",
          "count": "totalCount",
          "page": "pageIndex"
      },
      "defaults": {
          "orderBy": "id DESC"
      },
      "replace": [ "orderBy" ]
   }
   ......
}
```

### 示例说明
我们这里使用CSI中的查询示例。
调用参数包括：

|   name   |   type   | required |      note
|----------|----------|----------|----------------
|projectId | int      |    yes   | The project Id
|module    | string   |    no    | The module name
|keyword   | string   |    no    | The keyword for searching (name, module, owner, title)
|orderBy   | string   |    no    | Sorting
|pageIndex | int      |    yes   | pageIndex for pagination
|pageSize  | int      |    yes   | pageSize for pagination

对应的CSI可能是这样定义的：
```json
{
    "name": "list_api_for_project",
    "code": "SELECT * FROM api WHERE projectId = @projectId [AND `module`=@module] [AND (`name` LIKE CONCAT('%',@keyword,'%') OR `module` LIKE CONCAT('%',@keyword,'%') OR `owner` LIKE CONCAT('%',@keyword,'%') OR `title` LIKE CONCAT('%',@keyword,'%') )] @orderBy LIMIT @pageStart, @pageSize;",
    "resultSet": "M",
    "queryOnly": true,
    "requiredTransaction": false,
    "middleWares": {
        "pagination": {
            "size": "pageSize",
            "count": "totalCount",
            "page": "pageIndex"
        },
        "defaults": {
            "orderBy": "id DESC"
        },
        "replace": [ "orderBy" ]
    }
}
```

> 说明：
> 1. `Middleware`为CSI配置中需要的所有中间件，通过预定义的参数名称调用。
> 2. 该CSI将通过`pagination` 中间件来作分页操作。这里的`size`、`count`和`page`三个参数就是为Middleware预定义的配置，分别代表页数、数据总条数、页码，`pageSize`、`totalCount`和`pageIndex`三个`“value”`是要返回的结果中对应的参数名称。
> 3. `defaults`是默认配置的排序功能，该CSI默认以`id DESC`排序，调用者也可以通过传递`@orderBy`参数指定排序方式。
> 4. 因`orderBy`为特殊定义，Dapper并不能像SQL参数一样处理，所以需要通过调用`replace`作执行前处理。

