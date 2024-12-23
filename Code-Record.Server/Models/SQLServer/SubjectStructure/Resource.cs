using Newtonsoft.Json;

namespace Code_Record.Server.Models.SQLServer.SubjectStructure;

public partial class Resource : Base.SubjectStructure.Resource
{
	public Guid Id { get; set; }

	public Guid? IdSubject { get; set; }

	[JsonIgnore]
	public virtual Subject? IdSubjectNavigation { get; set; }
}
