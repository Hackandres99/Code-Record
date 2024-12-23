using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Code_Record.Server.Models.MongoAtlas;

public partial class Allow: Base.Allow
{
	[BsonId]
	public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

	public ObjectId? UserId { get; set; }

	public ObjectId? ResourceId { get; set; }
}
