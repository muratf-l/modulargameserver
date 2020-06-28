using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LinqToDB;
using Newtonsoft.Json.Linq;
using Reflect.GameServer.Data.Models;
using Reflect.GameServer.Data.Models.Helpers;
using Reflect.GameServer.Data.Models.Services;
using Reflect.GameServer.Database.Postgresql.Context;
using Reflect.GameServer.Database.Postgresql.Models;

namespace Reflect.GameServer.Database.Postgresql.Services
{
    public class UserService : IUserService
    {
        private readonly IPostgresqlDbContext _context;

        public UserService(IPostgresqlDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<HttpStatusCode> AddUserFromFacebook(JToken userData, Ref<UserInfo> token)
        {
            var platformUserId = userData["id"].ToObject<string>();

            await using (var db = new DbDataConnection())
            {
                var isExist = db.User.FirstOrDefault(x => x.PlatformId == platformUserId);

                if (isExist != null)
                {
                    token.Value = await GetUserInfo(isExist.UserToken);
                    return HttpStatusCode.OK;
                }

                var user = new User
                {
                    DateCreated = DateTimeOffset.Now,
                    PlatformId = platformUserId,
                    NameFirst = userData["name"].ToObject<string>(),
                    ProfileImageUrl = userData["photo"].ToObject<string>(),
                    Locale = userData["locale"].ToObject<string>(),
                    PlatformMethod = userData["platform"].ToObject<string>(),
                    RegisterStatus = UserRegisterStatus.Registered,
                    OnlineStatus = UserOnlineStatus.Online,
                    RegisterMethod = UserRegisterMethod.Facebook
                };

                db.BeginTransaction();

                user.Id = db.InsertWithInt32Identity(user);

                if (user.Id <= 0)
                {
                    db.RollbackTransaction();
                    return HttpStatusCode.InternalServerError;
                }

                db.CommitTransaction();

                token.Value = await GetUserInfo(user.UserToken);
            }

            return HttpStatusCode.OK;
        }

        public async Task<HttpStatusCode> AddUserFromEmail(JToken userData, Ref<UserInfo> token)
        {
            var user = new User
            {
                UserToken = Guid.NewGuid().ToString().Replace("-", ""),
                DateCreated = DateTimeOffset.Now,
                NameFirst = userData["name"].ToObject<string>(),
                Email = userData["mail"].ToObject<string>(),
                Password = userData["pass"].ToObject<string>(),
                RegisterStatus = UserRegisterStatus.Registered,
                OnlineStatus = UserOnlineStatus.Online,
                RegisterMethod = UserRegisterMethod.Mail
            };

            await using (var db = new DbDataConnection())
            {
                var isExist = db.User.FirstOrDefault(x => x.Email == user.Email);

                if (isExist != null) return HttpStatusCode.Ambiguous;

                db.BeginTransaction();

                user.Id = db.InsertWithInt32Identity(user);

                if (user.Id <= 0)
                {
                    db.RollbackTransaction();
                    return HttpStatusCode.InternalServerError;
                }

                db.CommitTransaction();
            }

            token.Value = await GetUserInfo(user.UserToken);

            return HttpStatusCode.OK;
        }

        public async Task<HttpStatusCode> LoginFromEmail(JToken userData, Ref<UserInfo> token)
        {
            var email = userData["mail"].ToObject<string>();
            var pass = userData["pass"].ToObject<string>();

            await using var db = new DbDataConnection();

            var isExist = db.User.FirstOrDefault(x => x.Email == email && x.Password == pass);

            if (isExist == null) return HttpStatusCode.NotFound;

            token.Value = await GetUserInfo(isExist.UserToken);

            return HttpStatusCode.OK;
        }

        public async Task<UserInfo> GetUserInfo(string userToken)
        {
            await using var db = new DbDataConnection();

            var isExist = db.User.FirstOrDefault(x => x.UserToken == userToken);

            if (isExist == null) return null;

            var result = new UserInfo
            {
                Id = isExist.UserToken,
                Coin = 2500,
                Name = $"{isExist.NameFirst} {isExist.NameLast}",
                Picture = isExist.ProfileImageUrl
            };

            return result;
        }

        public async Task<HttpStatusCode> ChangeUserStatus(string userToken, UserOnlineStatus status)
        {
            await using var db = new DbDataConnection();

            db.User
                .Where(p => p.UserToken == userToken)
                .Set(p => p.OnlineStatus, status)
                .Update();

            return HttpStatusCode.OK;
        }
    }
}