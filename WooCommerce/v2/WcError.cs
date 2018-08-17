using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WooCommerce.NET.Util;

namespace WooCommerce.NET.WooCommerce.v2
{
    [DataContract]
    [JsonConverter(typeof(WcErrorConverter))]
    public class WcError
    {
        [DataMember(EmitDefaultValue = false)]
        public string code { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string message { get; set; }


        [JsonIgnore]
        public string data { get; set; }

        [JsonIgnore]
        public ErrorData Data { get; set; }
    }

    [DataContract]
    public class ErrorData
    {
        [DataMember(EmitDefaultValue = false)]
        public int? status { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? resource_id { get; set; }
    }

    public class WcErrorConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject
            WcError target = new WcError();

            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), target);

            var jdataToken = jObject.SelectToken("data");

            if (!jdataToken.Children().Any())
            {
                target.data = jdataToken.Value<string>();
            }
            else {
                target.Data = new ErrorData()
                {
                    status = jdataToken.SelectToken("status")?.Value<int?>(),
                    resource_id = jdataToken.SelectToken("resource_id")?.Value<int?>()
                };
            }

            return target;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(WcError).IsAssignableFrom(objectType);
        }
    }
}
