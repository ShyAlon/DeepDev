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
    public  class TestController : APIControllerBase<Test, TestData>
    {
        protected override ICollection<TestData> GetEntities(User user)
        {
            return (from p in db.Projects
                    join r in db.Requirements on p.Id equals r.ParentId
                    join u in db.UseCases on r.Id equals u.ParentId
                    join t in db.Tests on u.Id equals t.ParentId
                    where p.Owner.Id == user.Id
                    select t).ToList().Select(entity => CreateData(entity, entity.Id)).ToList();
        }

        protected override Project GetProject(Test entity)
        {
            return (from p in db.Projects
                    join r in db.Requirements on p.Id equals r.ParentId
                    join u in db.UseCases on r.Id equals u.ParentId
                    where u.Id == entity.ParentId
                    select p).FirstOrDefault();
        }
        protected override IHttpActionResult Validate(Test test)
        {
            var user = (User)Request.Properties["user"];
            var useCase = db.UseCases.FirstOrDefault(p => p.Id == test.ParentId);
            if (useCase == null)
            {
                return Content<string>(HttpStatusCode.BadRequest, string.Format("Parent usecase not found"));
            }
            if (!ModelState.IsValid)
            {
                return Content<System.Web.Http.ModelBinding.ModelStateDictionary>(HttpStatusCode.BadRequest, ModelState);
            }
            return null;
        }

        protected override TestData CreateData(Test t, int id)
        {
            return new TestData(db, t);
        }

        protected override void ClearEntity(Test source)
        {
            ClearTest(source);
        }
    }
}