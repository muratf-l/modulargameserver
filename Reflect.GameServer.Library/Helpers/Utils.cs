using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Reflect.GameServer.Library.Helpers
{
    public static class Utils
    {
        public static JsonSerializerSettings JsonSerializerSettings;

        public static string GetGuidId()
        {
            var g = Guid.NewGuid().ToString().Replace("-","");

            return g;
        }

        public static string ToJson(this object graph)
        {
            if (JsonSerializerSettings == null)
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.None
                };

                settings.Converters.Add(new IsoDateTimeConverter());
                settings.Culture = new CultureInfo("tr-TR");

                JsonSerializerSettings = settings;
            }

            try
            {
                return JsonConvert.SerializeObject(graph, Formatting.None, JsonSerializerSettings);
            }
            catch (Exception e)
            {
                //LogService.WriteDebug(e.Message);
            }

            return null;
        }

        public static JToken FromJson(this string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<JToken>(json);
            }
            catch (Exception e)
            {
                //LogService.WriteDebug(e.Message);
            }

            return null;
        }

        public static T ToEnum<T>(this string enumString, T defalultValue)
        {
            try
            {
                return (T) Enum.Parse(typeof(T), enumString);
            }
            catch (Exception e)
            {
                //LogService.WriteDebug(e.Message);
                return defalultValue;
            }
        }
    }
}