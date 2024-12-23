using Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure.VideoStructure;

namespace Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure;

public partial class Theme : Base.SubjectStructure.ThemeStructure.Theme
{
	public Guid Id { get; set; }

	public Guid? IdSubject { get; set; }

	public virtual Subject? IdSubjectNavigation { get; set; }

    public virtual ICollection<Video> Videos { get; set; } = new List<Video>();
}
