using System;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Reflect.GameServer.Library.Logging;

namespace Reflect.GameServer.Library.Messages
{
    public static class MessageUtil
    {
        public static string Serialize(IMessage message)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None
            };

            settings.Converters.Add(new IsoDateTimeConverter());
            settings.Culture = new CultureInfo("tr-TR");

            try
            {
                var js = JsonConvert.SerializeObject(message, Formatting.None, settings);

                LogService.WriteDebug(js);

                return js;
            }
            catch (Exception e)
            {
                LogService.WriteDebug(e.Message);
            }

            return null;
        }

        public static T DeSerialize<T>(string json) where T : new()
        {
            try
            {
                LogService.WriteDebug(json);

                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                LogService.WriteDebug(e.Message);
            }

            return new T();
        }

        public static T DeSerialize<T>(byte[] buffer, long offset, long size) where T : new()
        {
            try
            {
                var json = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);

                return DeSerialize<T>(json);
            }
            catch (Exception e)
            {
                LogService.WriteDebug(e.Message);
            }

            return new T();
        }

        public static bool TryCast<T>(this object obj, out T result)
        {
            if (obj is T obj1)
            {
                result = obj1;
                return true;
            }

            result = default(T);

            return false;
        }

        public static bool TryCastJTokenObject<T>(this object obj, out T result)
        {
            if (obj is JToken obJToken)
            {
                result = obJToken.ToObject<T>() ;
                return true;
            }

            result = default(T);

            return false;
        }
    }
}