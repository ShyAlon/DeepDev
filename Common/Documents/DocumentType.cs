using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIBuildIt.Common.Base;
using UIBuildIt.Common.UseCases;

namespace UIBuildIt.Common.Documents
{
    /// <summary>
    /// The type of the document to generate
    /// </summary>
    public enum DocumentType
    {
        DetailedRequirements,
        HighLevelDesign,
        ProjectStatus,
        Gantt,
        CostEstimationsHourly, // (Effort * PriceTag)
        AcceptanceTest,
        WBS
    }
}
