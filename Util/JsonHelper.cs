using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce.NET.Util
{
    public static class JsonHelper
    {
        public static T DeserializeJSon<T>(string jsonString, string dateTimeFormat = "yyyy-MM-ddTHH:mm:ss")
        {
            Type dT = typeof(T);

            if (dT.Name.EndsWith("List"))
                dT = dT.GetTypeInfo().DeclaredProperties.First().PropertyType.GenericTypeArguments[0];

            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings()
            {
                DateTimeFormat = new DateTimeFormat(dateTimeFormat),
                UseSimpleDictionaryFormat = true
            };

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T), settings);
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            T obj = (T)ser.ReadObject(stream);
            stream.Dispose();

            return obj;
        }

        public static string SerializeJSon<T>(T t, string dateTimeFormat = "yyyy-MM-ddTHH:mm:ss")
        {
            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings()
            {
                DateTimeFormat = new DateTimeFormat(dateTimeFormat),
                UseSimpleDictionaryFormat = true
            };

            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer ds = new DataContractJsonSerializer(t.GetType(), settings);
            ds.WriteObject(stream, t);
            byte[] data = stream.ToArray();
            string jsonString = Encoding.UTF8.GetString(data, 0, data.Length);

            stream.Dispose();

            return jsonString;
        }
    }
}
