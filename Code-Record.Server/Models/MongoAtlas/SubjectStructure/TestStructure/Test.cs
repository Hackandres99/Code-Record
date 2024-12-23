using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Code_Record.Server.Models.MongoAtlas.DTOs;

namespace Code_Record.Server.Models.MongoAtlas.SubjectStructure.TestStructure;

public partial class Test : Base.SubjectStructure.TestStructure.Test
{
	[BsonId]
	public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

	public ObjectId? IdSubject { get; set; }

	public List<QuestionDTO>? Questions { get; set; }

	public List<ObjectId>? Results { get; set; }
}
