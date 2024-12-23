using Code_Record.Server.Models.Base.CommonStructure;

namespace Code_Record.Server.Models.Base;

public partial class Allow: CommonCreation
{
    public string ResourceType { get; set; } = null!;
}
