using Confluent.Kafka;

namespace aTES.Common;

public class BaseMessage<TPayload> : Message<string, string>
{
    public static BaseMessage<TPayload> Create(string messageId, string eventName, TPayload payload)
    {
        return new BaseMessage<TPayload>()
        {
            Key = messageId,
            Value = BasePayload<TPayload>.Create(eventName, payload).ToJson()
        };
    }
}