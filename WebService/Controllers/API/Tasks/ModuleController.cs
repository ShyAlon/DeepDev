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
using UIBuildIt.Common.Tasks;

namespace UIBuildIt.WebService.Controllers.API.Tasks
{
    public class ModuleController : APIControllerBase<Module, ModuleData>
    {
        protected override ICollection<ModuleData> GetEntities(User user)
        {
            Item.SetProjectDetails(false);
            return (from p in db.Projects
                    join m in db.Modules on p.Id equals m.ParentId
                    where p.Owner.Id == user.Id
                    select m).ToList().Select(entity => CreateData(entity, entity.Id)).ToList();
        }

        protected override Project GetProject(Module entity)
        {
            return (from p in db.Projects
                    where p.Id == entity.ParentId
                    select p).FirstOrDefault();
        }

        protected override IHttpActionResult Validate(Module entity)
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

        protected override ModuleData CreateData(Module u, int id)
        {
            var moduleData = new ModuleData(db, u);
            return moduleData;
        }

        protected override void ClearEntity(Module source)
        {
            ClearModule(source);
        }
    }
}