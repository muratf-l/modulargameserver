using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Reflect.GameServer.Data.Models;
using Reflect.GameServer.Database.Mongo.Helpers;

namespace Reflect.GameServer.Database.Mongo.Models
{
    [BsonCollection("ZUsers")]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("PlatformId")] public string PlatformId { get; set; }
        [BsonElement("PlatformMethod")] public string PlatformMethod { get; set; }

        [BsonElement("NameFirst")] public string NameFirst { get; set; }
        [BsonElement("NameLast")] public string NameLast { get; set; }
        [BsonElement("Email")] public string Email { get; set; }
        [BsonElement("ProfileImageUrl")] public string ProfileImageUrl { get; set; }
        [BsonElement("Locale")] public string Locale { get; set; }

        [BsonElement("Password")] public string Password { get; set; }

        [BsonElement("DateCreated")]
        [BsonRepresentation(BsonType.String)]
        public DateTimeOffset DateCreated { get; set; }

        [BsonElement("RegisterStatus")] public UserRegisterStatus RegisterStatus { get; set; }
        [BsonElement("OnlineStatus")] public UserOnlineStatus OnlineStatus { get; set; }

        [BsonElement("RegisterMethod")] public UserRegisterMethod RegisterMethod { get; set; }
    }
}