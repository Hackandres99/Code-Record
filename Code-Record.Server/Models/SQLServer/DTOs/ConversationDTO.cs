namespace Code_Record.Server.Models.SQLServer.DTOs
{
	public class ConversationDTO
	{
		public Guid Id { get; set; }

		public List<CommentDTO>? Comments { get; set; }

	}
}
