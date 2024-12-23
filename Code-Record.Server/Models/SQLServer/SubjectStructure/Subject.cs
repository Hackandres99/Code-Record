using Code_Record.Server.Models.SQLServer.SubjectStructure.TestStructure;
using Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure;

namespace Code_Record.Server.Models.SQLServer.SubjectStructure;

public partial class Subject : Base.SubjectStructure.Subject
{
	public Guid Id { get; set; }

	public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();

    public virtual ICollection<Theme> Themes { get; set; } = new List<Theme>();
}
