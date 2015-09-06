using System;
using System.Collections.Generic;
using UIBuildIt.Common.Authentication;
using UIBuildIt.Common.Base;
namespace UIBuildIt.Common.UseCases
{
    /// <summary>
    /// A defined system
    /// </summary>
    public class Project : Item, IDeadline, IOwner
    {
        public virtual User Owner { get; set; }

        public override bool IsIndexed()
        {
            return false;
        }

        /// <summary>
        /// Ugh.  Please don't
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeOwner()
        {
            return false;
        }
        public string Title { get; set; }

        public int PriceTag { get; set; }
        public DateTime Deadline { get; set; }

        public Project()
            : base()
        {
            Deadline = DateTime.UtcNow.AddMonths(3);
        }
    }

}
