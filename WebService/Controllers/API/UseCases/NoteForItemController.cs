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
    public class TaskForItemController : TaskController
    {
        protected override TaskData CreateData(Task t, int id)
        {
            return CreateDataFor(t, id);
        }

        protected override TaskData CreateData(Task source, int id, string parentType)
        {
            var data = CreateDataFor(source, id, null, parentType);
            data.ParentType = parentType;
            TaskData.SetParent(db, parentType, data);
            // var project = ItemData<Note, Item>.GetProject(data.Parent, db);
            data.Entity.ProjectId = data.ParentItem.ProjectId;
            db.SaveChanges();
            return data;
        }
    }
}