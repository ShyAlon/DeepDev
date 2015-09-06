using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using UIBuildIt.Common;
using UIBuildIt.Common.Authentication;
using UIBuildIt.Common.Base;
using UIBuildIt.Common.Tasks;
using UIBuildIt.Common.UseCases;

namespace UIBuildIt.Models
{
    public class UIBuildItContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public UIBuildItContext() : base("name=UIBuildItContext")
        {
        }
        public System.Data.Entity.DbSet<Project> Projects { get; set; }

        public System.Data.Entity.DbSet<Milestone> Milestones { get; set; }

        public System.Data.Entity.DbSet<User> Users { get; set; }

        public System.Data.Entity.DbSet<Token> Tokens { get; set; }

        public System.Data.Entity.DbSet<UseCase> UseCases { get; set; }

        public System.Data.Entity.DbSet<UseCaseAction> Actions { get; set; }

        public System.Data.Entity.DbSet<Parameter> Parameters { get; set; }

        public System.Data.Entity.DbSet<Test> Tests { get; set; }

        public System.Data.Entity.DbSet<TestParameter> TestParameters { get; set; }

        public System.Data.Entity.DbSet<Requirement> Requirements { get; set; }

        public System.Data.Entity.DbSet<Task> Tasks { get; set; }

        public System.Data.Entity.DbSet<Method> Methods { get; set; }

        public System.Data.Entity.DbSet<Component> Components { get; set; }

        public System.Data.Entity.DbSet<Module> Modules { get; set; }

        public System.Data.Entity.DbSet<Issue> Issues { get; set; }

        public DbSet<ProjectUser> ProjectUsers { get; set; }

        public DbSet<Note> Notes { get; set; }

        public DbSet<Sprint> Sprints { get; set; }

        public DbSet<TagEntity> TagEntities { get; set; }

        internal ICollection<Item> GetEntitiesWithName(string name)
        {
            var result = new List<Item>();
            result = (result.Concat(from p in (from p in Projects where p.Name.Contains(name) select p).ToList() select (Item)p)).ToList();
            result = (result.Concat(from p in (from p in Milestones where p.Name.Contains(name) select p).ToList() select (Item)p)).ToList();
            result = (result.Concat(from p in (from p in Sprints where p.Name.Contains(name) select p).ToList() select (Item)p)).ToList();
            result = (result.Concat(from p in (from p in Issues where p.Name.Contains(name) select p).ToList() select (Item)p)).ToList();
            result = (result.Concat(from p in (from p in Notes where p.Name.Contains(name) select p).ToList() select (Item)p)).ToList();
            result = (result.Concat(from p in (from p in Tasks where p.Name.Contains(name) select p).ToList() select (Item)p)).ToList();
            result = (result.Concat(from p in (from p in Requirements where p.Name.Contains(name) select p).ToList() select (Item)p)).ToList();
            result = (result.Concat(from p in (from p in UseCases where p.Name.Contains(name) select p).ToList() select (Item)p)).ToList();
            result = (result.Concat(from p in (from p in Methods where p.Name.Contains(name) select p).ToList() select (Item)p)).ToList();
            result = (result.Concat(from p in (from p in Components where p.Name.Contains(name) select p).ToList() select (Item)p)).ToList();
            result = (result.Concat(from p in (from p in Modules where p.Name.Contains(name) select p).ToList() select (Item)p)).ToList();
            result = (result.Concat(from p in (from p in Users where p.Name.Contains(name) select p).ToList() select (Item)p)).ToList();
            return result;
        }

        internal ICollection<Item> GetEntitiesWithProjectId(int id)
        {
            var proxyCreationEnabled = Configuration.ProxyCreationEnabled;
            try
            {
                using (MiniProfiler.Current.Step("GetEntitiesWithProjectId " + id.ToString()))
                {
                    Configuration.ProxyCreationEnabled = false;
                    var result = new List<Item>();
                    result = (result.Concat(from p in (from p in Projects.AsNoTracking() where p.Id == id select p).ToList() select (Item)p)).ToList();
                    result = (result.Concat(from p in (from p in Milestones.AsNoTracking() where p.ProjectId == id select p).ToList() select (Item)p)).ToList();
                    result = (result.Concat(from p in (from p in Sprints.AsNoTracking() where p.ProjectId == id select p).ToList() select (Item)p)).ToList();
                    result = (result.Concat(from p in (from p in Issues.AsNoTracking() where p.ProjectId == id select p).ToList() select (Item)p)).ToList();
                    result = (result.Concat(from p in (from p in Notes.AsNoTracking() where p.ProjectId == id select p).ToList() select (Item)p)).ToList();
                    result = (result.Concat(from p in (from p in Tasks.AsNoTracking() where p.ProjectId == id select p).ToList() select (Item)p)).ToList();
                    result = (result.Concat(from p in (from p in Requirements.AsNoTracking() where p.ProjectId == id select p).ToList() select (Item)p)).ToList();
                    result = (result.Concat(from p in (from p in UseCases.AsNoTracking() where p.ProjectId == id select p).ToList() select (Item)p)).ToList();
                    result = (result.Concat(from p in (from p in Methods.AsNoTracking() where p.ProjectId == id select p).ToList() select (Item)p)).ToList();
                    result = (result.Concat(from p in (from p in Components.AsNoTracking() where p.ProjectId == id select p).ToList() select (Item)p)).ToList();
                    result = (result.Concat(from p in (from p in Modules.AsNoTracking() where p.ProjectId == id select p).ToList() select (Item)p)).ToList();
                    result = (result.Concat(from p in (from p in Users.AsNoTracking() where p.ProjectId == id select p).ToList() select (Item)p)).ToList();
                    return result;
                }
            }
            finally
            {
                Configuration.ProxyCreationEnabled = proxyCreationEnabled;
            }
            
        }
    }
}
