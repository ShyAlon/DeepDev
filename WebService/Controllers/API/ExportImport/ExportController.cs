using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UIBuildIt.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using UIBuildIt.Common.UseCases;
using System.Web.Http.Description;
using UIBuildIt.Security;
using UIBuildIt.Common.Authentication;
using UIBuildIt.Common.Base;
using UIBuildIt.Common.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Net.Http.Headers;
using System.Web;
using System.Net.Mail;
using System.Threading;


namespace UIBuildIt.WebService.Controllers.API.UseCases
{
    public class ExportRequestData : ItemData<Project, Item>
    {
        public string Data{ get; set; }
    }

    public class ExportController : APIControllerBase<Project, ExportRequestData>
    {
        protected override ICollection<ExportRequestData> GetEntities(User user)
        {
            return null;
        }

        [AuthorizeToken]
        public override IHttpActionResult Post(Project entity)
        {
            int id = entity.Id;
            return GetItems(id);
        }
        private readonly string path = "~/App_Data/Tmp/FileUploads";
        private IHttpActionResult GetItems(int id)
        {
            var user = (User)Request.Properties["user"];
            var project = db.Projects.FirstOrDefault(p => p.Id == id);
            if (project == null || project.Owner.Id != user.Id)
            {
                return Content(HttpStatusCode.Unauthorized, "Export available only to owner");
            }
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;
            var json = JsonConvert.SerializeObject(db.GetEntitiesWithProjectId(id).ToList(), Formatting.Indented, settings);


            var inputString = json;
            var root = HttpContext.Current.Server.MapPath(path);
            var fileId = Guid.NewGuid().ToString();
            var fullPath = string.Format("{0}/{1}", root, fileId);
            using (var outStream = new MemoryStream())
            {
                using (var tinyStream = new GZipStream(outStream, CompressionMode.Compress))
                using (var mStream = new MemoryStream(Encoding.UTF8.GetBytes(inputString)))
                {
                    using (FileStream file = new FileStream(fullPath,FileMode.Create, FileAccess.Write)) 
                    {
                        using (GZipStream gzs = new GZipStream(file, CompressionLevel.Optimal))
                        {
                            mStream.CopyTo(gzs);
                        }
                    }
                    mStream.CopyTo(tinyStream);
                }
            }

            //HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            ////var stream = new FileStream(path, FileMode.Open);
            //result.Content = new ByteArrayContent(  File.ReadAllBytes(fullPath));
            //result.Content.Headers.ContentType =
            //    new MediaTypeHeaderValue("application/zip");
            //new Thread(() =>
            //{
            //    Thread.Sleep(2222);
            //    SendMail(user, fullPath);
            //}).Start(); ;



            //HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            //var stream = new FileStream(fullPath, FileMode.Open);
            //result.Content = new StreamContent(stream.);
            //result.Content.Headers.ContentType =
            //    new MediaTypeHeaderValue("application/octet-stream");
            //return ResponseMessage(result);
            return Content(HttpStatusCode.OK, fileId);
        }

        private IHttpActionResult SendMail(User user, string fullPath)
        {
            try
            {
                string Subject = "Project Export",
                Body = "This is the exported project file.",
                ToEmail = user.Email;

                string SMTPUser = "uibuildit@vrigar.com", SMTPPassword = "vr1gars0nuibuildit";

                //Now instantiate a new instance of MailMessage
                MailMessage mail = new MailMessage();

                //set the sender address of the mail message
                mail.From = new System.Net.Mail.MailAddress(SMTPUser, "DeepDev Team");

                //set the recepient addresses of the mail message
                mail.To.Add(ToEmail);

                mail.Attachments.Add(new Attachment(fullPath));

                //set the subject of the mail message
                mail.Subject = Subject;

                //set the body of the mail message
                mail.Body = String.Format(Body, user.Password);

                //leave as it is even if you are not sending HTML message
                mail.IsBodyHtml = true;

                //set the priority of the mail message to normal
                mail.Priority = System.Net.Mail.MailPriority.Normal;

                //instantiate a new instance of SmtpClient
                SmtpClient smtp = new SmtpClient();

                //if you are using your smtp server, then change your host like "smtp.yourdomain.com"
                smtp.Host = "smtp.gmail.com";

                //chnage your port for your host
                smtp.Port = 25; //or you can also use port# 587

                //provide smtp credentials to authenticate to your account
                smtp.Credentials = new System.Net.NetworkCredential(SMTPUser, SMTPPassword);

                //if you are using secure authentication using SSL/TLS then "true" else "false"
                smtp.EnableSsl = true;

                smtp.Send(mail);

                return Ok();
            }
            catch (SmtpException ex)
            {
                //catched smtp exception
                return Content<Exception>(HttpStatusCode.BadRequest, ex);
            }
            catch (Exception ex)
            {
                return Content<Exception>(HttpStatusCode.BadRequest, ex);
            }
        }


        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        // [AuthorizeToken]
        public override IHttpActionResult Get(int id)
        {
            return GetItems(id);
        }

        protected override ExportRequestData CreateData(Project source, int id)
        {
            throw new NotImplementedException();
        }

        protected override void ClearEntity(Project source)
        {
            throw new NotImplementedException();
        }

        protected override IHttpActionResult Validate(Project entity)
        {
            throw new NotImplementedException();
        }

        protected override Project GetProject(Project entity)
        {
            throw new NotImplementedException();
        }
    }
}