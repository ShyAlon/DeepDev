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

namespace UIBuildIt.WebService.Controllers.API.UseCases
{
    public class SprintController : APIControllerBase<Sprint, SprintData>
    {
        protected override ICollection<SprintData> GetEntities(User user)
        {
            Item.SetProjectDetails(false);
            return (from p in db.Projects
                    join m in db.Milestones on p.Id equals m.ParentId
                    join s in db.Sprints on m.Id equals s.ParentId
                    where p.Owner.Id == user.Id
                    select s).ToList().Select(entity => CreateData(entity, entity.Id)).ToList();
        }

        protected override Project GetProject(Sprint entity)
        {
            return (from p in db.Projects
                    join m in db.Milestones on p.Id equals m.ParentId
                    where m.Id == entity.ParentId
                    select p).FirstOrDefault();
        }

        protected override IHttpActionResult Validate(Sprint entity)
        {
            var user = (User)Request.Properties["user"];
            var milestone = db.Milestones.FirstOrDefault(p => p.Id == entity.ParentId);
            if (milestone == null)
            {
                return Content<string>(HttpStatusCode.NotFound, string.Format("failed to get parent"));
            }
            if (!ModelState.IsValid)
            {
                return Content<string>(HttpStatusCode.BadRequest, ModelState.ToString());
            }
            return null;
        }

        protected override SprintData CreateData(Sprint r, int id)
        {
            return new SprintData(db, r);
        }

        protected override void ClearEntity(Sprint source)
        {
            ClearSprint(source);
        }
    }
}