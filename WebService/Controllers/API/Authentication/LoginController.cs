using System;
using System.Data;
using System.Data.Entity; //Authentication;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using UIBuildIt.Common;
using UIBuildIt.Common.Authentication;

namespace UIBuildIt.WebService.Controllers.API.Authentication
{
    /// <summary>
    /// Used to log in
    /// </summary>
    public class LoginController : LoginControllerBase
    {
        // PUT api/Login/5
        public  IHttpActionResult Put(int id, User user)
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
        /// <summary>
        /// Used to login to the system 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        
        [ResponseType(typeof(Token))]
        public IHttpActionResult Post(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var found = db.Users.FirstOrDefault(u => u.Email == user.Email && u.Password == user.Password);

            if (found != null)
            {
                var token = CreateToken(found);
                var temp = db.Tokens.Add(token.GetToken());
                db.SaveChanges();
                token.Id = temp.Id;
                return Content<TokenData>(HttpStatusCode.OK, token);
            }
            else
            {
                return Content<Token>(HttpStatusCode.NotAcceptable, new Token() { Text = "User name and password are not valid", Creation = DateTime.UtcNow, Expiration = DateTime.UtcNow + TimeSpan.FromHours(1.0) }); 
            }
                
        }
    }
}