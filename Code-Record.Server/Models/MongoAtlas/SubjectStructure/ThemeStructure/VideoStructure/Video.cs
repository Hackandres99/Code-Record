using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure.VideoStructure;

public partial class Video : Base.SubjectStructure.ThemeStructure.VideoStructure.Video
{
	[BsonId]
	public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

	public ObjectId? IdTheme { get; set; }

}
