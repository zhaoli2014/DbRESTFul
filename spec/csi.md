配置SQL代码调用（CSI）
===================

配置SQL代码调用（Configured SQL Invocation）,即client端通过`http/https`直接调用后端配置好的`SQL`。该功能适用于复杂的业务场景，开发人员可以将复杂的`SQL`写到配置文件中，后台通过传进来的名称与参数调用。CSI是DbRESTFul的核心功能之一。

* * *
[TOC]
* * *

## 配置SQL代码文件
配置SQL代码调用的代码文件以`json`格式存储，通常，该配置代码文件存放在服务项目的CSI目录，也可以通过该项目的根目录下的[`config.json`](config.md)文件中指定。

```json
{
  ……
  "CSIPath": "CSI/"
  ……
}
```


## CSI JSON配置说明
### CSI Schema
CSI的详细定义，请参考`Wicture.DbRESTFul`项目中`Schema`目录下的`csi-schema.json`文件。它的主要结构如下：
```json
{
    "name": "",
    "code": "",
    "resultSet": "",
    "queryOnly": true,
    "requiredTransaction": false,
    "middleWares": {}
}
```

### 示例说明<a name="示例说明"></a>
假设我们需要查询指定项目下的所有接口定义。
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
返回结果请参考[`隐式对象代码`](#隐式对象代码)

> 说明：
> 1. `name`: 必须全局唯一，即整个应用程序级别的唯一性，否则应用程序启动加载时会抛异常。
> 2. `code`为要执行的SQL语句，参数通过`@parameterName`的形式声名。
> 3. 对于可选参数，将条件放入`[]`中，SQL被执行前，如果未指定该参数，则该条件将被忽略。
> 4. `"resultSet": "M"`: 此CSI返回结果集是一个集合。
> 5. `"queryOnly": true`: 该CSI将通过只读连接执行。
> 6. `"requiredTransaction": false`: 该CSI不启用事务。
> 7. 该CSI将通过`pagination` Middleware来作分页操作，具体请参考`Middleware`部分的说明。这里的`pageIndex`与`pageSize`参数就是为了使用该Middleware。
> 8. 该CSI默认以`id DESC`排序，调用者也可以指定排序方式。
> 9. 因`orderBy`为特殊定义，Dapper并不能像SQL参数一样处理，所以需要通过`replace`作执行前处理。
> 10. 对于所有参数都是可选参数的情况，`where`后面需要加一个 ` 1=1 ` 的恒成立条件。


### 支持代码方式说明
1. 单次执行代码，即：`code`为字符串类型的SQL语句。如：
```json
{
	"name": "GetApi",
	"code": "SELECT * FROM api WHERE id = @id;",
	"resultSet": "S",
	"queryOnly": true,
	"requiredTransaction": false
}
```
通过`DbRESTFul`标准化输入的结果为：
```json
{
	"statusCode": "200",
	"errorMessage": "",
	"data": {
	    "id": 23,
		"name": "GetUserInfo",
		……
	}
}
```

2. 对象型代码，即：`code`为`object`对象，即，`key`: `sql`。，且。如：`code`的定义为：
```json
{
	"name": "CreateCart",
	"code": {
        "user": "SELECT name, phone FROM user WHERE userId = @userId",
        "order": "UPDATE order SET status = @status WHERE orderId = @orderId;SELECT @orderId AS orderId",
        "cart": "INSERT INTO cart(sn, ammount, userId) VALUES(@sn, @ammount, @userId);SELECT LAST_INSERT_ID() AS cartId;"
    },
	"resultSet": "S,S,S",
	"queryOnly": false,
	"requiredTransaction": true
}
```
通过`DbRESTFul`标准化输入的结果可能是：
```json
{
	"statusCode": "200",
	"errorMessage": "",
	"data": {
	    "user": { "name": "dawson", "phone": 13545245245 },
		"order": { "orderId": 23 },
		"cart": { "cartId": 324 }
	}
}
```
> 说明：
> 1. 返回结果有多个时，通过`"resultSet": "S,S,S"`指定集合类型。
> 2. 执行的结果对象与`code`定义一一对应。
> 3. 如果`"requiredTransaction": true`的话，整个`code`对象将被放在一个`trancaction`内执行。

3. 隐式对象代码<a name="隐式对象代码"></a>，如上面[`示例说明`](#示例说明)的例子，`code`虽然是一个字符串，但因为使用了`pagination` Middleware，实际上也会以对象的方式执行。其它返回结果如下：
```json
{
	"statusCode": "200",
	"errorMessage": "",
	"data": {
	    "items": [
		    { "id": 32, "name": "GetUserInfo", …… },
		    { "id": 33, "name": "UpdateUserInfo", …… },
		    { "id": 34, "name": "DeleteUserInfo", …… },
	    ],
		"pagination": {
		    "pageIndex": 2,
			"pageSize": 10,
			"totalCount": 69
		}
	}
}
```

