using System;
using Dapper;
using Dapper.Contrib.Extensions;
using Oracle.ManagedDataAccess.Client;
namespace VNPTBKN.API.Common
{
    public static class db
    {
        public static OracleConnection Connection(string connectionString = "TTKD_BKN")
        {
            try
            {
                // if (string.IsNullOrEmpty(connectionString)) {
                //     var db = new TM.Core.Connection.Oracle("VNPTBK");
                //     return db.Connection;
                // } else {
                //     var db = new TM.Core.Connection.Oracle(connectionString);
                //     return db.Connection;
                // }
                var db = new TM.Core.Connection.Oracle(connectionString);
                return db.Connection;
            }
            catch (System.Exception) { throw; }
        }
        public static Boolean isExist(this OracleConnection con, string table, string column, string value, string condition = "")
        {
            var param = new Dapper.Oracle.OracleDynamicParameters("returns");
            param.Add("v_table", table);
            param.Add("v_column", column);
            param.Add("v_value", value);
            param.Add("v_condition", condition);
            var data = (decimal)con.ExecuteScalar("CHECK_EXIST", param, commandType: System.Data.CommandType.StoredProcedure);
            //var qry = $"select * from {table} where {column}=:value";
            //var data = Connection().QueryFirstOrDefault(qry, new { value = value });
            //if (data != null) return true;
            if (data > 0) return true;
            return false;
        }
        public static Authentication.Core.DBNguoidung getUserFromToken(this OracleConnection con, string token)
        {
            var qry = $"select nd.*,nv.* from nguoidung nda,admin_bkn.nguoidung nd,admin_bkn.nhanvien nv where nda.nguoidung_id=nd.nguoidung_id and nd.nhanvien_id=nv.nhanvien_id and nda.token='{token.Replace("Bearer ", "")}'";
            return con.QueryFirstOrDefault<Authentication.Core.DBNguoidung>(qry);
        }
        public static long getID(this OracleConnection con, string table, string key = "id")
        {
            var qry = $"select max({key})+1 {key} from {table}";
            var rs = con.ExecuteScalar(qry);
            if (rs == null) return 1;
            return Convert.ToInt64(rs);
        }
    }
}