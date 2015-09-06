using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIBuildIt.Common.Base;
using UIBuildIt.Common.Tasks;
using UIBuildIt.WebService.Controllers.API;

namespace UIBuildIt.WebService.Models
{
    public abstract class IItemTree
    {
        public ICollection<IItemTree> Children { get; set; }

        public Item Entity { get; set; }

        public string ParentType { get; set; }

        public string Type { get; set; }
    }
    public class ItemTree : IItemTree
    {
        public ItemTree(string type, string parentType)
        {
            Children = new List<IItemTree>();
            Type = type;
            ParentType = parentType;
        }
    }

    public class TaskItemTree : IItemTree
    {
        public TaskItemTree(TaskData source, ICollection<IItemTree> collection)
        {
            Entity = source.Entity;
            Container = source.Container;
            Type = source.Type;
            ParentType = source.Entity.ParentType;
            Creator = source.Creator;
            LastModifier = source.LastModifier;

            source.TotalEffort = ((Task)Entity).Effort;
            source.TotalEffortEstimation = ((Task)Entity).EffortEstimation;

            Children = collection;
            foreach(var child in Children)
            {
                source.TotalEffort += ((Task)(child.Entity)).Effort;
                source.TotalEffortEstimation += ((Task)(child.Entity)).EffortEstimation;
            }

            TotalEffort = source.TotalEffort;
            TotalEffortEstimation = source.TotalEffortEstimation;
        }

        public ItemDataPrimitive Container { get; set; }

        public string LastModifier { get; set; }

        public string Creator { get; set; }

        public int TotalEffortEstimation { get; set; }

        public int TotalEffort { get; set; }
    }
}
