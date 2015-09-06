using System; //Base;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using UIBuildIt.Common;
using UIBuildIt.Common.Base;

namespace UIBuildIt.Common.UseCases
{
    public class Requirement : Predecessor, IParentType
    {
        /// <summary>
        /// The feature the requirement belongs to
        /// </summary>
        public string Feature { get; set; }

        public Requirement()
            : base()
        {
            ParentType = "Requirement";
            Name = "New requirement";
            Id = -1;
            Description = "A new requirement";
        }

        public Requirement(int id) : this()
        {
            ParentId = id;
        }

        public override ProjectUserType GetOwnerType()
        {
            return ProjectUserType.ProductManager;
        }

        public override bool IsIndexed()
        {
            return true;
        }



        #region IParentType Members

        public string ParentType
        {
            get;
            set;
        }

        #endregion
    }

    public enum ContainerType
    {
        None,
        Milestone,
        Sprint
    }
}
