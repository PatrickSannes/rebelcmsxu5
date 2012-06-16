using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Umbraco.Cms.Packages.DevDataset
{
    public class HttpPostedFileProxy : HttpPostedFileBase
    {
        private readonly FileInfo _file;

        public HttpPostedFileProxy(string path)
        {
            var filePath = HttpContext.Current.Server.MapPath(path);
            _file = new FileInfo(filePath);
        }

        public override int ContentLength
        {
            get { return Convert.ToInt32(_file.Length); }
        }

        public override string FileName
        {
            get { return _file.Name; }
        }

        public override string ContentType
        {
            get { return "application/octet-stream"; }
        }

        public override Stream InputStream
        {
            get
            {
                return _file.OpenRead();
            }
        }
    }
}
