using MongoDB.Bson;

namespace Code_Record.Server.Models.MongoAtlas.DTOs
{
	public class ThemeDTO
	{
		public ObjectId Id { get; set; }

		public List<ObjectId>? Videos { get; set; }
	}
}
