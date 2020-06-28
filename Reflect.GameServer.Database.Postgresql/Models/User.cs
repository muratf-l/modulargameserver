using System;
using LinqToDB.Mapping;
using Reflect.GameServer.Data.Models;

namespace Reflect.GameServer.Database.Postgresql.Models
{
    [Table(Name = "ZUsers")]
    public class User
    {
        [PrimaryKey] [Identity] public int Id { get; set; }
        [Column(Name = "UserToken")] public string UserToken { get; set; }

        [Column(Name = "PlatformId")] public string PlatformId { get; set; }
        [Column(Name = "PlatformMethod")] public string PlatformMethod { get; set; }

        [Column(Name = "NameFirst")] public string NameFirst { get; set; }
        [Column(Name = "NameLast")] public string NameLast { get; set; }
        [Column(Name = "Email")] public string Email { get; set; }
        [Column(Name = "ProfileImageUrl")] public string ProfileImageUrl { get; set; }
        [Column(Name = "Locale")] public string Locale { get; set; }

        [Column(Name = "Password")] public string Password { get; set; }

        [Column(Name = "DateCreated")] public DateTimeOffset DateCreated { get; set; }

        [Column(Name = "RegisterStatus")] public UserRegisterStatus RegisterStatus { get; set; }

        [Column(Name = "OnlineStatus")] public UserOnlineStatus OnlineStatus { get; set; }

        [Column(Name = "RegisterMethod")] public UserRegisterMethod RegisterMethod { get; set; }
    }
}