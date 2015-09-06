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
using UIBuildIt.Security;

namespace UIBuildIt.WebService.Controllers.API.Authentication
{
    /// <summary>
    /// Used to sign up new users
    /// </summary>
    public class UpdateController : LoginControllerBase
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
        [AuthorizeToken]
        public IHttpActionResult Post(User user)
        {    
            var sessionuser = (User)Request.Properties["user"];
            if(sessionuser.Id != user.Id)
            {
                return Content<Token>(HttpStatusCode.NotAcceptable, new Token() { Text = "Unauthorized", Creation = DateTime.UtcNow, Expiration = DateTime.UtcNow + TimeSpan.FromHours(1.0) }); 
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if(String.IsNullOrEmpty(user.Name) || String.IsNullOrEmpty(user.Email))
            {
                return Content<Token>(HttpStatusCode.NotAcceptable, new Token() { Text = "Name and Email must be valid", Creation = DateTime.UtcNow, Expiration = DateTime.UtcNow + TimeSpan.FromHours(1.0) }); 
            }

            var found = db.Users.FirstOrDefault(u => u.Email == user.Email);

            if (found != null && found.Id != sessionuser.Id)
            {
                return Content<Token>(HttpStatusCode.NotAcceptable, new Token() { Text = "Email Already Exists", Creation = DateTime.UtcNow, Expiration = DateTime.UtcNow + TimeSpan.FromHours(1.0) }); 
            }

            
            var final = db.Users.FirstOrDefault(u => u.Id == user.Id);
            final.Name = user.Name;
            final.Email = user.Email;
            final.Organization = user.Organization;
            db.SaveChanges();
            var token = CreateToken(final);
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