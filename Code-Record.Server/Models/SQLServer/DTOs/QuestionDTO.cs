namespace Code_Record.Server.Models.SQLServer.DTOs
{
	public class QuestionDTO
	{
		public Guid Id { get; set; }

		public List<Guid>? Options { get; set; }
	}
}
