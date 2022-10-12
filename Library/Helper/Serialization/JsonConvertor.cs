using Newtonsoft.Json;

namespace Helper.Serialization;

public class SerialiazationException : Exception
{
    public SerialiazationException() { }
    public SerialiazationException(string msg) : base(msg) { }
    public SerialiazationException(string msg, Exception ex) : base(msg, ex) { }
}

public interface IJsonConvertor
{
    string ToJson<T>(T o);
    T ToInstance<T>(string json);
}

public class JsonConvertor : IJsonConvertor
{
    private readonly JsonSerializerSettings _settings;

    public JsonConvertor(JsonSerializerSettings settings)
    {
        _settings = settings;
    }

    public string ToJson<T>(T o)
    {
        try
        {
            var json = JsonConvert.SerializeObject(o, Newtonsoft.Json.Formatting.Indented, _settings);
            return json;
        }
        catch (Exception ex)
        {
            throw new SerialiazationException($"Convert [{o.GetType().FullName}] as type [{nameof(T)}] to Json failed.", ex);
        }
    }

    public T ToInstance<T>(string json)
    {
        var instance = JsonConvert.DeserializeObject<T>(json, _settings);
        return instance;
    }


    public class AbstractConverter<TReal, TAbstract>
        : JsonConverter where TReal : TAbstract
    {
        public override Boolean CanConvert(Type objectType)
            => objectType == typeof(TAbstract);

        public override Object ReadJson(JsonReader reader, Type type, Object value, JsonSerializer jser)
            => jser.Deserialize<TReal>(reader);

        public override void WriteJson(JsonWriter writer, Object value, JsonSerializer jser)
            => jser.Serialize(writer, value);
    }
}

