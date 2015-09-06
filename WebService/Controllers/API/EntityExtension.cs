using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIBuildIt.Common.Authentication;
using UIBuildIt.Common.UseCases;
using UIBuildIt.Models;

namespace UIBuildIt.WebService.Controllers.API
{
    public static class EntityExtension
    {
        public static ProjectUserType GetUserType(this User user, Project project, UIBuildItContext db)
        {
            using (MiniProfiler.Current.Step("Get user type for " + user.Name))
            {
                if (project == null) // Not associated with a project
                {
                    return ProjectUserType.Owner;
                }
                if (project.Owner == null)
                {
                    // Creator becomes owner
                    return ProjectUserType.Owner;
                }
                if (user.Id == project.Owner.Id)
                {
                    return ProjectUserType.Owner;
                }
                var projectUsers = (from pu in db.ProjectUsers
                                    where pu.UserMail == user.Email && pu.ProjectId == project.Id
                                    select pu).ToList();
                if (projectUsers == null || projectUsers.Count == 0)
                {
                    return ProjectUserType.None;
                }
                var result = ProjectUserType.None;
                foreach (var pu in projectUsers)
                {
                    result |= pu.UserType;
                }
                return result;
            }
        }

        public static bool IsOwner(this User user, Project project)
        {
            return user.Id == project.Owner.Id;
        }

        public static bool IsProductManager(this User user, Project project, UIBuildItContext db)
        {
            return user.IsOwner(project) || 
                (ProjectUserType.ProductManager & user.GetUserType(project, db)) == ProjectUserType.ProductManager;
        }

        public static bool IsProjectManager(this User user, Project project, UIBuildItContext db)
        {
            return user.IsOwner(project) || 
                (ProjectUserType.ProjectManager & user.GetUserType(project, db)) == ProjectUserType.ProjectManager;
        }

        public static bool IsSystemEngineer(this User user, Project project, UIBuildItContext db)
        {
            return user.IsOwner(project) || 
                (ProjectUserType.SystemEngineer & user.GetUserType(project, db)) == ProjectUserType.SystemEngineer;
        }
    }
}
