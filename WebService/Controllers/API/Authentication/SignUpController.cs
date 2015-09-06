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
    /// Used to sign up new users
    /// </summary>
    public class SignUpController : LoginControllerBase
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
        [ResponseType(typeof(Token))]
        public IHttpActionResult Post(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var found = db.Users.FirstOrDefault(u => u.Email == user.Email);

            if (found != null)
            {
                return Content<Token>(HttpStatusCode.NotAcceptable, new Token() { Text = "Email Already Exists", Creation = DateTime.UtcNow, Expiration = DateTime.UtcNow + TimeSpan.FromHours(1.0) }); 
            }

            
            db.Users.Add(user);
            db.SaveChanges();

            var token = CreateToken(user);
            var temp = db.Tokens.Add(token.GetToken());
            db.SaveChanges();
            token.Id = temp.Id;
            return Content<TokenData>(HttpStatusCode.OK, token);
        }

        // DELETE api/Login/5
        [ResponseType(typeof(User))]
        public IHttpActionResult DeleteUser(int id)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            db.Users.Remove(user);
            db.SaveChanges();

            return Ok(user);
        }
    }
}