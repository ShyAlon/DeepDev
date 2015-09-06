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
    /// A free text note to attach to any entity which can be displayed in a document.
    /// It can be converted to an entity later.
    /// </summary>
    public class Note : Item, IParentType
    {
        /// <summary>
        /// Tasks can sit under every type of entity
        /// </summary>
        public string ParentType { get; set; }

        public Note()
            : base()
        {
            Name = "New Note";
            Description = "A new note";
        }

        public Note(int id) : this()
        {
            Id = -1;
            ParentId = id;
            ParentType = typeof(Project).FullName;
        }

        public override ProjectUserType GetOwnerType()
        {
            return ProjectUserType.None;
        }

        public override bool IsIndexed()
        {
            return true;
        }

        public bool DisplayInRequirements { get; set; }

        public bool DisplayInDesign { get; set; }

        public bool DisplayInTasks { get; set; }

        /// <summary>
        /// Display the notes at the top of the entity. Otherwise will be displayed at the bottom.
        /// </summary>
        public bool DisplayAtTop { get; set; }
    }
}
