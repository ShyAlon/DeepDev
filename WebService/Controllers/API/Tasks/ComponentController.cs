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
    public class ComponentController : APIControllerBase<Component, ComponentData>
    {
        protected override ICollection<ComponentData> GetEntities(User user)
        {
            Item.SetProjectDetails(false);
            return (from p in db.Projects
                    join m in db.Modules on p.Id equals m.ParentId
                    join c in db.Components on m.Id equals c.ParentId
                    where p.Owner.Id == user.Id
                    select c).ToList().Select(entity => CreateData(entity, entity.Id)).ToList();
        }

        protected override Project GetProject(Component entity)
        {
            return (from p in db.Projects
                    join m in db.Modules on p.Id equals m.ParentId
                    where m.Id == entity.ParentId
                    select p).FirstOrDefault();
        }

        protected override IHttpActionResult Validate(Component entity)
        {
            var user = (User)Request.Properties["user"];
            var module = db.Modules.FirstOrDefault(p => p.Id == entity.ParentId);
            if (module == null)
            {
                return Content<string>(HttpStatusCode.NotFound, string.Format("failed to get parent"));
            }
            if (!ModelState.IsValid)
            {
                return Content<string>(HttpStatusCode.BadRequest, ModelState.ToString());
            }
            return null;
        }

        protected override ComponentData CreateData(Component u, int id)
        {
            var componentData = new ComponentData(db, u);
            return componentData;
        }

        protected override void ClearEntity(Component source)
        {
            ClearComponent(source);
        }
    }
}