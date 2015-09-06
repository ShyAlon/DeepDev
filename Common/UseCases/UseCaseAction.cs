using System.Collections.Generic;
using UIBuildIt.Common;
using UIBuildIt.Common.Base; //Base;

namespace UIBuildIt.Common.UseCases
{
    public class UseCaseAction : Predecessor
    {
        /// <summary>
        /// The object performing the action
        /// </summary>
        public string Object { get; set; }

        /// <summary>
        /// The subject of the action
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// What the object does to the subject
        /// </summary>
        public string Verb { get; set; }

        public override bool IsIndexed()
        {
            return true;
        }

        public UseCaseAction()
            : base()
        {
        }

        public UseCaseAction(int id)
        {
            Name = "New Action";
            Id = -1;
            ParentId = id;
            Description = "New Action";
        }

        public override ProjectUserType GetOwnerType()
        {
            return ProjectUserType.ProductManager;
        }
    }
}
