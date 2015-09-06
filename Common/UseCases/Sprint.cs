using System;
using System.Collections.Generic;
using UIBuildIt.Common;
using UIBuildIt.Common.Base; //Base;

namespace UIBuildIt.Common.UseCases
{
    public class Sprint : Predecessor, IDeadline
    {
        public DateTime Deadline { get; set; }

        public Sprint() : base()
        {
            //UseCasesIdsString = string.Empty;
            //PredecessorIdsString = string.Empty;
            Deadline = DateTime.UtcNow.AddDays(30.0);
        }

        public Sprint(int id)
            : this()
        {
            Name = "New Sprint";
            Id = -1;
            
            Description = "A new Sprint";
            ParentId = id;
        }

        public override bool IsIndexed()
        {
            return true;
        }
    }
}
