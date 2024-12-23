using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;

public partial class Conversation : Base.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure.Conversation
{
	[BsonId]
	public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

	public ObjectId? IdVideo { get; set; }

}
