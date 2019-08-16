using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace WulingWebApplication.DAL
{
    public static  class WuLinDAL
    {
        //public static int AddRecorder(string categary, string fcategary, string fTitle, string fUrl)
        //{
        //    string sql = string.Format("insert into Foot (categary,fcategary,fTitle,fUrl)values(@categary,@fcategary,@fTitle,@fUrl)");
        //    SqlParameter[] parm =
        //      {
        //   new SqlParameter("@categary",categary)
        //  ,new SqlParameter("@fcategary",fcategary)
        //  ,new SqlParameter("@fTitle",fTitle)
        //  ,new SqlParameter("@fUrl",fUrl)
        //};
        //    return new DBHelperSQL<Foot>(CommonTool.dbname).ExcuteSql(sql, parm);
        //}
    }
}