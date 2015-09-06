using UIBuildIt.Common.Base;
namespace UIBuildIt.Common.UseCases
{
    public class Parameter : Item
    {
        /// <summary>
        /// EF can't read Type
        /// </summary>
        public string Type { get; set; }

        public Parameter(int id)
        {
            Name = "New Parameter";
            Id = -1;
            ParentId = id;
            Description = "The parameter's description";
        }

        public Parameter()
            : base()
        { }

        public override ProjectUserType GetOwnerType()
        {
            return ProjectUserType.ProductManager;
        }
        public override bool IsIndexed()
        {
            return true;
        }
    }
}
