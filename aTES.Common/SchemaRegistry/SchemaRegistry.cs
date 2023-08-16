using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace aTES.Common.SchemaRegistry;

public static class SchemaRegistry
{
    private static JSchema Load(string eventName)
    {
        string path = Path.Combine(Path.GetDirectoryName(typeof(SchemaRegistry).Assembly.Location), @"SchemaRegistry",
            "Schemas");

        foreach (var file in Directory.GetFiles(path))
        {
            var fileName = Path.GetFileName(file).Replace(".json", "");
            if (fileName == eventName)
            {
                var stringSchema = File.ReadAllText(file);
                return JSchema.Parse(stringSchema);
            }
        }

        throw new Exception($"Schema for event {eventName} not found in SchemaRegistry");
    }

    public static void Validate(string eventName, string json)
    {
        var schema = Load(eventName);
        var jObj = JObject.Parse(json);
        if (!jObj.IsValid(schema, out IList<string> errors))
        {
            throw new Exception($"Schema validation failed for {eventName}. Details:\n{string.Join(", ", errors)}");
        }
    }
}