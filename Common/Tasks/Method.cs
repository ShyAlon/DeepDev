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
    public class Method : Item
    {
        public int TargetId { get; set; }

        public RiskLevel Risk { get; set; }

        public string RiskDescription { get; set; }

        /// <summary>
        /// Split by either a ; or a ,
        /// </summary>
        public string Parameters { get; set; }

        public Method(int id) : this()
        {
            ParentId = id;
        }

        public override bool IsIndexed()
        {
            return true;
        }

        public Method()
            : base()
        {
            Name = "New Method";
            Description = "A new Method";
        }

        public string Return { get; set; }

        #region Return Type

        public virtual ReturnType ReturnType { get; set; }

        [NotMapped]
        public virtual string ReturnMode
        {
            get
            {
                return Enum.GetName(typeof(ReturnType), ReturnType);
            }
            set
            {
                ReturnType res;
                Enum.TryParse<ReturnType>(value, out res);
                ReturnType = res;
            }
        }

        public bool ShouldSerializeReturnType()
        {
            return false;
        }

        #endregion

        [NotMapped]
        public virtual string RiskType
        {
            get
            {
                return Enum.GetName(typeof(RiskLevel), Risk);
            }
            set
            {
                RiskLevel res;
                Enum.TryParse<RiskLevel>(value, out res);
                Risk = res;
            }
        }

        public bool ShouldSerializeRisk()
        {
            return false;
        }

        #region Status

        [NotMapped]
        public virtual string RiskStatus
        {
            get
            {
                return Enum.GetName(typeof(RiskStatus), Status);
            }
            set
            {
                RiskStatus res;
                Enum.TryParse<RiskStatus>(value, out res);
                Status = res;
            }
        }
        
        public bool ShouldSerializeStatus()
        {
            return false;
        }

        public RiskStatus Status { get; set; }

        #endregion

        

        public override ProjectUserType GetOwnerType()
        {
            return ProjectUserType.SystemEngineer;
        }

        #region Return Type

        public virtual CallType CallType { get; set; }

        [NotMapped]
        public virtual string CallMode
        {
            get
            {
                return Enum.GetName(typeof(CallType), CallType);
            }
            set
            {
                CallType res;
                Enum.TryParse<CallType>(value, out res);
                CallType = res;
            }
        }

        public bool ShouldSerializeCallType()
        {
            return false;
        }

        #endregion
    }

    public enum ReturnType
    {
        Sync,
        ASync,
        None
    }

    public enum CallType
    {
        
        InProcess,
        InterProcess,
        COM,
        IP,
        HTTP,
        HTTPS,
        UserInteraction,
    }
}
