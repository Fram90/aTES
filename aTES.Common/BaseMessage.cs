using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace aTES.Common;

public class BaseMessage<TPayload>
{
    private static readonly JsonSerializerSettings SerializerSettings;

    static BaseMessage()
    {
        SerializerSettings = new JsonSerializerSettings();
        SerializerSettings.Converters.Add(new StringEnumConverter(new DefaultNamingStrategy(), false));
    }

    public Guid Id { get; set; }
    public DateTimeOffset SentTime { get; set; }
    public string ModelType { get; set; }
    public TPayload Payload { get; set; }

    public static BaseMessage<TPayload> Create(TPayload payload)
    {
        return new BaseMessage<TPayload>()
        {
            Id = Guid.NewGuid(),
            SentTime = DateTimeOffset.UtcNow,
            ModelType = typeof(TPayload).Name,
            Payload = payload
        };
    }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this, SerializerSettings);
    }
}