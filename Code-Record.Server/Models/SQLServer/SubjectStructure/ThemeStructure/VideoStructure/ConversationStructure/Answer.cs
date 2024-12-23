using Newtonsoft.Json;

namespace Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;

public partial class Answer : Base.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure.Answer
{
	public Guid Id { get; set; }

	public Guid IdComment { get; set; }

	[JsonIgnore]
	public virtual Comment? IdCommentNavigation { get; set; }

	[JsonIgnore]
	public virtual User? UserEmailNavigation { get; set; }

	[JsonIgnore]
	public virtual User? UserMentionNavigation { get; set; }
}
