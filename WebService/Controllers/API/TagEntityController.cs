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

namespace UIBuildIt.WebService.Controllers.API.UseCases
{
    public class TagEntityData : ItemData<TagEntity, Item>
    {

    }

    public class TagEntityController : APIControllerBase<TagEntity, TagEntityData>
    {
        protected override ICollection<TagEntityData> GetEntities(User user)
        {
            //TODO: Limit to the organization
            var result = (from t in
                              (from t in db.TagEntities.AsNoTracking()
                               join p in db.Projects.AsNoTracking() on t.ProjectId equals p.Id
                               where user.Id == p.Owner.Id
                               select t).ToList()
                          select new TagEntityData() { Entity = t }).ToList();

            return result;
        }

        protected override TagEntityData CreateData(TagEntity source, int id)
        {
            return new TagEntityData() { Entity = source };
        }

        protected override void ClearEntity(TagEntity source)
        {
            ClearTag(source);
        }

        protected override IHttpActionResult Validate(TagEntity entity)
        {
            var tag = (from t in db.TagEntities
                       where t.Name == entity.Name && t.EntityType == entity.EntityType && t.Id == entity.Id
                       select t).FirstOrDefault();
            if(tag != null)
            {
                return Content<string>(HttpStatusCode.Conflict, string.Format("Tag already exists"));
            }
            return null;
        }

        protected override Project GetProject(TagEntity entity)
        {
            Item parent = entity.GetParent(db);
            if (parent == null)
            {
                throw new ArgumentException("No parent for " + entity.Name);
            }
            parent.ParentId = 7;
            return (Project)TagEntityData.GetProject(parent, db);
        }
    }
}