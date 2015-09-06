using System; //Base;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using UIBuildIt.Common;
using UIBuildIt.Common.Base;

namespace UIBuildIt.Common.UseCases
{
    public class UseCase : Predecessor
    {
        public int Priority { get; set; }

        public string Result { get; set; }

        public string Initiator { get; set; }

        public override bool IsIndexed()
        {
            return true;
        }

        public UseCase()
            : base()
        {
            Initiator = string.Empty;
            Result = string.Empty;
            Id = -1;
        }

        public UseCase(int id) : this()
        {
            ParentId = id;
        }

        public override ProjectUserType GetOwnerType()
        {
            return ProjectUserType.ProductManager;
        }

        #region Methods

        public virtual ICollection<int> MethodIds { get; set; }

        public virtual string MethodIdsString
        {
            get
            {
                return GetIntString(MethodIds);
            }
            set
            {
                MethodIds = GetIntCollection(value);
            }
        }

        public bool ShouldSerializeMethodIds()
        {
            return true;
        }

        public bool ShouldSerializeMethodIdsString()
        {
            return false;
        }


        #endregion

        #region Comments

        public virtual IDictionary<int, string> MethodComments { get; set; }

        public virtual string MethodCommentsString
        {
            get
            {
                return GetStringsString(MethodComments);
            }
            set
            {
                MethodComments = GetStringCollection(value);
            }
        }

        public bool ShouldSerializeMethodComments()
        {
            return true;
        }

        public bool ShouldSerializeMethodCommentsString()
        {
            return false;
        }

        #endregion

        #region Status
        public virtual ExecutionStatus ExecutionStatus { get; set; }

        [NotMapped]
        public virtual string Status
        {
            get
            {
                return Enum.GetName(typeof(ExecutionStatus), ExecutionStatus);
            }
            set
            {
                ExecutionStatus res;
                Enum.TryParse<ExecutionStatus>(value, out res);
                ExecutionStatus = res;
            }
        }

        public bool ShouldSerializeExecutionStatus()
        {
            return false;
        }

        #endregion
    }
}
