using Code_Record.Server.Models.Base.CommonStructure;

namespace Code_Record.Server.Models.Base.SubjectStructure.ThemeStructure.VideoStructure;

public partial class Video : CommonUpload
{
    public string Title { get; set; } = null!;

    public TimeOnly Duration { get; set; }

    public string Src { get; set; } = null!;
}
