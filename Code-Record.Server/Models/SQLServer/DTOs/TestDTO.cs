namespace Code_Record.Server.Models.SQLServer.DTOs;

public partial class TestDTO
{
	public Guid Id { get; set; }

	public List<QuestionDTO>? Questions { get; set; }

	public List<Guid>? Results { get; set; }
}
