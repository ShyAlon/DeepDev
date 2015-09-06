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
    public class MethodController : APIControllerBase<Method, MethodData>
    {
        protected override ICollection<MethodData> GetEntities(User user)
        {
            Item.SetProjectDetails(false);
            return (from p in db.Projects
                    join m in db.Modules on p.Id equals m.ParentId
                    join c in db.Components on m.Id equals c.ParentId
                    join me in db.Methods on c.Id equals me.ParentId
                    where p.Owner.Id == user.Id
                    select me).ToList().Select(entity => CreateData(entity, entity.Id)).ToList();
        }

        protected override Project GetProject(Method entity)
        {
            return (from p in db.Projects
                    join m in db.Modules on p.Id equals m.ParentId
                    join c in db.Components on m.Id equals c.ParentId
                    where c.Id == entity.ParentId
                    select p).FirstOrDefault();
        }

        protected override IHttpActionResult Validate(Method entity)
        {
            var user = (User)Request.Properties["user"];
            var component = db.Components.FirstOrDefault(c => c.Id == entity.ParentId);
            if (component == null)
            {
                return Content<string>(HttpStatusCode.NotFound, string.Format("failed to get parent"));
            }
            if (!ModelState.IsValid)
            {
                return Content<string>(HttpStatusCode.BadRequest, ModelState.ToString());
            }
            return null;
        }

        protected override MethodData CreateData(Method u, int id)
        {
            var data = new MethodData(db, u);
            return data;
        }

        protected override void ClearEntity(Method source)
        {
            ClearMethod(source);
        }
    }
}