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
    public class ProjectUserController : APIControllerBase<ProjectUser, ProjectUserData>
    {
        protected override ICollection<ProjectUserData> GetEntities(User user)
        {
            Item.SetProjectDetails(false);
            // user = db.Users.First(u => u.Id == user.Id);
            return (from p in db.Projects
                    join pu in db.ProjectUsers on p.Id equals pu.ProjectId
                    where p.Owner.Id == user.Id
                    select pu).ToList().Select(entity => CreateData(entity, entity.Id)).ToList();
        }

        protected override Project GetProject(ProjectUser entity)
        {
            return (from p in db.Projects
                    where p.Id == entity.ProjectId
                    select p).FirstOrDefault();
        }

        protected override IHttpActionResult Validate(ProjectUser projectUser)
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

        protected override ProjectUserData CreateData(ProjectUser entity, int id)
        {
            var pu = new ProjectUserData() { Entity = entity };
            return pu;
        }

        protected override void ClearEntity(ProjectUser source)
        {
            ClearProjectUser(source);
        }

        protected override string BeforeAttach(ProjectUser entity)
        {
            entity.Name = entity.UserMail;
            entity.Description = entity.UserMail;
            var existing = (from p in db.ProjectUsers
                            where p.ProjectId == entity.ProjectId && p.UserMail == entity.UserMail && p.UserType == entity.UserType
                            select p).FirstOrDefault();
            return existing == null ? null : "Exact project user already exists";
        }
    }
}