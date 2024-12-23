using Code_Record.Server.Models.SQLServer.SubjectStructure.TestStructure;
using Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;

namespace Code_Record.Server.Models.SQLServer;

public partial class User : Base.UserStructure.User
{
	public Guid Id { get; set; }

	public virtual ICollection<Allow> Allows { get; set; } = new List<Allow>();

	public virtual ICollection<Answer> AnwserUserEmailNavigations { get; set; } = new List<Answer>();

    public virtual ICollection<Answer> AnwserUserMentionNavigations { get; set; } = new List<Answer>();

    public virtual ICollection<Comment> CommentUserEmailNavigations { get; set; } = new List<Comment>();

    public virtual ICollection<Comment> CommentUserMentionNavigations { get; set; } = new List<Comment>();

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
