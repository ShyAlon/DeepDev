using UIBuildIt.Common.Base;
namespace UIBuildIt.Common.UseCases
{
    public class TestParameter : Item
    {
        public string Value { get; set; }

        public int ParameterId { get; set; }

        public TestParameter(int parameter, int test)
        {
            Name = string.Empty;
            Id = -1;
            ParentId = test;
            ParameterId = parameter;
            Description = string.Empty;
        }

        public TestParameter(int parameter) : this(parameter, -1)
        {
            
        }

        public override bool IsIndexed()
        {
            return true;
        }

        public TestParameter()
            : base()
        { }

        public override ProjectUserType GetOwnerType()
        {
            return ProjectUserType.ProductManager;
        }
    }
}
