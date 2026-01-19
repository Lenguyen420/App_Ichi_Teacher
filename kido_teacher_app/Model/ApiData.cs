using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace kido_teacher_app.Model
{
    public class ApiData<T>
    {
        // ApiData<T> dùng chung cho:
        // - Response: data.items
        // - Request: { users: [...] }
        public List<T> items { get; set; } = new();
    }



public class ApiDataConverter<T> : JsonConverter<ApiData<T>>
    {
        public override ApiData<T> ReadJson(
            JsonReader reader,
            Type objectType,
            ApiData<T> existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            // Case 1: data = [ ... ]
            if (token.Type == JTokenType.Array)
            {
                return new ApiData<T>
                {
                    items = token.ToObject<List<T>>() ?? new List<T>()
                };
            }

            // Case 2: data = { items: [ ... ] }
            if (token.Type == JTokenType.Object)
            {
                var itemsToken = token["items"];
                return new ApiData<T>
                {
                    items = itemsToken?.ToObject<List<T>>() ?? new List<T>()
                };
            }

            return new ApiData<T>();
        }

        public override void WriteJson(
            JsonWriter writer,
            ApiData<T> value,
            JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.items);
        }
    }


}
