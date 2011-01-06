using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Mvc;

namespace WebApplication
{
    public class JsonpResult : System.Web.Mvc.JsonResult
    {
        public string Callback { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HttpResponseBase response = context.HttpContext.Response;

            if (!String.IsNullOrEmpty(ContentType))
            {
                response.ContentType = ContentType;
            }
            else
            {
                response.ContentType = "application/json";
            }
            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }
            if (Data != null)
            {
                HttpRequestBase request = context.HttpContext.Request;

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                if (null != Callback)
                {
                    response.Write(Callback + "(" + serializer.Serialize(Data) + ")");
                }
                else
                {
                    response.Write(serializer.Serialize(Data));
                }
            }
        }
    }
}
