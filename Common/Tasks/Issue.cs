using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIBuildIt.Common.Base;
using UIBuildIt.Common.UseCases;

namespace UIBuildIt.Common.Tasks
{
    /// <summary>
    /// Represents a known issue with a specific sequence
    /// </summary>
    public class Issue : Item
    {
        public Issue()
            : base()
        {
            Name = "New Issue";
            Description = "A new issue";
            ClosedTime = DateTime.MaxValue;
        }

        public Issue(int id)
            : this()
        {
            ParentId = id;
        }

        public DateTime ClosedTime { get; set; }

        public int Effort { get; set; }

        public int EffortEstimation { get; set; }

        public override bool IsIndexed()
        {
            return true;
        }

        public int Priority { get; set; }

        #region Status

        public virtual IssueStatus IssueStatus { get; set; }

        [NotMapped]
        public virtual string Status
        {
            get
            {
                return Enum.GetName(typeof(IssueStatus), IssueStatus);
            }
            set
            {
                IssueStatus res;
                Enum.TryParse<IssueStatus>(value, out res);
                if (res != IssueStatus)
                {
                    IssueStatus = res;
                    if (IssueStatus == IssueStatus.New)
                    {
                        Modified = DateTime.UtcNow;
                    }
                    if (IssueStatus == IssueStatus.Complete)
                    {
                        ClosedTime = DateTime.UtcNow;
                    }
                    else
                    {
                        ClosedTime = DateTime.MaxValue;
                    }
                }
            }
        }

        public bool ShouldSerializeIssueStatus()
        {
            return false;
        }

        #endregion

        public override ProjectUserType GetOwnerType()
        {
            return ProjectUserType.None;
        }

        #region Requirements

        public virtual ICollection<int> TestIds { get; set; }

        public virtual string TestIdsString
        {
            get
            {
                return GetIntString(TestIds);
            }
            set
            {
                TestIds = GetIntCollection(value);
            }
        }

        public bool ShouldSerializeTestIds()
        {
            return true;
        }

        public bool ShouldSerializeTestIdsString()
        {
            return false;
        }

        #endregion
    }

    /// <summary>
    /// Execution Status
    /// </summary>
    public enum IssueStatus
    {
        New,
        Accepted,
        Rejected,
        InProgress,
        WontFix,
        PendingApproval,
        Complete
    }
}

