﻿using System;
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
    public class RequirementForProjectController : RequirementController
    {
        protected override RequirementData CreateData(Requirement r, int id)
        {
            return CreateDataFor(null, id, null, "Project");
        }
    }
}