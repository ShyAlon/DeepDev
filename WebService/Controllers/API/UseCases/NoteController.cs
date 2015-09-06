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
    public class NoteController : APIControllerBase<Note, NoteData>
    {
        protected override ICollection<NoteData> GetEntities(User user)
        {
            Item.SetProjectDetails(false);

            return (from n in db.Notes
                    join p in db.Projects on n.ProjectId equals p.Id
                    where p.Owner.Id == user.Id
                    select n).ToList().Select(entity => CreateData(entity, entity.Id)).ToList();
        }

        protected override Project GetProject(Note entity)
        {
            return (Project)ItemData<Note, Item>.GetProject(entity, db);
        }

        protected override IHttpActionResult Validate(Note entity)
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

        protected override NoteData CreateData(Note entity, int id)
        {
            var taskData = new NoteData(db, entity);
            return taskData;
        }

        protected override void ClearEntity(Note source)
        {
            ClearNote(source);
        }
    }
}