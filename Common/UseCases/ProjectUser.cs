using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIBuildIt.Common.Base;

namespace UIBuildIt.Common.UseCases
{
    /// <summary>
    /// Many to many connection between users and projects
    /// </summary>
    public class ProjectUser : Item
    {
        public string UserMail { get; set; }
        public ProjectUserType UserType { get; set; }

        public ProjectUser() : base()
        {

        }

        public override bool IsIndexed()
        {
            return false;
        }
    }

    [Flags]
    public enum ProjectUserType
    {
        Owner = -1,
        None = 0,
        ProductManager = 1, 
        ProjectManager = 2, 
        ProductAndProjectManager = 3,
        SystemEngineer = 4,
        ProductAndSystemEngineer = 5,
        ProjectAndSystemEngineer = 6,
    }
}
