using System; //Authentication;
using System.Data;
using System.Data.Entity;
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
    /// Used to log out
    /// </summary>
    public class LogoutController : LoginControllerBase
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
        [ResponseType(typeof(User))]
        public IHttpActionResult Post(Token token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var found = (from t in db.Tokens
                         where t.Id == token.Id && t.UserId == token.UserId && t.Text == token.Text
                         select t).FirstOrDefault();
            if (found != null)
            {
                if (token.Expiration > DateTime.UtcNow)
                {
                    token.Expiration = DateTime.UtcNow;
                }
                db.SaveChanges();
                return Content<string>(HttpStatusCode.OK, "Logged out");
            }
            else
            {
                return Content<Token>(HttpStatusCode.NotAcceptable, new Token() { Text = "Failed", Creation = DateTime.UtcNow, Expiration = DateTime.UtcNow });
            }
            return CreatedAtRoute("DefaultApi", new { id = token.Id }, token);
        }
    }
}