using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIBuildIt.Common.Base
{
    public abstract class Predecessor : Item
    {
        public virtual ICollection<int> PredecessorIds { get; set; }

        public virtual string PredecessorIdsString
        {
            get
            {
                return GetIntString(PredecessorIds);
            }
            set
            {
                PredecessorIds = GetIntCollection(value);
            }
        }

        public bool ShouldSerializePredecessorIds()
        {
            return true;
        }

        public bool ShouldSerializePredecessorIdsString()
        {
            return false;
        }

        public Predecessor()
            : base()
        {
            PredecessorIds = new List<int>();
        }
    }
}
