using System.Collections.Generic;
using UIBuildIt.Common;
using UIBuildIt.Common.Base; //Base;

namespace UIBuildIt.Common.UseCases
{
    public class Test : Predecessor
    {
        public string Result { get; set; }

        public Test() : base() { }

        public Test(int id)
        {
            Name = "New test";
            Id = -1;
            ParentId = id;
            Description = "A new test";
        }

        public override ProjectUserType GetOwnerType()
        {
            return ProjectUserType.ProductManager;
        }

        public override bool IsIndexed()
        {
            return true;
        }
    }
}
