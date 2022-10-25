namespace Helper.Interface;


public interface IJsonConvertor
{
    string ToJson<T>(T o);
    T ToInstance<T>(string json);
    Task ToFileJsonAsync<T>(string filename, T o, CancellationToken cancellationToken = default);
    Task<T> FileToInstanceAsync<T>(string filename, CancellationToken cancellationToken = default);
}