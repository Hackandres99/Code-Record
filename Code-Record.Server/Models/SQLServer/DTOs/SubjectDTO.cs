namespace Code_Record.Server.Models.SQLServer.DTOs;

public partial class SubjectDTO : Base.SubjectStructure.Subject
{
	public Guid Id { get; set; }

	public List<Guid>? Resources { get; set; }

	public List<TestDTO>? Tests { get; set; }

	public List<ThemeDTO>? Themes { get; set; }
}
