using System;
using Newtonsoft.Json;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{

    /// <summary>
    /// Custom JSON converter for ClientNotifications
    /// </summary>
    public class ClientNotificationsJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var notifications = (ClientNotifications)value;
                writer.WriteStartArray();

                foreach (var p in notifications)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("id");
                    writer.WriteValue(p.Id.ToString("N"));
                    writer.WritePropertyName("message");
                    writer.WriteValue(p.Message);
                    writer.WritePropertyName("title");
                    writer.WriteValue(p.Title);
                    writer.WritePropertyName("type");
                    writer.WriteValue(p.Type.ToString().ToLower());
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType.Equals(typeof(ClientNotifications)));
        }
    }
}