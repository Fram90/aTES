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
    public string EventName { get; set; }
    public TPayload Payload { get; set; }

    public static BaseMessage<TPayload> Create(string eventName, TPayload payload)
    {
        return new BaseMessage<TPayload>()
        {
            Id = Guid.NewGuid(),
            SentTime = DateTimeOffset.UtcNow,
            EventName = eventName,
            ModelType = typeof(TPayload).Name,
            Payload = payload
        };
    }

    // немножко говнокода. В реальной системе SchemaRegistry была бы каким-нибудь отдельным сервисом, не в смысле микро,
    // а в смысле класс, который бы подгружал при старте известные схемы, тречил бы изменения, подтягивал их и держал в памяти
    // актуальные версии. Но т.к. учимся на кошках, тут это просто статический класс
    public string ToJson(bool validateSchema = true)
    {
        if (validateSchema)
        {
            var payload = JsonConvert.SerializeObject(this.Payload, SerializerSettings);
            SchemaRegistry.SchemaRegistry.Validate(EventName, payload);
        }

        var serialized = JsonConvert.SerializeObject(this, SerializerSettings);
        return serialized;
    }
}