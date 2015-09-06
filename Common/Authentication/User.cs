using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIBuildIt.Common.Base;
using UIBuildIt.Common.UseCases;

namespace UIBuildIt.Common.Authentication
{
    public  class User : Item
    {
        /// <summary>
        /// Of course this will be replaced in production
        /// </summary>
        public string Password { get; set; }

        public string Email { get; set; }

        public User()
        {

        }

        public override bool IsIndexed()
        {
            return false;
        }

        public override ProjectUserType GetOwnerType()
        {
            return ProjectUserType.None;
        }

        //public bool ShouldSerializeProjects()
        //{
        //    return false;
        //}

        /// <summary>
        /// A unique identifier for the user's organization
        /// </summary>
        public string Organization { get; set; }
    }
   
}
