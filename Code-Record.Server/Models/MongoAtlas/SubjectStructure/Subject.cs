using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Code_Record.Server.Models.MongoAtlas.DTOs;

namespace Code_Record.Server.Models.MongoAtlas.SubjectStructure;

public partial class Subject : Base.SubjectStructure.Subject
{
	[BsonId]
	public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

	public List<ObjectId>? Resources { get; set; }

	public List<TestDTO>? Tests { get; set; }

	public List<ThemeDTO>? Themes { get; set; }
}
