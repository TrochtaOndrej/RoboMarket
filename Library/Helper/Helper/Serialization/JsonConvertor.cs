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
    string ToJson<T>(object o);
    T ToInstance<T>(string json);
}

public class JsonConvertor : IJsonConvertor
{
    public string ToJson<T>(object o)
    {
        try
        {
            var json = JsonConvert.SerializeObject((T)o,Newtonsoft.Json.Formatting.Indented);
            return json;
        }
        catch (Exception ex)
        {
            throw new SerialiazationException($"Convert [{o.GetType().FullName}] as type [{nameof(T)}] to Json failed.", ex);
        }
    }

    public T ToInstance<T>(string json)
    {
        return JsonToClass<T>(json);
    }

    public static T JsonToClass<T>(string json)
    {
        var instance = JsonConvert.DeserializeObject<T>(json);
        return instance;
    }
}

