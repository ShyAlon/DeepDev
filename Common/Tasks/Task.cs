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
    public class Task : Predecessor, IParentType
    {
        public int Priority { get; set; }

        #region Effort
        public int Effort { get; set; }

        public int EffortEstimationMean { get; set; }

        public int EffortEstimationOptimistic { get; set; }

        public int EffortEstimationPessimistic { get; set; }

        public int EffortEstimation
        {
            get
            {
                return (4 * EffortEstimationMean + EffortEstimationOptimistic + EffortEstimationPessimistic) / 6;
            }
        }

        public int StandardDeviation
        {
            get
            {
                return (EffortEstimationPessimistic - EffortEstimationOptimistic) / 6;
            }
        }

        #endregion

        #region Timing

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
        #endregion

        public override bool IsIndexed()
        {
            return true;
        }

        /// <summary>
        /// Tasks can sit under every type of entity
        /// </summary>
        public string ParentType { get; set; }

        #region Risk Type

        [NotMapped]
        public virtual string RiskType
        {
            get
            {
                if(StandardDeviation == 0)
                {
                    return RiskLevel.None.ToString();
                }
                var ratio = EffortEstimation / StandardDeviation;
                if (ratio > 48) return RiskLevel.None.ToString();
                if (ratio > 24) return RiskLevel.VeryLow.ToString();
                if (ratio > 12) return RiskLevel.Low.ToString();
                if (ratio > 6) return RiskLevel.Medium.ToString();
                return RiskLevel.High.ToString();
            }
        }

        public RiskLevel Risk { get; set; }

        public bool ShouldSerializeRisk()
        {
            return false;
        }


        #endregion

        #region Risk Status

        [NotMapped]
        public virtual string RiskStatus
        {
            get
            {
                return Enum.GetName(typeof(RiskStatus), RiskStatusStatusE);
            }
            set
            {
                RiskStatus res;
                Enum.TryParse<RiskStatus>(value, out res);
                RiskStatusStatusE = res;
            }
        }

        public bool ShouldSerializeRiskStatusStatusE()
        {
            return false;
        }

        public RiskStatus RiskStatusStatusE { get; set; }

        #endregion

        public string Owner { get; set; }

        public Task()
            : base()
        {
            Name = "New Task";
            RequirementIds = new List<int>();
            Closed = DateTime.MaxValue;
            Description = "A new task";
            Owner = String.Empty;
            ContainerType = ContainerType.None;
            StartTime = DateTime.UtcNow;
            EndTime = DateTime.UtcNow.AddMonths(1);
        }

        public Task(int id) : this()
        {
            Id = -1;
            ParentId = id;
            ParentType = typeof(Project).FullName;
        }

        public Task(string name, string description, Milestone milestone, RiskLevel risk, int effort, int priority, string owner) : this()
        {
            Name = name;
            Id = -1;
            ParentId = milestone.Id;
            Description = description;
            Risk = risk;
            Effort = effort;
            Priority = priority;
            Owner = owner;
        }

        public override ProjectUserType GetOwnerType()
        {
            return ProjectUserType.ProductAndProjectManager;
        }

        #region Status

        public virtual IssueStatus TaskStatus { get; set; }

        [NotMapped]
        public virtual string Status
        {
            get
            {
                return Enum.GetName(typeof(IssueStatus), TaskStatus);
            }
            set
            {
                IssueStatus res;
                Enum.TryParse<IssueStatus>(value, out res);
                if (res != TaskStatus)
                {
                    TaskStatus = res;
                    if (TaskStatus == IssueStatus.New)
                    {
                        Created = DateTime.UtcNow;
                    }
                    if (TaskStatus == IssueStatus.Complete)
                    {
                        Closed = DateTime.UtcNow;
                    }
                    else
                    {
                        Closed = DateTime.MaxValue;
                    }
                }
            }
        }

        public bool ShouldSerializeTaskStatus()
        {
            return false;
        }

        #endregion

        #region Requirements

        public virtual ICollection<int> RequirementIds { get; set; }

        public virtual string RequirementIdsString
        {
            get
            {
                return GetIntString(RequirementIds);
            }
            set
            {
                RequirementIds = GetIntCollection(value);
            }
        }

        public bool ShouldSerializeRequirementIds()
        {
            return true;
        }

        public bool ShouldSerializeRequirementIdsString()
        {
            return false;
        }

        #endregion

        #region Container
        
        public int ContainerID { get; set; }

        [NotMapped]
        public virtual string ContainerEntityType
        {
            get
            {
                return Enum.GetName(typeof(ContainerType), ContainerType);
            }
            set
            {
                ContainerType res;
                Enum.TryParse<ContainerType>(value, out res);
                ContainerType = res;
            }
        }

        public bool ShouldSerializeContainerType()
        {
            return false;
        }

        public ContainerType ContainerType { get; set; }

        #endregion

        public DateTime Closed { get; set; }
    }
}
