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
using System.Data.Entity;

namespace UIBuildIt.WebService.Controllers.API.UseCases
{
    public class UseCaseActionController : APIControllerBase<UseCaseAction, UseCaseActionData>
    {

        protected override ICollection<UseCaseActionData> GetEntities(User user)
        {
            return (from p in db.Projects
                    join m in db.Milestones on p.Id equals m.ParentId
                    join r in db.Requirements on m.Id equals r.ParentId
                    join u in db.UseCases on r.Id equals u.ParentId
                    join a in db.Actions on u.Id equals a.ParentId
                    where p.Owner.Id == user.Id
                    select a).ToList().Select(entity => CreateData(entity, entity.Id)).ToList(); ;
        }

        protected override Project GetProject(UseCaseAction entity)
        {
            return (from p in db.Projects
                    join m in db.Milestones on p.Id equals m.ParentId
                    join r in db.Requirements on m.Id equals r.ParentId
                    join u in db.UseCases on r.Id equals u.ParentId
                    where u.Id == entity.ParentId
                    select p).FirstOrDefault();
        }

        protected override IHttpActionResult Validate(UseCaseAction useCaseAction)
        {
            var useCase = db.UseCases.FirstOrDefault(p => p.Id == useCaseAction.ParentId);
            if (useCase == null)
            {
                return Content<string>(HttpStatusCode.NotFound, string.Format("failed to get parent"));
            }
            if (!ModelState.IsValid)
            {
                return Content<string>(HttpStatusCode.BadRequest, ModelState.ToString());
            }
            return null;
        }

        protected override UseCaseActionData CreateData(UseCaseAction u, int id)
        {
            return new UseCaseActionData(db, u);
        }

        protected override void ClearEntity(UseCaseAction source)
        {
            ClearUseCaseAction(source);
        }

        protected override string BeforeAttach(UseCaseAction entity)
        {
            if(string.IsNullOrEmpty(entity.Subject))
            {
                return "Cant attach an action without a subject";
            }
            var module = db.Modules.FirstOrDefault(mod => mod.Name == entity.Subject);
            if(module == null)
            {
                var useCase = db.UseCases.FirstOrDefault(u => u.Id == entity.ParentId);
                var requirement = db.Requirements.FirstOrDefault(r => r.Id == useCase.ParentId);
                var milestone = db.Milestones.FirstOrDefault(m => m.Id == requirement.ParentId);
                module = new Module()
                {
                    Name = entity.Subject,
                    Description = "A new Module for " + entity.Name,
                    ParentId = milestone.ParentId
                };
                db.Modules.Attach(module);
                var entry = db.Entry(module);
                entry.State = EntityState.Added;
                db.SaveChanges();
            }
            return null;
        }
    }
}