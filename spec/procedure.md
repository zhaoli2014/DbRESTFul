# 存储过程调用

--------------------------------------


[TOC]


## 存储过程调用（PROCEDURE）
如：数据库中有查询`user`分数大于指定参数的用户的存储过程：

```sql
CREATE PROCEDURE sp_queryUserWithScore
(IN score INT)
BEGIN
  SELECT * FROM `user` WHERE `score` = score;
END
```

通过DbRESTFul调用该存储过程的方式为：
* **请求URL：** `https://localhost:<port>/api/<version>/procedure`
* **请求类型：** `post`
* **请求参数：**
```json
{
    'name': 'sp_queryUserWithScore',
    'parameters': {
        'score': 80
    }
}
```
* **请求返回数据：** `json`格式
```json
{
    "hasError": false,    // 如果是true，说明操作有错误
    "errorMessage": null, // 如果hasError为true，则errorMessage为错误信息
    "data": {}            // 存储过程返回的数据
}
```