using Newtonsoft.Json;

namespace Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;

public partial class Conversation : Base.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure.Conversation
{
    public Guid Id { get; set; }

	public Guid IdVideo { get; set; }

	public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

	[JsonIgnore]
	public virtual Video? IdVideoNavigation { get; set; }

	[JsonIgnore]
	public virtual User? UserEmailNavigation { get; set; }
}
