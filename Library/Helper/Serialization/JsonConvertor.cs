using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
    Task ToFileJsonAsync<T>(string filename, T o, CancellationToken cancellationToken = default);
    Task<T> FileToInstanceAsync<T>(string filename, CancellationToken cancellationToken = default);
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

    public async Task ToFileJsonAsync<T>(string filename, T o, CancellationToken cancellationToken = default)
    {
        await File.WriteAllTextAsync(filename, ToJson(o), cancellationToken);
    }

    public T ToInstance<T>(string json)
    {
        var instance = JsonConvert.DeserializeObject<T>(json, _settings);
        return instance;
    }

    public async Task<T> FileToInstanceAsync<T>(string filename, CancellationToken cancellationToken = default)
    {
        return ToInstance<T>(await File.ReadAllTextAsync(filename, cancellationToken));
    }

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

