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
using UIBuildIt.Common.Documents;
using UIBuildIt.Common;

namespace UIBuildIt.WebService.Controllers.API.UseCases
{
    public class UserController : APIControllerBase<User, UserData>
    {
        protected override ICollection<UserData> GetEntities(User user)
        {
            Item.SetProjectDetails(false);
            // user = db.Users.First(u => u.Id == user.Id);
            return (from p in db.Projects
                    join pu in db.Users on p.Id equals pu.ProjectId
                    where p.Owner.Id == user.Id
                    select pu).ToList().Select(entity => CreateData(entity, entity.Id)).ToList();
        }

        /// <summary>
        /// Fake it till you make it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected override Project GetProject(User entity)
        {
            return new Project();
        }

        protected override IHttpActionResult Validate(User User)
        {
            var user = (User)Request.Properties["user"];
            if (user != null)
            {
                if (!ModelState.IsValid)
                {
                    return Content<string>(HttpStatusCode.BadRequest, ModelState.ToString());
                }
            }
            return null;
        }

        protected override UserData CreateData(User entity, int id)
        {
            var pu = new UserData(db, entity);
            return pu;
        }

        protected override void ClearEntity(User source)
        {
            throw new NotImplementedException();
            //ClearUser(source);
        }
    }
}