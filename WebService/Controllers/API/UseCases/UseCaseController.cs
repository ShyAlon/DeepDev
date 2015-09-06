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
    public class UseCaseController : APIControllerBase<UseCase, UseCaseData>
    {
        protected override ICollection<UseCaseData> GetEntities(User user)
        {
            Item.SetProjectDetails(false);
            return (from p in db.Projects
                    join m in db.Milestones on p.Id equals m.ParentId
                    join r in db.Requirements on m.Id equals r.ParentId
                    join u in db.UseCases on r.Id equals u.ParentId
                    where p.Owner.Id == user.Id
                    select u).ToList().Select(entity => CreateData(entity, entity.Id)).ToList();
        }

        protected override Project GetProject(UseCase entity)
        {
            return (from p in db.Projects
                    join r in db.Requirements on p.Id equals r.ParentId
                    where r.Id == entity.ParentId
                    select p).FirstOrDefault();
        }

        protected override IHttpActionResult Validate(UseCase entity)
        {
            var user = (User)Request.Properties["user"];
            var milestone = db.Requirements.FirstOrDefault(p => p.Id == entity.ParentId);
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

        protected override UseCaseData CreateData(UseCase u, int id)
        {
            var useCaseData = new UseCaseData(db, u);
            return useCaseData;
        }

        protected override void ClearEntity(UseCase source)
        {
            ClearUsecase(source);
        }

        protected override ItemDataBase<UseCase> CreateData(UseCase entity, int id, int parentId, User user)
        {
            if (id != -1)
            {
                throw new NotImplementedException();
            }
            var sourceEntity = db.UseCases.FirstOrDefault(t => t.Id == parentId);
            var sourceData = new UseCaseData(db, sourceEntity);
            var scaffold = new UseCaseData();
            sourceData.Duplicate(db, user, scaffold, 2);
            return scaffold;
        }
    }
}