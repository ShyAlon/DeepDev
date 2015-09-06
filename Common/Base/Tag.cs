using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UIBuildIt.Common.Authentication;
using UIBuildIt.Common.UseCases;

namespace UIBuildIt.Common.Base
{
    /// <summary>
    /// Represents a persisted data item
    /// </summary>
    public class TagEntity : Item
    {
        public int EntityId { get; set; }

        public int TagId { get; set; }
        //TODO - we reall need a type table
        public string EntityType { get; set; }

        public override bool IsIndexed()
        {
            return false;
        }
    }

    public interface IParentType
    {
        string ParentType { get; set; }
    }

    public interface IDeadline
    {
        DateTime Deadline { get; set; }
    }

    public interface IOwner
    {
        User Owner { get; set; }
    }
}
