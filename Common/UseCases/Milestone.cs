using System;
using System.Collections.Generic;
using UIBuildIt.Common;
using UIBuildIt.Common.Base; //Base;

namespace UIBuildIt.Common.UseCases
{
    public class Milestone : Predecessor, IDeadline
    {
        public DateTime Deadline { get; set; }

        public Milestone() : base()
        {
            //UseCasesIdsString = string.Empty;
            //PredecessorIdsString = string.Empty;
            Deadline = DateTime.UtcNow.AddDays(30.0);
        }

        public Milestone(int id) : this()
        {
            Name = "New Milestone";
            Id = -1;
            
            Description = "A new milestone";
            ParentId = id;
        }

        public bool ShouldSerializeUseCasesIds()
        {
            return true;
        }
        public override bool IsIndexed()
        {
            return true;
        }

        public bool ShouldSerializeUseCasesIdsString()
        {
            return false;
        }
    }
}
