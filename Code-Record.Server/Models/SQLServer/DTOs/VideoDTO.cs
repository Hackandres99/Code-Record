namespace Code_Record.Server.Models.SQLServer.DTOs
{
	public class VideoDTO
	{
		public Guid Id { get; set; }

		public List<ConversationDTO>? Conversations { get; set; }

	}
}
