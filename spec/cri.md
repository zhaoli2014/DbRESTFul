配置Redis代码调用（CRI）
===================

--------------------------------------
配置Redis代码调用（Configured Redis Invocation），即client端通过`http/https`直接调用，操作`Redis`数据库。该功能适用于直接对`Redis`数据库的一些基本操作，开发人员可以将操作代码的`Redis`写到配置文件中，或者直接访问`api/v1/cri`接口调用。

* * *
[TOC]
* * *


## 配置说明
开发人员可以通过修改项目的根目录下的[`config.json`](config.md)文件中，`CRI`子项的相关属性来设置适合项目的Redis数据库操作配置，具体格式如下：
```json
{
    ……
    "CRI": {
        "ConnectionString": "trade.wicture.com:6379,password=Wicture@123",
        "Path": "CRI/",
        "UnwrapParameterName": "param",
        "ApiOnly": false
    }
    ……
}
```
> 说明：
> 1. `ConnectionString`：Redis数据库的连接字符串。
> 2. `Path`: `CRI`的配置文件目录。
> 3. `UnwrapParameterName`: 定义Api的参数时，可以指定一个参数名称，将这个参数名称的值做为`param`的值，对于写操作时非常有用，否则会将该参数的名称一起包在一个对象存入Redis.
> 4. `ApiOnly`: 是否只启用通过API定义的方式调用，否则可以通过直接通过`DbRESTFul`接口调用，但该方式没有安全控制。


## 直接通过`DbRESTFul`接口`api/v1/cri`调用

### 接口说明：

简单直接对Redis数据库直接键值操作，可以直接通过下面的方式调用:

* **请求URL：** `http://<host:port>/api/v1/cri`
* **请求类型：** `POST`
* **请求参数: ** 无
* **表单参数: **
```json
{
    "key": "String",
    "method": "String",
    "dbIndex": "Number",
    "resultType": "String",
    "param": "Object"
}
```
* **参数说明: **
|      name    |  type  | nullable |description
|--------------|--------|----------|-------------
|key           |string  |no        |Redis数据库的Key
|method        |string  |no        |调用方法，请参考CRI支持方法。
|dbIndex       |int     |yes       |Redis数据库Index，默认为：0
|resultType    |string  |yes       |返回数据的类型，String类型或者Object类型，默认为：String
|param         |object  |yes       |要操作的数据，通过在写入时需要指定，可以是string类型，也可以是object类型。

* **请求返回数据：**
```json
{
    "statusCode": "200",
    "errorMessage": "",
    "data": {}
}
```
* **返回数据说明: **
|      name    |  type  | nullable |description
|--------------|--------|----------|-------------
|statusCode    |string  |no        |状态码，200为正常，500为内部错误，其它为用户自定义错误
|errorMessage  |string  |no        |如果无错误信息则为空，否则为错误信息。
|data          |int     |no        |返回在数据，如果为请求的`resultType`为String则返回字符串，否则为对象类型。


### 调用示例：
假设我需要插入一个用户信息，则可以通过如下调用方式：
```bash
curl -X POST \
    -H "Content-Type: application/json" \
    -d '{
        "dbIndex": 1,
        "method": "StringSet",
        "resultType": "Object",
        "key": "Member_Dawson",
        "param": {
          "id": 10,
          "name": "Dawson",
          "age": 43
        }
    }' \
    http://localhost:5000/api/v1/cri
```
返回结果：
```json
{
    "statusCode": "200",
    "errorMessage": null,
    "data": {
        "success": true
    }
}
```

查询该操作的结果：
```bash
curl -X POST \
    -H "Content-Type: application/json" \
    -d '{
        "dbIndex": 1,
        "method": "StringGet",
        "resultType": "Object",
        "key": "Member_Dawson"
    }' \
    http://localhost:5000/api/v1/cri
```
返回结果：
```json
{
    "statusCode": "200",
    "errorMessage": null,
    "data": {
        "id": 10,
        "name": "Dawson",
        "age": 43
    }
}
```


## 通过后端配置代码调用
### CRI JSON配置说明
CRI的详细定义，请参考`Wicture.DbRESTFul`项目中`Schema`目录下的`cri-schema.json`文件。它的主要结构如下：
```json
{
    "name": "String",
    "key": "String",
    "method": "String",
    "dbIndex": "Number",
    "resultType": "String",
    "param": "Oject"
}
```
> 说明：通过后端配置代码调用，其配置结构与直接调用接口非常相似，只是要求给该配置加了一个`name`，以标识其唯一性。


### 示例说明
假设我们需要向`Redis`数据库`1`中插入下面的一个键为`Greeting`的`String`类型的值。我们可以这样做：
1. 定义`CRI`，在项目的`CRI`目录下，新建一个`cri-file.json`的文件，然后加入：
```json
[{
    "name": "SetGreeting",
    "key": "SetGreeting",
    "dbIndex": 1,
    "method": "StringSet"
}]
```
2. 定义相关API接口，将其实现方式改为`cri`，同时指定实现名称：`Greeting`
```json
{
    ……
    "module": "cri",
    "url": "string/set",
    "method": "POST",
    "implementation": {
        "type": "cri",
        "name": "SetGreeting"
    },
    "parameter": {
      "body": [
        {
          "name": "param",
          "type": "string",
          "nullable": false,
          "description": "数据"
        }
      ]
    }
    ……
}
```
3. 通过Api访问
    - Url：`http://<host>:<port>/api/cri/string/set`
    - Method: `POST`
    - Body:
    ```json
    {
        "param": {
          "id": 10,
          "name": "Dawson",
          "age": 43
        }
    }
    ```
4. 假设我们设置`ApiOnly = false`，则可以通过该API来验证：
	- Url: `http://<host>:<port>/api/v1/cri`
    - Method: `GET`
    - Request:
    ```json
    {
        "dbIndex": 1,
        "method": "StringGet",
        "resultType": "Object",
        "key": "SetGreeting"
    }
    ```
    - Result:
    ```json
    {
        "statusCode": "200",
        "errorMessage": null,
        "data": {
            "id": 10,
            "name": "Dawson",
            "age": 43
        }
    }
```


## CRI支持方法 <a name='SupportedMethods'></a>
CRI支持通用的Redis操作，基本代码可以在`DbRESTFul`项目的`Redis`目录下找到`RedisContext`类来做扩展，目前已支持的方法如下：
   Method    |    Description
-------------|-----------------------
StringGet    |读取字符数据
StringSet    |写入字符数据
ListPop      |对应Redis的ListLeftPop操作
ListRange    |对应Redis的ListRange操作
ListPush     |对应Redis的ListRightPush操作
KeyDelete    |删除键，对应Redis的KeyDelete操作
SetAdd       |给Set添加项目，对应Redis的SetAdd操作
……           |……
