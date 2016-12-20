配置SQL代码调用（CSI）
===================

--------------------------------------
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

### 示例说明
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

> 说明：
> 1. `code`为要执行的SQL语句，参数通过`@parameterName`的形式声名。
> 2. 对于可选参数，将条件放入`[]`中，SQL被执行前，如果未指定该参数，则该条件将被忽略。
> 3. `"resultSet": "M"`: 此CSI返回结果集是一个集合。
> 4. `"queryOnly": true`: 该CSI将通过只读连接执行。
> 5. `"requiredTransaction": false`: 该CSI不启用事务。
> 6. 该CSI将通过`pagination` Middleware来作分页操作，具体请参考`Middleware`部分的说明。这里的`pageIndex`与`pageSize`参数就是为了使用该Middleware。
> 7. 该CSI默认以`id DESC`排序，调用者也可以指定排序方式。
> 8. 因`orderBy`为特殊定义，Dapper并不能像SQL参数一样处理，所以需要通过`replace`作执行前处理。


在code配置中，
如果`where`后面只有一个可选条件，则需要加一个 `1=1` 条件。
```json
[
    {
        name: "query_user_info",
        code: "SELECT * FROM user WHERE id > @userId [and id < @minUserId]",
        resultSet: 'M',
        requiredTransaction: true,
        queryOnly: true,
        validator: {'userId': ['required', 'int']}
    },
    {
        name: "search_user_info",
        code: {
        	items: 'SELECT * FROM user WHERE name LIKE CONCAT("%" + @keyword + "%") LIMIT @pageSize, @pageStart',
            pagination: 'SELECT count(*) as count, @pageSize as size, @pageIndex as index FROM user WHERE name LIKE CONCAT("%" + @keyword + "%")'
        },
        resultSet: "M,S",
        requiredTransaction: true,
        queryOnly: true,
        validator: {
        	'pageSize': ['required', 'int'],
        	'pageIndex': ['required', 'int']
        }
    }
]
```
## **说明：**
> **支持代码方式**
> 1. 单次执行代码，即：`code`为字符串类型的SQL语句。
> 2. 对象型代码，即：`code`为`json`对象类型，即，`key`: `sql`。整个`code`对象将被放在一个`trancaction`内执行（如果指定了的话），且执行的结果将以对象的方式返回。如：`code`的定义为：
> ```json
> {
>     users: 'SELECT name, phone FROM user WHERE score > @score',
>     order: 'SELECT sn, ammount, userId FROM order WHERE orderId = @orderId',
>     pagination: 'SELECT count(*) as count, @pageSize as size, @pageIndex as index FROM user WHERE score = @score'
> }
> ```
> 返回值可能是：
> ```json
> {
>     users: [{ name: 'dawson', phone: 13545245245}, ...],
>     order: [{ sn: 'wa039234913', amount: 231.23, userId: 23}],
>     pagination: [{ count: 34, size: 3, index: 2}]
> }
> ```
> 3. 通过`resultSet`，指定执行结果是单条(`S`)还多条(`M`)记录。如：上面示例中，如果指定`resultSet: "M,S,S"`，则返回值为：
> ```json
> {
>     users: [{ name: 'dawson', phone: 13545245245}, ...],
>     order: { sn: 'wa039234913', amount: 231.23, userId: 23},
>     pagination: { count: 34, size: 3, index: 2}
> }
> ```

## 配置代码调用
如：前端需要查询某一段时间内用户的姓名，电话，下单号与订单金额信息，可以通过下面的配置文件（内容）来调用：

```json
[{
	'name': 'query_user_order_info',
    'code': 'SELECT Users.UserName, Users.Phone, Orders.OrderNo, Orders.Amount
             FROM Users
             INNER JOIN Orders
             ON Users.Id = Orders.UserId
             ORDER BY Orders.CreatedDate
             WHERE Orders.CreatedDate >= @startDate AND Orders.CreatedDate <= @endDate',
    'requiredTransaction': true,
	'queryOnly': true,
     'validator': {
     	'startDate': ['datetime', 'required'],
     	'endDate': ['datetime', 'required']
     }
},{
	'name': '……',
    'code': '……',
    'requiredTransaction': true,
	'queryOnly': true,
    'validator': {}
}, ……]
```

**说明：**
> 1. `name`: 在一个或者多个cci的`json`文件下，名称必须是唯一的，否则会出现异常
> 2. `requiredTransaction`: 支持事务
> 3. `queryOnly`: 支持读写分离，`true`，且配置好了读写分离连接，系统将自动通过只读连接处理查询
> 4. `validator`: 支持数据验证，详情参考[数据验证](#validator)

通过DbRESTFul调用该存储过程的方式为：
* **请求URL：** `https://localhost:<port>/api/<version>/invoke`
* **请求类型：** `post`
* **请求参数：**
```json
{
    'name': 'query_user_order_info',
    'parameters': {
        'startDate': '2015-02-15',
        'endDate': '2015-04-15'
    }
}
```
* **请求返回数据：** `json`格式
```json
{
    "hasError": false,    // 如果是true，说明操作有错误
    "errorMessage": null, // 如果hasError为true，则errorMessage为错误信息
    "data": {}            // 调用sql的返回的数据
}
```


## 数据验证 <a name='validator'></a>
配置代码的调用支持数据的验证，通过`validator`来指定数据的验证规则，目前支持的规则如下：
   规则名称   |    规则说明
-------------|-----------------------
required     |不能为空
datetime     |类型为日期类型
email        |邮件类型
int          |int类型
decimal      |数字类型（decimal, int, float, double）
bool         |布尔类型（true, false）
regex:\w     |正则表达式,`:`后接表达式
maxLength:10 |最大长度为10
minLength:5  |最小长度为5
range:3,14   |数字区间，3到14之间
in:a,b,23    |其中的值，'a', 'b'或'23'
min:30       |输入值不能小于30
max:50       |输入值不能大于50


## TODOs
- 考虑是否支持`json`格式的配置代码调用，即：后台的`query.dm`，另外调用。