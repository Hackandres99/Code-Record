using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Code_Record.Server.Models.MongoAtlas.SubjectStructure.TestStructure;

public partial class Option : Base.SubjectStructure.TestStructure.Option
{
	[BsonId]
	public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

	public ObjectId? IdQuestion { get; set; }
}
