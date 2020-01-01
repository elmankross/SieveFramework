using SieveFramework.Attributes;

namespace SieveFrameworkTests.Models
{
    public class AttributedTestModel
    {
        [CanSort]
        public int Number { get; set; }
        [CanFilter]
        public string String { get; set; }
        public bool Boolean { get; set; }
    }
}
