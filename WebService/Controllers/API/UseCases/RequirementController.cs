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
    public class RequirementController : APIControllerBase<Requirement, RequirementData>
    {
        protected override ICollection<RequirementData> GetEntities(User user)
        {
            Item.SetProjectDetails(false);
            return (from p in db.Projects
                    join r in db.Requirements on p.Id equals r.ParentId
                    where p.Owner.Id == user.Id
                    select r).ToList().Select(entity => CreateData(entity, entity.Id)).ToList();
        }

        protected override Project GetProject(Requirement entity)
        {
            return (from p in db.Projects
                    where p.Id == entity.ParentId
                    select p).FirstOrDefault();
        }

        protected override IHttpActionResult Validate(Requirement entity)
        {
            var user = (User)Request.Properties["user"];
            var project = db.Projects.FirstOrDefault(p => p.Id == entity.ParentId);
            if (project == null)
            {
                return Content<string>(HttpStatusCode.NotFound, string.Format("failed to get parent"));
            }
            if (!ModelState.IsValid)
            {
                return Content<string>(HttpStatusCode.BadRequest, ModelState.ToString());
            }
            return null;
        }

        protected override RequirementData CreateData(Requirement r, int id)
        {
            var requirementData = new RequirementData(db, r);
            return requirementData;
        }

        protected override void ClearEntity(Requirement source)
        {
            ClearRequirement(source);
        }
    }
}