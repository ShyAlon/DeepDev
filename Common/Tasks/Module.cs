using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIBuildIt.Common.Base;
using UIBuildIt.Common.UseCases;

namespace UIBuildIt.Common.Tasks
{
    public class Module : Item
    {
        public Module(int id) : this()
        {
            ParentId = id;
        }

        public Module()
            : base()
        {
            Name = "New Module";
            Description = "A new Module";
        }

        public override ProjectUserType GetOwnerType()
        {
            return ProjectUserType.SystemEngineer;
        }

        public override bool IsIndexed()
        {
            return true;
        }

        /// <summary>
        /// The hosting process
        /// </summary>
        public string Process { get; set; }
    }
}
