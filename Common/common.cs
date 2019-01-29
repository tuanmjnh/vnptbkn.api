using System;
using Dapper;
using Dapper.Contrib.Extensions;
using Oracle.ManagedDataAccess.Client;
namespace VNPTBKN.API.Common {
    public static class db {
        public static OracleConnection Connection(string connectionString = "TTKD_BKN") {
            try {
                // if (string.IsNullOrEmpty(connectionString)) {
                //     var db = new TM.Core.Connection.Oracle("VNPTBK");
                //     return db.Connection;
                // } else {
                //     var db = new TM.Core.Connection.Oracle(connectionString);
                //     return db.Connection;
                // }
                var db = new TM.Core.Connection.Oracle(connectionString);
                return db.Connection;
            } catch (System.Exception) { throw; }
        }
        public static Boolean isExist(this OracleConnection con, string table, string column, string value) {
            var param = new Dapper.Oracle.OracleDynamicParameters("returns");
            param.Add("v_table", table);
            param.Add("v_column", column);
            param.Add("v_value", value);
            var data = (decimal) con.ExecuteScalar("CHECK_EXIST", param, commandType : System.Data.CommandType.StoredProcedure);
            //var qry = $"select * from {table} where {column}=:value";
            //var data = Connection().QueryFirstOrDefault(qry, new { value = value });
            //if (data != null) return true;
            if (data > 0) return true;
            return false;
        }
        public static long getID(this OracleConnection con, string table, string key = "id") {
            var qry = $"select max({key}) {key} from {table}";
            var rs = (decimal) con.ExecuteScalar(qry);
            return Convert.ToInt64(rs);
        }
    }
}