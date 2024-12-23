using Code_Record.Server.Models.Base.CommonStructure;

namespace Code_Record.Server.Models.Base.SubjectStructure.TestStructure;

public partial class Question : CommonCreation
{
    public string Question1 { get; set; } = null!;

    public string Anwser { get; set; } = null!;
}
