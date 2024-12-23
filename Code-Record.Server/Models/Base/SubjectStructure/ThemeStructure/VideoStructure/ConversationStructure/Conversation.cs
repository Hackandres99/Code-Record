using Code_Record.Server.Models.Base.CommonStructure;

namespace Code_Record.Server.Models.Base.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;

public partial class Conversation : CommonCreation
{
    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string UserEmail { get; set; } = null!;
}
