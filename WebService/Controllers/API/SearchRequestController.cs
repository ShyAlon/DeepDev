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
using System.Text.RegularExpressions;

namespace UIBuildIt.WebService.Controllers.API.UseCases
{
    public class SearchRequestData : ItemData<SearchRequest, SearchRequest>
    {
        public List<SearchResultItem> Items { get; set; }

        public bool Searched { get; set; }

        public bool ShouldSerializeSearched()
        {
            return false;
        }

        public bool ShouldSerializeItems()
        {
            return false;
        }

        public Dictionary<string, List<SearchResultItem>> Results { get; set; }
    }

    public class SearchRequest : Item
    {
        public Dictionary<string, string> Params { get; set; }
        public override bool IsIndexed()
        {
            return false;
        }
    }

    public class SearchResultItem
    {
        public SearchResultItem(){}

        public SearchResultItem(Item t)
        {
            Name = t.Name;
            Description = t.Description;
            Type = t.GetEntityFinalType();
            Id = t.Id;
            Status = string.Empty;
            Risk = string.Empty;
            ParentId = t.ParentId;
            Owner = string.Empty;

            if(t is Task)
            {
                Status = ((Task)t).Status;
                Risk = ((Task)t).RiskStatus;
                Owner = ((Task)t).Owner;
            }
            if (t is Project)
            {
                Owner = ((Project)t).Owner.Name;
            }
            if (t is Method)
            {
                Status = ((Method)t).RiskStatus;
                Risk = ((Method)t).RiskType;
            }
            if (t is Issue)
            {
                Status = ((Issue)t).Status;
            }
        }


        //public SearchResultItem(Item item)
        //{
        //    Name = item.Name;
        //    Description = item.Description;
        //    Type = item.GetType().Name;
        //    Id = item.Id;
        //    Status = string.Empty;
        //    Risk = string.Empty;
        //}
    
        public  string Name { get; set; }
        public  string Description { get; set; }
        public  string Type { get; set; }

        public int Id { get; set; }

        public string Status { get; set; }

        public string Risk { get; set; }

        public string Owner { get; set; }

        public int ParentId { get; set; }
    }

    public class SearchRequestController : APIControllerBase<SearchRequest, SearchRequestData>
    {
        protected override ICollection<SearchRequestData> GetEntities(User user)
        {
            return null;
        }

        [AuthorizeToken]
        public override IHttpActionResult Post(SearchRequest entity)
        {
            var user = (User)Request.Properties["user"];
            ICollection<int> projectsIds = AllowedProjectIds(user);
            var data = new SearchRequestData() { Items = new List<SearchResultItem>() };
            SetTaggedEntities(entity, user, projectsIds, data);

            ExecuteSearch(entity, user, projectsIds, data, SetNamedEntities, FilterEntitiesByName);
           
            ExecuteSearch(entity, user, projectsIds, data, SetStatusEntities, FilterEntitiesByStatus);

            ExecuteSearch(entity, user, projectsIds, data, SetRiskEntities, FilterEntitiesByRisk);

            ExecuteSearch(entity, user, projectsIds, data, SetOwnerEntities, FilterEntitiesByOwner);

            if (data.Items != null)
            {
                var groups = from t in data.Items
                             group t by t.Type into g
                             select g;
                data.Results = new Dictionary<string, List<SearchResultItem>>();
                foreach (var g in groups)
                {
                    data.Results[g.Key] = g.ToList();
                }
            }

            return Content(HttpStatusCode.OK, data);
        }

        private void ExecuteSearch(SearchRequest entity, User user, ICollection<int> projectsIds, SearchRequestData data
            , Action<SearchRequest, User, ICollection<int>, SearchRequestData> set
            , Action<SearchRequest, User, ICollection<int>, SearchRequestData> filter )
        {
            if (!data.Searched || (data.Items.Count > 0))
            {
                if (!data.Searched)
                {
                    set(entity, user, projectsIds, data);
                }
                else
                {
                    filter(entity, user, projectsIds, data);
                }
            }
        }













        private void SetOwnerEntities(SearchRequest entity, Common.Authentication.User user, ICollection<int> projectsIds, SearchRequestData data)
        {
            foreach (var key in entity.Params.Keys)
            {
                if (key.ToString().StartsWith("owner", StringComparison.InvariantCultureIgnoreCase))
                {
                    data.Searched = true;
                    var tag = entity.Params[key];
                    var set = (from t in
                                   (from t in db.Tasks.AsNoTracking()
                                    where projectsIds.Contains(t.ProjectId) && t.Owner.Contains(tag)
                                    select t).ToList()
                               select new SearchResultItem(t)).ToList();
                    data.Items = (data.Items.Concat(set)).ToList();
                    set = (from t in
                               (from t in db.Projects.AsNoTracking()
                                where projectsIds.Contains(t.Id) && (t.Owner.Name.Contains(tag) || t.Owner.Email.Contains(tag))
                                select t).ToList()
                           select new SearchResultItem(t)).ToList();
                    data.Items = (data.Items.Concat(set)).ToList();
                }
            }
        }

        private void FilterEntitiesByOwner(SearchRequest entity, Common.Authentication.User user, ICollection<int> projectsIds, SearchRequestData data)
        {
            var result = new List<SearchResultItem>();
            foreach (var key in entity.Params.Keys)
            {
                if (key.ToString().StartsWith("owner", StringComparison.InvariantCultureIgnoreCase))
                {

                        result = result.Concat((from item in data.Items
                                                where item.Owner.Contains(entity.Params[key])
                                                select item)).Distinct().ToList();

                }
            }
            if (result.Count != 0)
            {
                data.Items = result;
            }
        }

























        private void SetRiskEntities(SearchRequest entity, Common.Authentication.User user, ICollection<int> projectsIds, SearchRequestData data)
        {
            foreach (var key in entity.Params.Keys)
            {
                if (key.ToString().StartsWith("risk", StringComparison.InvariantCultureIgnoreCase))
                {
                    data.Searched = true;
                    RiskLevel status;
                    var name = Regex.Replace(entity.Params[key], @"\s+", "");
                    if (Enum.TryParse<RiskLevel>(name, out status))
                    {
                        var set = (from t in
                                       (from t in db.Tasks.AsNoTracking()
                                        where projectsIds.Contains(t.ProjectId) && t.Risk == status
                                        select t).ToList()
                                   select new SearchResultItem(t)).ToList();
                        data.Items = (data.Items.Concat(set)).ToList();
                        set = (from t in
                                   (from t in db.Methods.AsNoTracking()
                                    where projectsIds.Contains(t.ProjectId) && t.Risk == status
                                    select t).ToList()
                               select new SearchResultItem(t)).ToList();
                        data.Items = (data.Items.Concat(set)).ToList();
                    }
                }
            }
        }

        private void FilterEntitiesByRisk(SearchRequest entity, Common.Authentication.User user, ICollection<int> projectsIds, SearchRequestData data)
        {
            var result = new List<SearchResultItem>();
            foreach (var key in entity.Params.Keys)
            {
                if (key.ToString().StartsWith("risk", StringComparison.InvariantCultureIgnoreCase))
                {
                    RiskLevel status;
                    var name = Regex.Replace(entity.Params[key], @"\s+", "");
                    if (Enum.TryParse<RiskLevel>(name, out status))
                    {
                        result = result.Concat((from item in data.Items
                                                where item.Risk.Contains(entity.Params[key])
                                                select item)).Distinct().ToList();
                    }
                }
            }
            if (result.Count != 0)
            {
                data.Items = result;
            }
        }

        private void SetStatusEntities(SearchRequest entity, Common.Authentication.User user, ICollection<int> projectsIds, SearchRequestData data)
        {
            foreach (var key in entity.Params.Keys)
            {
                if (key.ToString().StartsWith("status", StringComparison.InvariantCultureIgnoreCase))
                {
                    data.Searched = true;
                    IssueStatus status;
                    if (Enum.TryParse<IssueStatus>(entity.Params[key], out status))
                    {
                        var set = (from t in
                                       (from t in db.Tasks.AsNoTracking()
                                        where projectsIds.Contains(t.ProjectId) && t.TaskStatus == status
                                        select t).ToList()
                                   select new SearchResultItem(t)).ToList();
                        data.Items = (data.Items.Concat(set)).ToList();
                        set = (from t in
                                   (from t in db.Issues.AsNoTracking()
                                    where projectsIds.Contains(t.ProjectId) && t.IssueStatus == status
                                    select t).ToList()
                               select new SearchResultItem(t)).ToList();
                        data.Items = (data.Items.Concat(set)).ToList();
                    }
                }
            }
        }

        private void FilterEntitiesByStatus(SearchRequest entity, Common.Authentication.User user, ICollection<int> projectsIds, SearchRequestData data)
        {
            var result = new List<SearchResultItem>();
            foreach (var key in entity.Params.Keys)
            {
                if (key.ToString().StartsWith("status", StringComparison.InvariantCultureIgnoreCase))
                {
                    IssueStatus status;
                    if (Enum.TryParse<IssueStatus>(entity.Params[key], out status))
                    {
                        result = result.Concat((from item in data.Items
                                                where item.Status.Contains(entity.Params[key])
                                                select item)).Distinct().ToList();
                    }
                }
            }
            if (result.Count != 0)
            {
                data.Items = result;
            }
        }

        private void SetNamedEntities(SearchRequest entity, Common.Authentication.User user, ICollection<int> projectsIds, SearchRequestData data)
        {
            foreach (var key in entity.Params.Keys)
            {
                if (key.ToString().StartsWith("name", StringComparison.InvariantCultureIgnoreCase))
                {
                    data.Searched = true;
                    ICollection<Item> entities = db.GetEntitiesWithName(entity.Params[key]);
                    var set = from t in entities
                              where projectsIds.Contains(t.ProjectId) 
                              || (t is User && ((User)t).Organization == user.Organization)
                              || (t is Project && projectsIds.Contains(t.Id))
                               select new SearchResultItem(t);
                    data.Items = (data.Items.Concat(set)).ToList();
                }
            }
        }

        private void FilterEntitiesByName(SearchRequest entity, Common.Authentication.User user, ICollection<int> projectsIds, SearchRequestData data)
        {
            var result = new List<SearchResultItem>();
            foreach (var key in entity.Params.Keys)
            {
                if (key.ToString().StartsWith("name", StringComparison.InvariantCultureIgnoreCase))
                {
                        result = result.Concat((from item in data.Items
                                                where item.Name.Contains(entity.Params[key])
                                                select item)).Distinct().ToList();
                }
            }
            if (result.Count != 0)
            {
                data.Items = result;
            }
        }

        private void SetTaggedEntities(SearchRequest entity, Common.Authentication.User user, ICollection<int> projectsIds, SearchRequestData data)
        {
            foreach (var key in entity.Params.Keys)
            {
                if (key.ToString().StartsWith("tag", StringComparison.InvariantCultureIgnoreCase))
                {
                    data.Searched = true;
                    var tag = entity.Params[key];
                    var result = from t in
                                     (from t in db.TagEntities.AsNoTracking()
                                      where t.Name == tag && projectsIds.Contains(t.ProjectId)
                                      select t).ToList()
                                 select new SearchResultItem(t.GetParent(db));
                    data.Items = (data.Items.Concat(result)).ToList();
                    var users = from u in
                                    (from t in db.TagEntities.AsNoTracking()
                                     join u in db.Users.AsNoTracking() on t.EntityId equals u.Id
                                     where t.Name == tag && t.EntityType == "User" && u.Organization == user.Organization
                                     select u).ToList()
                                select new SearchResultItem(u);
                    data.Items = (data.Items.Concat(users)).ToList();
                }
            }
        }

        private ICollection<int> AllowedProjectIds(User user)
        {
            var owned = (from p in db.Projects.AsNoTracking()
                         where p.Owner.Id == user.Id
                         select p.Id).ToList();
            var defined = (from u in db.ProjectUsers
                           where u.UserMail == user.Email
                           select u.ProjectId).ToList();

            return owned.Concat(defined).Distinct().ToList();
        }

        protected override SearchRequestData CreateData(SearchRequest source, int id)
        {
            throw new NotImplementedException();
        }

        protected override void ClearEntity(SearchRequest source)
        {
            throw new NotImplementedException();
        }

        protected override IHttpActionResult Validate(SearchRequest entity)
        {
            throw new NotImplementedException();
        }

        protected override Project GetProject(SearchRequest entity)
        {
            throw new NotImplementedException();
        }
    }
}