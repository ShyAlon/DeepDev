using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIBuildIt.Common.Base
{
    public enum RiskLevel
    {
        None,
        VeryLow,
        Low,
        Medium,
        High
    }

    public enum RiskStatus
    {
        Unknown,
        /// <summary>
        /// Understood
        /// </summary>
        Resolved,
        /// <summary>
        /// We crashed into it and are dealing with it
        /// </summary>
        Realized,
        /// <summary>
        /// The Risk was dealt with 
        /// </summary>
        Managed,
        /// <summary>
        /// The risk killed the radio star - including the sequences involved
        /// </summary>
        Terminal
    }

    public enum ExecutionStatus
    {
        New,
        Accepted,
        InProgress,
        PendingApproval,
        Done
    }
}
