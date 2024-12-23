namespace Code_Record.Server.Models.SQLServer.DTOs
{
	public class CommentDTO
	{
		public Guid Id { get; set; }

		public List<Guid>? Anwsers { get; set; }

	}
}
