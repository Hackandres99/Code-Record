using Newtonsoft.Json;

namespace Code_Record.Server.Models.SQLServer.SubjectStructure.TestStructure;

public partial class Option : Base.SubjectStructure.TestStructure.Option
{
	public Guid Id { get; set; }

	public Guid IdQuestion { get; set; }

	[JsonIgnore]
	public virtual Question? IdQuestionNavigation { get; set; }
}
