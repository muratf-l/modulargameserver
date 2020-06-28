using System;
using System.Net;
using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Reflect.GameServer.Data.Models;
using Reflect.GameServer.Data.Models.Helpers;
using Reflect.GameServer.Data.Models.Services;
using Reflect.GameServer.Database.Mongo.Models;

namespace Reflect.GameServer.Database.Mongo.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoDbContext _context;

        public UserService(IMongoDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<HttpStatusCode> AddUserFromFacebook(JToken userData, Ref<UserInfo> token)
        {
            var platformId = userData["id"].ToObject<string>();

            var builder = Builders<User>.Filter;

            var filter = builder.Eq(widget => widget.PlatformId, platformId);

            try
            {
                var isExist = await _context.GetCollection<User>().Find(filter).FirstOrDefaultAsync();

                if (isExist != null)
                {
                    token.Value = await GetUserInfo(isExist.Id);
                    return HttpStatusCode.OK;
                }

                var user = new User
                {
                    DateCreated = DateTimeOffset.Now,
                    PlatformId = platformId,
                    NameFirst = userData["name"].ToObject<string>(),
                    ProfileImageUrl = userData["photo"].ToObject<string>(),
                    Locale = userData["locale"].ToObject<string>(),
                    PlatformMethod = userData["platform"].ToObject<string>(),
                    RegisterStatus = UserRegisterStatus.Registered,
                    OnlineStatus = UserOnlineStatus.Online,
                    RegisterMethod = UserRegisterMethod.Facebook
                };

                await _context.GetCollection<User>().InsertOneAsync(user);

                token.Value = await GetUserInfo(user.Id);

                return HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                //LogService.WriteDebug(e.Message);
                return HttpStatusCode.InternalServerError;
            }
        }

        public async Task<HttpStatusCode> AddUserFromEmail(JToken userData, Ref<UserInfo> token)
        {
            var user = new User
            {
                DateCreated = DateTimeOffset.Now,
                NameFirst = userData["name"].ToObject<string>(),
                Email = userData["mail"].ToObject<string>(),
                Password = userData["pass"].ToObject<string>(),
                RegisterStatus = UserRegisterStatus.Registered,
                OnlineStatus = UserOnlineStatus.Online,
                RegisterMethod = UserRegisterMethod.Mail
            };

            var builder = Builders<User>.Filter;

            var filter = builder.Eq(widget => widget.Email, user.Email);

            try
            {
                var isExist = await _context.GetCollection<User>().CountDocumentsAsync(filter);

                if (isExist > 0) return HttpStatusCode.Ambiguous;

                await _context.GetCollection<User>().InsertOneAsync(user);

                token.Value = await GetUserInfo(user.Id);

                return HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                //LogService.WriteDebug(e.Message);
                return HttpStatusCode.InternalServerError;
            }
        }

        public async Task<HttpStatusCode> LoginFromEmail(JToken userData, Ref<UserInfo> token)
        {
            var email = userData["mail"].ToObject<string>();
            var pass = userData["pass"].ToObject<string>();

            var builder = Builders<User>.Filter;

            var filter = builder.Eq(widget => widget.Email, email) & builder.Eq(widget => widget.Password, pass);

            try
            {
                var isExist = await _context.GetCollection<User>().Find(filter).FirstOrDefaultAsync();

                if (isExist == null) return HttpStatusCode.NotFound;

                token.Value = await GetUserInfo(isExist.Id);

                return HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                //LogService.WriteDebug(e.Message);
                return HttpStatusCode.InternalServerError;
            }
        }

        public async Task<UserInfo> GetUserInfo(string userToken)
        {
            var builder = Builders<User>.Filter;

            var filter = builder.Eq(widget => widget.Id, userToken);

            try
            {
                var isExist = await _context.GetCollection<User>().Find(filter).FirstOrDefaultAsync();

                if (isExist == null) return null;

                var result = new UserInfo
                {
                    Id = isExist.Id,
                    Coin = 2500,
                    Name = $"{isExist.NameFirst} {isExist.NameLast}",
                    Picture = isExist.ProfileImageUrl
                };

                return result;
            }
            catch (Exception e)
            {
                //LogService.WriteDebug(e.Message);
                return null;
            }
        }

        public async Task<HttpStatusCode> ChangeUserStatus(string userToken, UserOnlineStatus status)
        {
            var filter = Builders<User>.Filter.Eq(widget => widget.Id, userToken);

            var update = Builders<User>.Update.Set(widget => widget.OnlineStatus, status);

            await _context.GetCollection<User>().UpdateOneAsync(filter, update);

            return HttpStatusCode.OK;
        }
    }
}