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
using System.Security.Authentication;

namespace UIBuildIt.WebService.Controllers.API.UseCases
{
    public class ProjectController : APIControllerBase<Project, ProjectData>
    {
        protected override ICollection<ProjectData> GetEntities(User user)
        {
            Item.SetProjectDetails(false);
            // user = db.Users.First(u => u.Id == user.Id);
            var projects =  (from p in db.Projects
                    where p.Owner.Id == user.Id
                    select p).ToList().Select(entity => CreateData(entity, entity.Id)).ToList();

            var Ids = (from i in projects select i.Entity.Id).ToList();

            var projectIds = (from pu in db.ProjectUsers
                              where pu.UserMail == user.Email
                              select pu.ProjectId).ToList();

            var additional = (from p in db.Projects
                              where projectIds.Contains(p.Id) && !Ids.Contains(p.Id)
                              select p).ToList().Select(entity => CreateData(entity, entity.Id)).ToList();
            projects.AddRange(additional);
            return projects;
        }

        protected override Project GetProject(Project entity)
        {
            return entity;
        }

        protected override IHttpActionResult Validate(Project project)
        {
            var user = (User)Request.Properties["user"];
            if (user != null)
            {
                if (project.Owner == null || project.Owner.Id < 1)
                {
                    project.Owner = user;
                }
            }
            if (!ModelState.IsValid)
            {
                return Content<string>(HttpStatusCode.BadRequest, ModelState.ToString());
            }
            return null;
        }

        protected override string BeforeAttach(Project project)
        {
            SetUnchanged(project.Owner, db);
            return null;
        }

        protected override ProjectData CreateData(Project project, int id)
        {
            var user = (User)Request.Properties["user"];
            var data = new ProjectData(db, project, user, id);
            
            return data;
        }

       

        protected override void ClearEntity(Project source)
        {
            ClearProject(source);
        }
    }
}