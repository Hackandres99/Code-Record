using Newtonsoft.Json;

namespace Code_Record.Server.Models.SQLServer.SubjectStructure.TestStructure;

public partial class Result : Base.SubjectStructure.TestStructure.Result
{
	public Guid Id { get; set; }

	public Guid IdTest { get; set; }

	[JsonIgnore]
	public virtual Test? IdTestNavigation { get; set; }

	[JsonIgnore]
	public virtual User? UserEmailNavigation { get; set; }
}
