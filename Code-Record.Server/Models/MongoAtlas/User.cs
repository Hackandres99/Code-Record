using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Code_Record.Server.Models.MongoAtlas;

public partial class User : Base.UserStructure.User
{
	[BsonId]
	public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
}
