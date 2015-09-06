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
    public class IssueController : APIControllerBase<Issue, IssueData>
    {
        protected override ICollection<IssueData> GetEntities(User user)
        {
            Item.SetProjectDetails(false);
            return (from p in db.Projects
                    join m in db.Milestones on p.Id equals m.ParentId
                    join t in db.Tasks on m.Id equals t.ParentId
                    join i in db.Issues on t.Id equals i.ParentId
                    where p.Owner.Id == user.Id
                    select i).ToList().Select(entity => CreateData(entity, entity.Id)).ToList();
        }

        protected override Project GetProject(Issue entity)
        {
            return (from p in db.Projects
                    join t in db.Tasks on p.Id equals t.ProjectId
                    where t.Id == entity.ParentId
                    select p).FirstOrDefault();
        }

        protected override IHttpActionResult Validate(Issue entity)
        {
            var parent = db.Tasks.FirstOrDefault(p => p.Id == entity.ParentId);
            if (parent == null)
            {
                return Content<string>(HttpStatusCode.NotFound, string.Format("failed to get parent"));
            }
            if (!ModelState.IsValid)
            {
                return Content<string>(HttpStatusCode.BadRequest, ModelState.ToString());
            }
            return null;
        }

        protected override IssueData CreateData(Issue u, int id)
        {
            var data = new IssueData(db, u);
            return data;
        }

        protected override void ClearEntity(Issue source)
        {
            ClearIssue(source);
        }
    }
}