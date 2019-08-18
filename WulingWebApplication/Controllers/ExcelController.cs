
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WulingWebApplication.DAL;

namespace WulingWebApplication.Controllers
{
    [Authorize]
    [Authorize(Roles = "Admin")]
    public class ExcelController : Controller
    {
        //
        // GET: /foot/
        //private static readonly String Folder = "/files";
        public ActionResult Index()
        {
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            return View();
        }

        /// 导入excel文档
        public async Task<ActionResult> ImportExcel(HttpPostedFileBase[] files)
        {
            //1.接收客户端传过来的数据
            //HttpPostedFileBase file = Request.Files["file"];
            HttpPostedFileBase file = files[0];
            if (file == null || file.ContentLength <= 0)
            {
                return Json("请选择要上传的Excel文件", JsonRequestBehavior.AllowGet);
            }
            //string filepath = Server.MapPath(Folder);
            //if (!Directory.Exists(filepath))
            //{
            //  Directory.CreateDirectory(filepath);
            //}
            //var fileName = Path.Combine(filepath, Path.GetFileName(file.FileName));
            // file.SaveAs(fileName);
            //获取一个streamfile对象，该对象指向一个上传文件，准备读取改文件的内容
            Stream streamfile = file.InputStream;
            DataTable dt = new DataTable();
            string FinName = Path.GetExtension(file.FileName);
            if (FinName != ".xls" && FinName != ".xlsx")
            {
                return Json("只能上传Excel文档", JsonRequestBehavior.AllowGet);
            }
            else
            {
                try
                {
                    if (FinName == ".xls")
                    {
                        //创建一个webbook，对应一个Excel文件(用于xls文件导入类)
                        HSSFWorkbook hssfworkbook = new HSSFWorkbook(streamfile);
                        dt = await ExcelDAL.ImExport(dt, hssfworkbook);
                    }
                    else
                    {
                        XSSFWorkbook hssfworkbook = new XSSFWorkbook(streamfile);
                        dt = await ExcelDAL.ImExport(dt, hssfworkbook);
                    }
                    return Json("", JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json("导入失败 ！" + ex.Message, JsonRequestBehavior.AllowGet);
                }
            }

        }


        #region 文件上传 大容量文件
        [HttpPost]
        public ActionResult Upload()
        {
            string fileName = Request["name"];
            string fileRelName = fileName.Substring(0, fileName.LastIndexOf('.'));//设置临时存放文件夹名称
            int index = Convert.ToInt32(Request["chunk"]);//当前分块序号
            var guid = Request["guid"];//前端传来的GUID号
            var dir = Server.MapPath("~/Upload");//文件上传目录
            dir = Path.Combine(dir, fileRelName);//临时保存分块的目录
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            string filePath = Path.Combine(dir, index.ToString());//分块文件名为索引名，更严谨一些可以加上是否存在的判断，防止多线程时并发冲突
            var data = Request.Files["file"];//表单中取得分块文件
            //if (data != null)//为null可能是暂停的那一瞬间
            //{
            data.SaveAs(filePath);//报错
            //}
            return Json(new { erron = 0 });//Demo，随便返回了个值，请勿参考
        }
        public ActionResult Merge()
        {
            var guid = Request["guid"];//GUID
            var uploadDir = Server.MapPath("~/Upload");//Upload 文件夹
            var fileName = Request["fileName"];//文件名
            string fileRelName = fileName.Substring(0, fileName.LastIndexOf('.'));
            var dir = Path.Combine(uploadDir, fileRelName);//临时文件夹          
            var files = System.IO.Directory.GetFiles(dir);//获得下面的所有文件
            var finalPath = Path.Combine(uploadDir, fileName);//最终的文件名（demo中保存的是它上传时候的文件名，实际操作肯定不能这样）
            var fs = new FileStream(finalPath, FileMode.Create);
            foreach (var part in files.OrderBy(x => x.Length).ThenBy(x => x))//排一下序，保证从0-N Write
            {
                var bytes = System.IO.File.ReadAllBytes(part);
                fs.Write(bytes, 0, bytes.Length);
                bytes = null;
                System.IO.File.Delete(part);//删除分块
            }
            fs.Flush();
            fs.Close();
            System.IO.Directory.Delete(dir);//删除文件夹
            ExcelDAL.SqlBulkCopyToDB(finalPath, "PassengerVehicle");
            return Json(new { error = 0 });//随便返回个值，实际中根据需要返回
        }
        #endregion

    }
}