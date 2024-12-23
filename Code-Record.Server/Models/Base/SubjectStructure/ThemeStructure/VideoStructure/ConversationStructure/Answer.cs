using Code_Record.Server.Models.Base.CommonStructure;

namespace Code_Record.Server.Models.Base.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;

public partial class Answer : CommonCreation
{
    public string Message { get; set; } = null!;

    public string UserMention { get; set; } = null!;

    public string UserEmail { get; set; } = null!;
}
