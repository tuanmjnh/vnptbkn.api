using System;
using Dapper;
using Dapper.Contrib.Extensions;
using Oracle.ManagedDataAccess.Client;
namespace VNPTBKN.API.Common
{
    public static class db
    {
        public static Oracle.ManagedDataAccess.Client.OracleConnection _connection;
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
                // if (_connection == null)
                _connection = new TM.Core.Connection.Oracle(connectionString).Connection;
                if (_connection.State == System.Data.ConnectionState.Closed)
                    _connection.Open();
                return _connection;
            }
            catch (System.Exception) { throw; }
        }
        public static void Open()
        {
            try
            {
                if (_connection != null && _connection.State == System.Data.ConnectionState.Closed)
                    _connection.Open();
            }
            catch (Exception) { throw; }
        }
        public static void Close()
        {
            try
            {
                if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
                {
                    _connection.Close();
                    // Connection.Dispose();
                }
            }
            catch (Exception) { throw; }
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
        public static nguoidung getUserFromToken(this OracleConnection con, string token)
        {
            try
            {
                var qry = "select nd.*,nv.*,dv.ten_dv,dv.ma_dv,r.name ten_quyen,r.levels cap_quyen,r.code ma_quyen,r.roles quyen ";
                qry += "from ttkd_bkn.nguoidung nda,ttkd_bkn.roles r,admin_bkn.nguoidung nd,admin_bkn.nhanvien nv,ttkd_bkn.db_donvi dv ";
                qry += "where nda.roles_id=r.id and nda.nguoidung_id=nd.nguoidung_id and nd.nhanvien_id=nv.nhanvien_id and nv.donvi_id=dv.donvi_id(+) ";
                qry += $"and nda.token='{token.Replace("Bearer ", "")}'";
                return con.QueryFirstOrDefault<nguoidung>(qry);
            }
            catch (System.Exception) { throw; }
            finally { }
        }
        public static bool inRoles(this nguoidung nd, string roles)
        {
            var tmp = roles.Trim(',').Split(',');
            foreach (var item in tmp)
            {
                if (nd.quyen.IndexOf($",{item},") > -1)
                    return true;
            }
            return false;
        }
        public static long getID(this OracleConnection con, string table, string key = "id")
        {
            var qry = $"select max({key})+1 {key} from {table}";
            var rs = con.ExecuteScalar(qry);
            if (rs == null) return 1;
            return Convert.ToInt64(rs);
        }
        public partial class nguoidung : Authentication.Core.DBNguoidung
        {
            public string ten_dv { get; set; }
            public string ma_dv { get; set; }
            public string ten_quyen { get; set; }
            public int cap_quyen { get; set; }
            public string ma_quyen { get; set; }
            public string quyen { get; set; }
        }
    }
}