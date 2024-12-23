using Newtonsoft.Json;

namespace Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;

public partial class Comment : Base.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure.Comment
{
	public Guid Id { get; set; }

	public Guid IdConversation { get; set; }

	public virtual ICollection<Answer> Anwsers { get; set; } = new List<Answer>();

	[JsonIgnore]
	public virtual Conversation? IdConversationNavigation { get; set; }

	[JsonIgnore]
	public virtual User? UserEmailNavigation { get; set; }

	[JsonIgnore]
	public virtual User? UserMentionNavigation { get; set; }
}
