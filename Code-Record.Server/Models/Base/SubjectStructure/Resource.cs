using Code_Record.Server.Models.Base.CommonStructure;

namespace Code_Record.Server.Models.Base.SubjectStructure;

public partial class Resource : CommonUpload
{
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

	public string ResourceContent { get; set; } = null!;

	public string Src { get; set; } = null!;

    public string Tag { get; set; } = null!;
}
