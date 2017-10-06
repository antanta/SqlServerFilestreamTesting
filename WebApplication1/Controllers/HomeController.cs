using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SqlServerFilestreamTesting;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
            this.mngr = new FileStreamManager();
        }

        public ActionResult Index(bool? status, string errorMessage)
        {
            ViewBag.Status = status;
            ViewBag.ЕrrorMessage = errorMessage;
            return View();
        }

        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            bool result = false;
            string errorMessage = null;
            try
            {
                if (file.ContentLength > 0)
                {
                    //I Directly use the incoming stream
                    this.mngr.WriteFileToFileStream(file.InputStream);

                    //II save to App Data first
                    //var fileName = Path.GetFileName(file.FileName);
                    //var path = Path.Combine(Server.MapPath("~/App_Data/Uploads"), fileName);
                    //file.SaveAs(path);
                    //this.mngr.WriteFileToFileStream(path);
                    //System.IO.File.Delete(path);

                    result = true;
                }
            }
            catch(Exception e)
            {
                errorMessage = e.Message;
            }
            return RedirectToAction("Index", new { status = result, errorMessage = errorMessage });
        }

        public FileResult DownloadFile(int id)
        {
            Response.AddHeader("Content-type", "application/octet-stream");
            Response.AddHeader("Content-Disposition", String.Format("attachment; filename=\"{0}\"", "myfile.txt"));

            return new FileContentResult(this.mngr.ReadFileFromFileStream(id), "application/octet-stream");
        }

        #region Private memberd
        private readonly FileStreamManager mngr;
        #endregion
    }
}