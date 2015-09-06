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
    public class ParameterController : APIControllerBase<Parameter, ParameterData>
    {
        protected override ICollection<ParameterData> GetEntities(User user)
        {
            return (from p in db.Projects
                    join m in db.Milestones on p.Id equals m.ParentId
                    join r in db.Requirements on m.Id equals r.ParentId
                    join u in db.UseCases on r.Id equals u.ParentId
                    join a in db.Actions on u.Id equals a.ParentId
                    join par in db.Parameters on a.Id equals par.ParentId
                    where p.Owner.Id == user.Id
                    select par).ToList().Select(entity => CreateData(entity, entity.Id)).ToList(); ;
        }

        protected override Project GetProject(Parameter entity)
        {
            return (from p in db.Projects
                    join m in db.Milestones on p.Id equals m.ParentId
                    join r in db.Requirements on m.Id equals r.ParentId
                    join u in db.UseCases on r.Id equals u.ParentId
                    join a in db.Actions on u.Id equals a.ParentId
                    where a.Id == entity.ParentId
                    select p).FirstOrDefault();
        }

        protected override IHttpActionResult Validate(Parameter parameter)
        {
            var user = (User)Request.Properties["user"];
            var action = db.Actions.FirstOrDefault(p => p.Id == parameter.ParentId);
            if (action == null)
            {
                return Content<string>(HttpStatusCode.NotFound, string.Format("parent not found"));
            }
            if (!ModelState.IsValid)
            {
                return Content<string>(HttpStatusCode.BadRequest, ModelState.ToString());
            }
            return null;
        }

        protected override ParameterData CreateData(Parameter p, int id)
        {
            var parameter = new ParameterData()
            {
                Entity = p
            };

            return parameter;
        }

        protected override void ClearEntity(Parameter source)
        {
            ClearParameter(source);
        }
    }
}