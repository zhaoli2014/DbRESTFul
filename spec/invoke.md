配置SQL代码调用（CSI）
===================

--------------------------------------
配置SQL代码调用（Configured SQL Invocation）,即client端通过`http/https`直接调用后端配置好的`SQL`。该功能适用于复杂的业务场景，开发人员可以将复杂的`SQL`写到配置文件中，后台通过传进来的名称与参数调用。

* * *
[TOC]
* * *

## 配置SQL代码文件
配置SQL代码调用的代码文件以`json`格式存储，通常，该配置代码文件存放在服务项目的CSI目录，也可以通过该项目的根目录下的`config.json`文件中指定。

```json
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <appSettings>
      <add key="CCIPath" value="~/cci" />
    </appSettings>
</configuration>
```


## CCI JSON配置
在code配置中，使用`[]`来设置是否是可选SQL段，`[]`中必须包含`@Parameter`。在前端调用时，如果未指定某参数，则后端将忽略此字段判断。
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