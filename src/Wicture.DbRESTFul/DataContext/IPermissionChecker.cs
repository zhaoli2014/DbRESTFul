using System;

namespace Wicture.DbRESTFul
{
    [Flags]
    public enum TableOperation
    {
        None = 0x00,
        Select = 0x01,
        Insert = 0x02,
        Update = 0x04,
        Delete = 0x08,
        Execute = 0x10,
        Invoke = 0x20,
        CURD = Select | Insert | Update | Delete
    }

    public enum ObjectType
    {
        TableOrView = 0,
        ConfiguredCodeInvocation = 1,
        CodeInvocation = 2,
        StoreProcedure = 3
    }

    public interface IPermissionChecker
    {
        bool HasPermission(string tableName, object userId, TableOperation operation, ObjectType type);
    }
}