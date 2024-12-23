using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Code_Record.Server.Models.MongoAtlas.SubjectStructure.TestStructure;

public partial class Question : Base.SubjectStructure.TestStructure.Question
{
	[BsonId]
	public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

	public ObjectId? IdTest { get; set; }

	public List<ObjectId>? Options { get; set; }
}
