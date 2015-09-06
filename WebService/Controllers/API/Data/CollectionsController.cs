using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UIBuildIt.Common.Authentication;
using UIBuildIt.Common.Base;
using UIBuildIt.Common.Documents;
using UIBuildIt.Common.Tasks;
using UIBuildIt.Common.UseCases;
using UIBuildIt.Security;

namespace UIBuildIt.WebService.Controllers.API.Data
{
    public class CollectionsController : APIControllerBase<ProjectUser, ProjectUserData>
    {
        protected override ICollection<ProjectUserData> GetEntities(User user)
        {
            throw new NotImplementedException();
        }

        protected override Common.UseCases.Project GetProject(ProjectUser entity)
        {
            throw new NotImplementedException();
        }

        protected override ProjectUserData CreateData(ProjectUser source, int id)
        {
            throw new NotImplementedException();
        }

        protected override IHttpActionResult Validate(ProjectUser entity)
        {
            return null;
        }

        protected override void ClearEntity(ProjectUser source)
        {
            throw new NotImplementedException();
        }

        // GET api/collections
        [AuthorizeToken]
        public override IHttpActionResult Get()
        {
            var user = (User)Request.Properties["user"];
            if (user != null)
            {
                return Content(HttpStatusCode.OK,
                    new
                        {
                            RiskLevels = Enum.GetNames(typeof(RiskLevel)),
                            RiskStatuses = Enum.GetNames(typeof(RiskStatus)),
                            ReturnTypes = Enum.GetNames(typeof(ReturnType)),
                            CallTypes = Enum.GetNames(typeof(CallType)),
                            DocumentTypes = Enum.GetNames(typeof(DocumentType)),
                        });
            }
            return Content<string>(HttpStatusCode.NotFound, "failed to get collections");
        }
    }
}
