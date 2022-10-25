namespace Helper.Serialization;

public interface IJsonFolderConfigAppRobo<T>
{
    T Data { get; }
    Task SaveDataAsync(CancellationToken cancellationToken = default);
    Task<T> LoadDataAsync(CancellationToken cancellationToken = default);
}
