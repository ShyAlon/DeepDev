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
using UIBuildIt.Common.Tasks;

namespace UIBuildIt.WebService.Controllers.API.Tasks
{
    public class IssueForTaskController : IssueController
    {
        protected override IssueData CreateData(Issue i, int id)
        {
            return CreateDataFor(null, id);
        }
    }
}