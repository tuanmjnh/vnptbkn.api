using System;
using Dapper;
using Dapper.Contrib.Extensions;
namespace VNPTBKN.API.Common {
    public class db {
        public static Oracle.ManagedDataAccess.Client.OracleConnection Connection(string connectionString = null) {
            try {
                if (string.IsNullOrEmpty(connectionString)) {
                    var db = new TM.Core.Connection.Oracle();
                    return db.Connection;
                } else {
                    var db = new TM.Core.Connection.Oracle(connectionString);
                    return db.Connection;
                }
            } catch (System.Exception) { throw; }
        }
        public static Boolean isExist(string table, string column, string value) {
            var qry = $"select * from {table} where {column}=:value";
            var data = Connection().QueryFirstOrDefault(qry, new { value = value });
            if (data != null) return true;
            return false;
        }
    }
}