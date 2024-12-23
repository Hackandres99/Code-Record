using Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;

namespace Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure.VideoStructure;

public partial class Video : Base.SubjectStructure.ThemeStructure.VideoStructure.Video
{
	public Guid Id { get; set; }

	public Guid? IdTheme { get; set; }

	public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual Theme? IdThemeNavigation { get; set; }
}
