# 权限控制
DbRESTFul中的API支持权限控制，其权限判断基于用户角色对指定表的`查询/插入/更新/删除`操作授权。

##权限相关基础表
1. 用户表（User）
 字段称  |  类型  | 是否为空 |    说明
--------| ------| --------| ---------
id		| int	| 否  	 | 用户的标识
roleId  | int	| 是  	 | 用户所属角色
……		| ……	| ……      | 其它字段

2. 角色表（Role）
 字段称  |      类型    	| 是否为空 |    说明
--------| ------------- | --------| ---------
id		| int			| 否  	 | 角色的标识
name  	| varchar(31)	| 否  	 | 角色名称
……		| ……			| ……      | 其它字段

3. 权限表（ApiPermission）
     字段称	  |      类型    	 | 是否为空 |    说明
------------	| ------------- |:-------:| ---------
id				| int			| 否  	 | 权限的标识
name			| varcahr		| 否  	 | 权限的名称
roleId			| int			| 否  	 | 权限的标识
type  			| smallint		| 否  	 | 权限操作类型(0:table or view, 1:store procedure, 2:configured code invocation)
object  		| varchar(31)	| 否  	 | 待操作对象（table, view, sp, cci）
flag			| int		    | 是      | 允许操作的二进制值（请参考说明）

**说明**
> `flag`为权限操作标识，判断的时候，通过`&`即可，如：`01010`表示`可插入`与`可删除`
> - `00001`: `selectable`
> - `00010`: `insertable`
> - `00100`: `updatable`
> - `01000`: `deletable`
> - `10000`: `executable`
> `type`:
> - TableOrView = 0
> - ConfiguredCodeInvocation = 1
> - StoreProcedure = 2


##权限判断
当用户非法访问时，返回信息如下：
```JSON
{
    "hasError": true,
    "errorMessage": "此操作无权限！",
    "data": null
}
```

##权限编辑

1. 获取角色
    * **请求URL：** `https://localhost:<port>/api/<version>/permission`
    * **请求类型：** `get`
    * **请求参数：**
    ```json
    {
        'id': 21 			// 用户的Id，可为不指定，如果指定，则加载用户所属角色，如果不指定，则加载所有的角色。
    }
    ```
    * **请求返回数据：** `json`格式
    ```json
    {
        "hasError": false,    // 如果是true，说明操作有错误
        "errorMessage": null, // 如果hasError为true，则errorMessage为错误信息
        "data": [{}]          // Role 中的角色，如果没指定用户的Id，则所有用户角色都加载进来
    }
    ```

2. 创建角色
    * **请求URL：** `https://localhost:<port>/api/<version>/permission`
    * **请求类型：** `post`
    * **请求参数：**
    ```json
    {
        'name': '超级管理员'    // 角色名称
    }
    ```
    * **请求返回数据：** `json`格式
    ```json
    {
        "hasError": false,    // 如果是true，说明操作有错误
        "errorMessage": null, // 如果hasError为true，则errorMessage为错误信息
        "data": 1             // 执行影响行数
    }
    ```

3. 设置用户的角色
    * **请求URL：** `https://localhost:<port>/api/<version>/permission`
    * **请求类型：** `put`
    * **请求参数：**
    ```json
    {
        'id': 21，			// 用户的Id
        'roleId'：2			// 角色
    }
    ```
    * **请求返回数据：** `json`格式
    ```json
    {
        "hasError": false,    // 如果是true，说明操作有错误
        "errorMessage": null, // 如果hasError为true，则errorMessage为错误信息
        "data": 1             // 执行影响行数
    }
    ```

4. 给角色授权
    * **请求URL：** `https://localhost:<port>/api/<version>/permission`
    * **请求类型：** `patch`
    * **请求参数：**
    ```json
    {
        'roleId': 21，			// 角色Id
        'tableName'：'order', 	// 表名
        'selectEnabled': true,	// 是否能查询
        'insertEnabled': true,	// 是否能插入
        'updateEnabled': true,	// 是否能更新
        'deleteEnabled': true 	// 是否能删除
    }
    ```
    * **请求返回数据：** `json`格式
    ```json
    {
        "hasError": false,    // 如果是true，说明操作有错误
        "errorMessage": null, // 如果hasError为true，则errorMessage为错误信息
        "data": 1             // 执行影响行数
    }
    ```
    * **说明：**
    > 这个操作总是拿`roleId`与`tableName`去查询，如果已有记录，则更新`selectEnabled`等4个基本操作的权限，否则，创建一条新的授权记录。