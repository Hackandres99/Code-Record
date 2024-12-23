namespace Code_Record.Server.Models.SQLServer.DTOs
{
	public class ThemeDTO
	{
		public Guid Id { get; set; }

		public List<VideoDTO>? Videos { get; set; }
	}
}
