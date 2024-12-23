using Code_Record.Server.Models.Base.CommonStructure;

namespace Code_Record.Server.Models.Base.SubjectStructure;

public partial class Subject : CommonCreation
{
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Tool { get; set; } = null!;

    public string Image { get; set; } = null!;

    public string Link { get; set; } = null!;
}
