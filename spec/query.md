# Query Api - 单表或者单视图`CURD`操作

* * *

[TOC]

* * *

<a name='select'></a>
## 查询操作（SELECT）

### 1. **简单查询**
简单查询是指对一下数据库的表或者视图进行简单的查询操作。如：查询`user`表的所有数据:
* **请求URL：** `https://localhost:<port>/dbrestful/api/query/user`
* **请求类型：** `GET`
* **请求参数: ** 无
* **表单参数: ** 无
* **请求返回数据：** `json`格式的用户列表数据 <a name="usersResult"></a>
```json
{
    "hasError": false,    // 如果是true，说明操作有错误
    "errorMessage": null, // 如果hasError为true，则errorMessage为错误信息
    "data": [{            // 用户数组
        "id": 1,
        "name": "Dawson",
        ...
    }, ...]
}
```


<a name="where"></a>
### 2. **条件查询（Where）**
如：查询`user`表中，`id > 5`，且`score <= 60`的用户：
* **请求URL：** `https://localhost:<port>/dbrestful/api/query/user`
* **请求类型：** `GET`
* **请求参数：** `parameters`为[`WHERE条件查询`](#whereCondition)结构的字符串:
```json
{
    '$where': {
        '$and': [{
            '$gt': {
                'id'：5
            }
        }, {
            '$lte': {
                'score'：60
            }
        }]
    }
}
```
* **表单参数: ** 无
* **请求返回数据：** `json`格式的[用户列表数据](#usersResult)
* **说明：**
> 请求参数`parameters`为`json`形式的字符串，即通过`JSON.stringify(parameters)`来将对象转成字符串
> 对于所有查询操作，都只描述`parameters`参数。


<a name="filters"></a>
### 3. **字段筛选（Filters）**
如：查询`user`表的用户姓名`name`表示成`DisplayName`，年龄`age`与分数`score`字段：
* **请求URL：** `https://localhost:<port>/dbrestful/api/query/user`
* **请求类型：** `GET`
* **请求参数：** `parameters`为:
```json
{
    '$where': {},                // 查询条件
    "$includes": [{
            "$name": "name",
            "$alias": "DisplayName"
        }, {
            "$name": "age"       // 如果只指定`$name`，可以简写为字段名称，即："age"
        }
        "score"
    ]
}
```
* **表单参数: ** 无
* **请求返回数据：** `json`格式的[用户列表数据](#usersResul)
* **说明：**
> 1. 如果未指定`$includes`和`$excludes`，则返回全部字段
> 2. 如果只想排除某些字段，可以通过`$excludes`来设置，如：`$excludes:['name', 'score']`
> 3. `$excludes`后面的数组只能是字段名称
> 4. `$includes`，后面是一个包括[字段对象](#fieldObject)的对象，或者如果只指定`$name`，可以简写为该字段名称字符串
> 5. 字段筛选可与[`条件查询`](#whereCondition)一起使用


### 4. **分页支持（Pagination）**
如：查询所有用户，获取每页为10条记录，第3页的数据。
* **请求URL：** `https://localhost:<port>/dbrestful/api/query/user`
* **请求类型：** `GET`
* **请求参数：** `parameters`为:
```json
{
    '$where': {},        // 查询条件
    '$includes': [],     // 字段筛选
    '$page': [10,3]      // 表示：`pageSize:10`, `pageIndex:3`
}
```
* **表单参数: ** 无
* **请求返回数据：** `json`格式数据的
```json
{
    "hasError": false,     // 如果是true，说明操作有错误
    "errorMessage": null,  // 如果hasError为true，则errorMessage为错误信息
    "data": {
        "items": [{        // 用户数组
            "id": 1,
            "name": "Dawson",
            ...
        }, ...],
        "pagination": {
        	"size": 10,	  // 每页10条记录
            "index": 3,	  // 当前第3页
            "count": 45	  // 记录总数
        }
    }
}
```

### 5. **排序支持（OrderBy）**
如：查询用户，按生日`birthday`升序排列，然后按分数`score`降序排列。
* **请求URL：** `https://localhost:<port>/dbrestful/api/query/user`
* **请求类型：** `GET`
* **请求参数：** `parameters`为:
```json
{
    '$where': {}, 	  // 查询条件
    "$order": [
    	"birthday",
    	{
            "$name": "score",
            "$desc": true
        }
    ]
}
```
* **表单参数: ** 无
* **请求返回数据：** 按要求排好序的`json`格式的[用户列表数据](#usersResult)
* **说明：**
>  默认是按升序进行排列，如`birthday`，并可写成字符串形式
> `$order`数组内的设置会以[字段对象](#fieldObject)顺序应用，即，先`order by birthday asc, score desc`


### 6. **分组支持（GroupBy）**
如：查询用户，按年龄`age`和分数`score`分组。
* **请求URL：** `https://localhost:<port>/dbrestful/api/query/user`
* **请求类型：** `GET`
* **请求参数：** `parameters`为:
```json
{
    '$where': {}, 	  // 查询条件
    "$groupby": ['age']
}
```
* **表单参数: ** 无
* **请求返回数据：** 按要求分组的`json`格式数据
```json
{
    "hasError": false,     // 如果是true，说明操作有错误
    "errorMessage": null,  // 如果hasError为true，则errorMessage为错误信息
    "data": [{
            "age": 12,
            "count": 5,
            ...
        }, ...]
    }
}
```
* **说明：**
> `$groupby`可以通过`$includes`[字段筛选](#filters)中的[字段对象](#fieldObject)指定`$aggregation`[聚合指令](#aggregation)配合使用，以计算分组结果。


### 7. **Having支持（Having）**
如：查询学生分数表，所有考试的分数平均大于70的学生。
* **请求URL：** `https://localhost:<port>/dbrestful/api/query/user`
* **请求类型：** `GET`
* **请求参数：** `parameters`为:
```json
{
    '$where': {}, 	  // 查询条件
    "$groupby": 'name',
    "$having": {
        "$eq": {
            "$key": {
                "$name": "score",
                "$aggregation": "$avg"
            },
            "$value": 70
        }
    }
}
```
* **表单参数: ** 无
* **请求返回数据：** 按要求分组的`json`格式数据
* **说明：**
> `$having`必须同时与`$aggregation`[聚合指令](#aggregation)配合使用。


<a name='fieldObject'></a>
## 字段对象
`字段对象`表示某一个字段（此字段可能只有名称，或者还包含聚合函数、正序或倒序排序、别名），所有能包含的属性如下：
```json
{
    "$name": "fieldName",		// 要查询的字段名，如果是复杂字段形式，此属性必须指定
    "$distinct": true,		   // 是否要加 distinct 关键字
    "$aggregation": "$avg",	  // 聚合操作
    "$alias": "aliasName",		// 别名
    "$desc": true				// 是否倒序
}
```

**说明：**
> 1. `$distinct` 只需指定一次
> 2. `$aggregation` 为聚合操作，请参考[聚合指令](#aggregation)
> 3. 如果只想查询指定的字段名称，可以直接使用该字段名称作为简写的字段对象形式
> 4. 放在`$excludes`指令的字段对象，只能是其简写形式，即该字段的名称。
> 5. `$alias` 只能使用在`$includes`中
> 6. `$desc` 只能使用在`$orderby`中


<a name="aggregation"></a>
### 聚合指令
聚合指令是指查询操作中，调用一些简单的数据库聚合函数，完全其对应的聚合操作。目前DbRESTFul支持的聚合指令有：

  |指令名称 | 说明
  |--------| -------
  |$avg    | 求平均值
  |$max    | 求最大值
  |$min    | 求最小值
  |$count  | 求记录条数
  |$sum    | 求和


<a name='whereCondition'></a>
## `WHERE`条件查询结构
`WHERE`条件查询通过`$where`关键属性，通过`$and`与`$or`组合嵌套[基本条件指令](#conditions)（如：`$eq`, `$gt`, `$in`），来构建查询对象，然后通过转成`json`字符串，传入后端调用。其实基本结构如下：
```json
{
    '$where': {
    	'$or': [
            {
            	'$and': [
                    {'$gt': { 'id'：5 }},		   // (id > 5 and
                    {'$lte': { 'score'：60}}		// score <= 60)
            	]
            },
        	{'$lk': {'name': 'Daw'}}	 // or (name like '%Daw%')
        ]
    }
}
```


<a name='conditions'></a>
### 基本条件指令（必须用在where或having中）
基本条件指令通过下面的结构来描述：
```json
{
    '$eq': {
        '$key': {
            '$name': 'id',
            "$aggregation": "$avg"
        },
        '$value': {'$name': 'parentId' }
    }
}
```

**说明：**
> 1. `$value`可以是字段名，也可以直接指定值，如果是字段名，通过`{'$name': 'fieldName' }`来指定
> 2. 如果`$value`为值，则后面直接指定该值，如：`'$value': 5` 或 `'$value': 'shanghai'`，如果是时间字段可以指定为时间字符串（'yyyy-MM-dd HH:mm:ss'/'yyyy-MM-dd'）
> 3. 如果是简单的条件比较，`$key`与`$value`都可以缺省，即简化为：`{ '$eq': {'id': 5 } }`
> 4. `$eq`为条件比较指令，下面是所有支持的条件比较指令说明：
| 指令名称 | 			示例	 		| 				说明				|
|----------|----------------------------|-----------------------------------|
|$eq       |`{'$eq':{'id':5}}`   	    |表示：id = 5						|
|$neq      |`{'$neq':{'id':5}}`  	    |表示：id <> 5						|
|$gt       |`{'$gt':{'id':5}}`   	    |表示：id > 5						|
|$gte      |`{'$gte':{'id':5}}`  	    |表示：id >= 5						|
|$lt       |`{'$lt':{'id':5}}`   	    |表示：id < 5						|
|$lte      |`{'$lte':{'id':5}}`  	    |表示：id <= 5						|
|$in       |`{'$in':{'id':[4,5,15]}`    |表示：id in [4,5,15]				|
|$nin      |`{'$nin':{'id':[4,5,15]}`   |表示：id not in [4,5,15]			|
|$btn      |`{'$btn':{'id':[4,15]}` 	|表示：id between 4 and 15			|
|$nbtn     |`{'$nbtn':{'id':[4,15]}`	|表示：id not between 4 and 15		|
|$lk       |`{'$lk':{'name':'Daw'}`	    |表示：id like '%Daw%'				|
|$lkl      |`{'$lkl':{'name':'Daw'}`	|表示：id like 'Daw%'				|
|$lkr      |`{'$lkr':{'name':'Daw'}`	|表示：id like '%Daw'				|
|$nlk      |`{'$nlk':{'name':'Daw'}`	|表示：id not like '%Daw%'			|
|$nlkl     |`{'$nlkl':{'name':'Daw'}`   |表示：id not like 'Daw%'			|
|$nlkr     |`{'$nlkr':{'name':'Daw'}`   |表示：id not like '%Daw'			|


### 组合条件：
条件组合通过`$and`可以将多个基本条件以`‘且’`的方式组合，通过`$or`来将多个基本条件以`‘或’`的方式组合，且`$and`和`$or`可以嵌套使用,如：
```json
{
    '$and':[
        {'$gt':{'id':5}},
        {'$or': [
            {'$lte':{'score': 60}},
            {'$lk':{'name': 'Liu'}},
        ]}
    ]
}
```


<a name='insert'></a>
## 插入操作（Insert）
可以插入多条数据，如：向`user`表插入一条数据
* **请求URL：** `https://localhost:<port>/dbrestful/api/query/user`
* **请求类型：** `POST`
* **请求参数：** 无
* **表单参数: **
```json
{
    data: [
        { name: 'Dawson', age: 32, score: 89 },
        { name: 'Ricky', age: 1, score: 12 }
    ]
}
```
* **请求返回数据：**  `json`格式
```json
{
    hasError: false,      // 如果是true，说明操作有错误
    errorMessage: null,   // 如果hasError为true，通常errorMessage为错误信息
    data:null             // 暂无数据返回
}
```

<a name='update'></a>
## 更新操作（Update）
如：更新`user`表中，id为3的数据
* **请求URL：** `https://localhost:<port>/dbrestful/api/query/user`
* **请求类型：** `PUT`
* **请求参数：** 无
* **表单参数: **
```json
{
    parameters: {
        '$where': { '$eq': { 'id'：3 }}
    },
    data: {
        name: 'Dawson',
        age: 12,
        score: 89
    }
}
```
* **请求返回数据：** `json`格式
```json
{
    hasError: false, 	// 如果是true，说明操作有错误
    errorMessage: null, // 如果hasError为true，通常errorMessage为错误信息
    data: 12 			// 执行更新操作影响的行数
}
```
* **说明：**
> `parameters`参数，请参考[查询操作](#where)，如果不指定，或者查询结果是多条记录，则对每条记录作同样的更新。


<a name='delete'></a>
## 删除操作（Delete）
如：删除`user`表中，`birthday < '2001-12-31'`的记录
* **请求URL：** `https://localhost:<port>/dbrestful/api/query/user`
* **请求类型：** `DELETE`
* **请求参数：** `parameters`为:
```json
{
    '$where': { '$lt': { 'birthday'：'2001-12-31' }}
}
```
* **请求返回数据：** `json`格式
```json
{
    hasError: false, 	// 如果是true，说明操作有错误
    errorMessage: null,  // 如果hasError为true，通常errorMessage为错误信息
    data: 12 			// 执行更新操作影响的行数
}
```
* **说明：**
> `parameters`参数，请参考[查询操作](#where)，如果不指定，或者查询结果是多条记录，则对每条记录作同样的更新。


