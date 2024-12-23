using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Code_Record.Server.Models.MongoAtlas.DTOs;

namespace Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure;

public partial class Theme : Base.SubjectStructure.ThemeStructure.Theme
{
	[BsonId]
	public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

	public ObjectId? IdSubject { get; set; }

	public List<ObjectId>? Videos { get; set; }
}
