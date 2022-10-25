using Helper.Interface;
using Newtonsoft.Json;

namespace Helper.Serialization;

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
            var json = JsonConvert.SerializeObject(o, Formatting.Indented, _settings);
            return json;
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Convert [{o.GetType().FullName}] as type [{nameof(T)}] to Json failed.", ex);
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