using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIBuildIt.Common.Base;
using UIBuildIt.Common.UseCases;

namespace UIBuildIt.Common.Tasks
{
    public class Component : Item, IParentType
    {
        public Component(int id): this()
        {
            ParentId = id;
        }

        public Component()
            : base()
        {
            Name = "New Component";
            Description = "A new Component";
        }

        public override bool IsIndexed()
        {
            return true;
        }

        public override ProjectUserType GetOwnerType()
        {
            return ProjectUserType.SystemEngineer;
        }

        #region IParentType Members

        public string ParentType
        {
            get;
            set;
        }

        #endregion
    }
}
