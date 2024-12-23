using MongoDB.Bson;

namespace Code_Record.Server.Models.MongoAtlas.DTOs
{
	public class QuestionDTO
	{
		public ObjectId Id { get; set; }

		public List<ObjectId>? Options { get; set; }
	}
}
