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
    public class TaskController : APIControllerBase<Task, TaskData>
    {
        protected override ICollection<TaskData> GetEntities(User user)
        {
            Item.SetProjectDetails(false);
            return (from p in db.Projects
                    join m in db.Milestones on p.Id equals m.ParentId
                    join r in db.Requirements on m.Id equals r.ParentId
                    join t in db.Tasks on r.Id equals t.ParentId
                    where p.Owner.Id == user.Id
                    select t).ToList().Select(entity => CreateData(entity, entity.Id)).ToList();
        }

        protected override Project GetProject(Task entity)
        {
            return (Project)ItemData<Task, Item>.GetProject(entity, db);
        }

        protected override IHttpActionResult Validate(Task entity)
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

        protected override TaskData CreateData(Task entity, int id)
        {
            var user = (User)Request.Properties["user"];
            var taskData = new TaskData(db, entity);  
            var tags = entity.GetTags(db);
            var users = (from u in db.Users
                         where u.Organization == user.Organization
                         select u).Distinct().ToList();

            var userNames = from u in users
                            orderby u.Closer(tags, db) descending
                            select u.Email;

            taskData.Owners = userNames.Concat(taskData.Owners).Distinct().ToList();
            return taskData;
        }

        

        protected override void ClearEntity(Task source)
        {
            ClearTask(source);
        }
    }
}