using Newtonsoft.Json;

namespace Code_Record.Server.Models.SQLServer.SubjectStructure.TestStructure;

public partial class Question : Base.SubjectStructure.TestStructure.Question
{
	public Guid Id { get; set; }

	public Guid IdTest { get; set; }

	[JsonIgnore]
	public virtual Test? IdTestNavigation { get; set; }

	public virtual ICollection<Option> Options { get; set; } = new List<Option>();
}
