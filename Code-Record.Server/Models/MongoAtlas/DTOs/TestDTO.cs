using MongoDB.Bson;

namespace Code_Record.Server.Models.MongoAtlas.DTOs;

public partial class TestDTO
{
	public ObjectId Id { get; set; }

	public List<QuestionDTO>? Questions { get; set; }

}
