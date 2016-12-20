定义API调用接口（API）
===================

--------------------------------------
定义API（Application Programming Interface）调用接口，是通过约定格式来配置传统WebAPI的Controller以及一些其他配置参数。使得开发人员可以节约成本并更方便的管理其开发接口。API（本文中后续的API统一指的是Dbrestful中的API）还可以模拟返回结果以及生成文档，这样可以使得接口在未开发完成时前端也可以根据接口文档并行开发。不必浪费时间等待后端接口开发完成。


* * *
[TOC]
* * *

## 配置API调用接口
配置API调用接口可以通过文件直接配置或通过PMT平台页面配置管理，配置文件以`json`格式存储，通常，该配置代码文件存放在服务项目的API目录，也可以通过该项目的根目录下的[`config.json`](config.md)文件中指定。

```json
{
  ……
  "APIPath": "API/"
  ……
}
```


## API配置说明
### API Schema
API的详细定义，请参考`Wicture.DbRESTFul`项目中`Schema`目录下的`api-schema.json`文件。它的主要结构如下：
```json
{
    "version": "",
    "owner": "",
    "updatedTime": "",
    "name": "",
    "module": "",
    "url": "",
    "useAbsoluteUrl": true,
    "method": "",
    "title": "",
    "summary": "",
    "note": "",
    "allowAnonymous": false,
    "cache": {
      "enabled": false,
      "type": "redis",
      "expiration": 300
    },
    "implemented": true,
    "implementation": {
      "type": "repository",
      "name": ""
    },
    "parameter": {
      "query": [
        {
          "type": "",
          "name": "",
          "description": ""
        }
      ],
      "body": []
    },
    "result": {
      "type": "json",
      "schema": [
        {
          "name": "",
          "type": "int",
          "nullable": false,
          "description": ""
        },
        {
          "name": "errorMessage",
          "type": "string",
          "nullable": true,
          "description": ""
        },
        {
          "name": "data",
          "type": "object",
          "nullable": true,
          "description": "",
          "schema": [
            {
              "name": "items",
              "type": "array",
              "nullable": false,
              "description": "",
              "schema": [
                {
                  "name": ",
                  "type": "int",
                  "nullable": false,
                  "description": ""
                },...]
               }]
      ]},
    "mock": [
      {
        "input": {
          "waitForStatus": "*",
          "pageIndex": "*",
          "pageSize": "*"
        },
        "output": {
          "statusCode": 200,
          "errorMessage": "",
          "data": {
            "items": "",
            "pagination": ""
          }
        }
      }
    ]
  },
```

### 示例说明
1. 假设我们需要配置API基本信息我可以这样配置：
```json
{
    "version": "1.0",
    "owner": "田成果",
    "updatedTime": "2016-09-26T10:03:16",
    "name": "ListOrders",
    "module": "Order.Info",
    "url": "/client/v1/order/list",
    "useAbsoluteUrl": true,
    "method": "GET",
    "title": "获取订单列表",
    "summary": "获取订单列表",
    "note": "",
    "allowAnonymous": false,
    "cache": {
      "enabled": false,
      "type": "redis",
      "expiration": 300
    },
    "implemented": true,
    "implementation": {
      "type": "repository",
      "name": "OrderRepository.ListOrders"
    },
    //"implementation": {
    //"type": "csi",
    //"name": "ListOrders"
    //},
}
```

> 说明：
> 1. `version`：`API`的版本号。
> 2. `owner`：`API`的负责人。
> 3. `name`：名称，必须全局唯一。
> 4. `module`：`API`所属模块。
> 5. `url`：`API`的访问地址。
> 6. `useAbsoluteUrl`：是否绝对地址(否的时候，访问地址为API\模块名\url)
> 7. `method`：HTTP请求类型(GET POST DELETE PUT ect)
> 8. `useAbsoluteUrl`：是否绝对地址(否的时候，访问地址为API\模块名\url)
> 9. `title`：接口标题
> 10. `summary`：描述
> 11. `note`：备注
> 12. `allowAnonymous`：是否匿名访问，一般都要使用身份验证和权限控制,但总有部分接口是可以匿名访问的
> 13. `useAbsoluteUrl`：是否绝对地址(否的时候，访问地址为API\模块名\url)
>       
> 14. `cache`: 缓存，暂时只支持redis
> 15. `implemented`: 是否已实现，否的时候会返回mock中定义的返回结果。
> 16. `implementation`: 实现方式，实现方式分为两种，一种为csi，一种为repository。（请参考`Wicture.DbRESTFul`项目中`docs`目录下的`CSI`和`repository`文件）


2. 假设我们需要配置API请求信息我可以这样配置：
```json
{
  "parameter": {
      "query": [
        {
          "type": "int",
          "name": "id",
          "nullable": true
          "description": "{id:1}"
        }
      ],
      "body": [
        {
          "type": "object",
          "name": "test"
        }
      ]
    },
}
```
> 说明：
> 1. `parameter`：请求信息格式，分类两类：query以及body。
> 2. `query`：`API`的URL参数。即在URL后跟上请求的定义参数。如：api/test?id=1
> 3. `body`：`API`的表单参数。按raw方式请求，定义内容可以JSON
> 4. `nullable`：表示是否必须填写，如是则必须有内容。


3. 假设我们需要配置API返回以及模拟信息我可以这样配置：
```json
{
  "result": {
      "type": "json",
      "schema": [
        {
          "name": "",
          "type": "int",
          "nullable": false,
          "description": ""
        },
        {
          "name": "errorMessage",
          "type": "string",
          "nullable": true,
          "description": ""
        },
        {
          "name": "data",
          "type": "object",
          "nullable": true,
          "description": "",
          "schema": [
            {
              "name": "items",
              "type": "array",
              "nullable": false,
              "description": "",
              "schema": [
                {
                  "name": ",
                  "type": "int",
                  "nullable": false,
                  "description": ""
                },...]
               }]
      ]},
    "mock": [
      {
        "input": {
          "waitForStatus": "*",
          "pageIndex": "*",
          "pageSize": "*"
        },
        "output": {
          "statusCode": 200,
          "errorMessage": "",
          "data": {
            "items": "",
            "pagination": ""
          }
        }
      }
}
```
> 说明：
> 1. `result`：返回类型默认是json。
> 2. `schema`：`result`中的`schema`指的是返回集合定义。可以是多个返回参数的集合。
> 3. `nullable`：表示是否必须填写，如是则必须有内容。
> 4. `type`：返回字段类型。string，int，object，array etc...
> 5. `mock`：模拟返回信息，当`implemented`是false时才会生效。返回内容可以是多条。条件根据`input`设置匹配
> 6. `input`：模拟的请求参数，为json格式。`*`表示所有参数，不然则按照定义的返回
> 7. `output`：模拟返回字段，为json格式。


## TODOs
