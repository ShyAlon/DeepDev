using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Http;
using System.Web.Http.Description;
using UIBuildIt.Common;
using UIBuildIt.Common.Authentication;

namespace UIBuildIt.WebService.Controllers.API.Authentication
{
    /// <summary>
    /// Used to recover user credentials
    /// </summary>
    public class RecoverController : LoginControllerBase
    {
        // PUT api/Login/5
        public IHttpActionResult Put(int id, User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.Id)
            {
                return BadRequest();
            }

            db.Entry(user).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/Login
        [ResponseType(typeof(string))]
        public IHttpActionResult Post(User user)
        {
            if (string.IsNullOrEmpty(user.Email))
            {
                return BadRequest(ModelState);
            }

            var us = (from u in db.Users
                     where u.Email == user.Email
                     select u).FirstOrDefault();

            if(us == null)
            {
                return Content<string>(HttpStatusCode.BadRequest, "Failed to find user with email address " + user.Email);
            }

            try
            {
                string Subject = "UIBuildIt password recovery",
                Body = "This is how to restore you password: go to the http://www.vrigar.com/app/#/set-new-password and paste {0} into the verification box and set a new password.",
                ToEmail = user.Email;

                string SMTPUser = "uibuildit@vrigar.com", SMTPPassword = "vr1gars0nuibuildit";

                //Now instantiate a new instance of MailMessage
                MailMessage mail = new MailMessage();

                //set the sender address of the mail message
                mail.From = new System.Net.Mail.MailAddress(SMTPUser, "UIBuildIt Team");

                //set the recepient addresses of the mail message
                mail.To.Add(ToEmail);

                //set the subject of the mail message
                mail.Subject = Subject;

                //set the body of the mail message
                mail.Body = String.Format(Body, us.Password);

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


            return Content<string>(HttpStatusCode.OK, "Mail with the instructions was sent to your email address");
        }
    }
}