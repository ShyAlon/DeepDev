using System;
using System.Data;
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
    public class NewPasswordController : LoginControllerBase
    {
        // POST api/NewPassword
        [ResponseType(typeof(string))]
        public IHttpActionResult Post(User user)
        {
            if (string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Invalid New Password");
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                return BadRequest("Invalid Email");
            }

            if (string.IsNullOrEmpty(user.Name))
            {
                return BadRequest("Invalid user token");
            }

            var us = (from u in db.Users
                     where u.Email == user.Email && u.Password == user.Name
                     select u).FirstOrDefault();

            if(us == null)
            {
                return Content<string>(HttpStatusCode.BadRequest, "Failed to identify user " + user.Email);
            }

            us.Password = user.Password;
            db.SaveChanges();
            var token = CreateToken(us);
            token.Id = db.Tokens.Add(token.GetToken()).Id;
            db.SaveChanges();
            return Content<TokenData>(HttpStatusCode.OK, token);
        }
    }
}