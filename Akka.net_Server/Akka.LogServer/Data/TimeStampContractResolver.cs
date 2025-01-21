using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Reflection;

namespace Akka.LogServer
{
    //google.protobuf.Timestamp를 Newtonsoft.Json을 사용하여 Deserialize 하기위한 커스텀 소스코드
    //사용 방법
    /*
      var settings = new JsonSerializerSettings
      {
        ContractResolver = new TimeStampContractResolver()
      };
       return JsonConvert.DeserializeObject<Loader>(jsonArray);
     */
    public class TimeStampContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyType == typeof(Google.Protobuf.WellKnownTypes.Timestamp))
            {
                property.Converter = new TimeStampConverter();
            }

            return property;
        }
        public class TimeStampConverter : DateTimeConverterBase
        {
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                DateTime date = DateTime.Parse(reader.Value.ToString());
                date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                return Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(date);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(((Google.Protobuf.WellKnownTypes.Timestamp)value).ToString());
            }
        }
    }
}
