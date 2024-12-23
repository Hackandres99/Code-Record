using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Code_Record.Server.Models.MongoAtlas.SubjectStructure.TestStructure;

public partial class Result : Base.SubjectStructure.TestStructure.Result
{
	[BsonId]
	public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

	public ObjectId? IdTest { get; set; }
}
