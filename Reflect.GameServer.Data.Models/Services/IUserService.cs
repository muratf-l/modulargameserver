using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Reflect.GameServer.Data.Models.Helpers;

namespace Reflect.GameServer.Data.Models.Services
{
    public interface IUserService
    {
        Task<HttpStatusCode> AddUserFromFacebook(JToken userData, Ref<UserInfo> token);

        Task<HttpStatusCode> AddUserFromEmail(JToken userData, Ref<UserInfo> token);

        Task<HttpStatusCode> LoginFromEmail(JToken userData, Ref<UserInfo> token);

        Task<UserInfo> GetUserInfo(string userToken);

        Task<HttpStatusCode> ChangeUserStatus(string userToken, UserOnlineStatus status);
    }
}