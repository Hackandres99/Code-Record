using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Code_Record.Server.Models.MongoAtlas;

public partial class Visit : Base.Visit
{
	[BsonId]
	public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
}
