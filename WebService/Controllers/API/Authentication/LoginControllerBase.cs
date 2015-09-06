using System;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Security.Cryptography; //Authentication;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using UIBuildIt.Common;
using UIBuildIt.Common.Authentication;
using UIBuildIt.Models;

namespace UIBuildIt.WebService.Controllers.API.Authentication
{
    /// <summary>
    /// Used to log in
    /// </summary>
    public class LoginControllerBase : ApiController
    {
        protected UIBuildItContext db = new UIBuildItContext();

        protected TokenData CreateToken(User user)
        {
            var token = new TokenData()
            {
                UserId = user.Id,
                Text = GetMD5HashCode(MD5.Create(), user),
                Creation = DateTime.UtcNow,
                Expiration = DateTime.UtcNow + TimeSpan.FromHours(1.0),
                UserEmail = user.Email,
                UserName = user.Name,
                Organization = user.Organization
            };
            // Add the token to the database
            return token;
        }

        private string GetMD5HashCode(MD5 md5Hash, User user)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(user.Email + DateTime.UtcNow.ToString() + user.Password));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        protected bool UserExists(int id)
        {
            return db.Users.Count(e => e.Id == id) > 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class TokenData
    {

        public string Organization { get; set; }

        public string UserName { get; set; }

        public string UserEmail { get; set; }

        public DateTime Creation { get; set; }

        public DateTime Expiration { get; set; }

        public string Text { get; set; }

        public int UserId { get; set; }

        public Token GetToken()
        {
            return new Token()
            {
                Creation = Creation,
                Expiration = Expiration,
                UserId = UserId,
                Text = Text
            };
        }

        public int Id { get; set; }
    }
}