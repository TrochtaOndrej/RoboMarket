using Helper;
using Helper.Interface;
using Helper.Serialization;
using RoboWorkerService.Config;

namespace RoboWorkerService.Json;

public class JsonFolderConfigAppRobo<T> : IJsonFolderConfigAppRobo<T> where T : new()
{
    private readonly IJsonConvertor _jsonConvertor;
    private readonly IConfig _config;
    private string _fileName => _config.ConfigPath + typeof(T)?.FullName!.MakeValidFileName();

    public JsonFolderConfigAppRobo(IJsonConvertor jsonConvertor, IConfig config)
    {
        _jsonConvertor = jsonConvertor;
        _config = config;
        LoadDataAsync().Wait();
    }


    public async Task<T> LoadDataAsync(CancellationToken cancellationToken = default)
    {
        if (File.Exists(_fileName))
        {
            var obj = await _jsonConvertor.FileToInstanceAsync<T>(_fileName, cancellationToken);
            Data = obj;
            return Data;
        }

        await SaveData(new T(), cancellationToken);
        throw new FileNotFoundException(" Create new Config File with default data: " + _fileName);
    }

    public Task SaveDataAsync(CancellationToken cancellationToken = default)
    {
        if (Data is null) return Task.CompletedTask;
        return SaveData(Data, cancellationToken);
    }

    public T Data { get; private set; }

    private Task SaveData<T>(T o, CancellationToken cancellationToken = default)
    {
        return _jsonConvertor.ToFileJsonAsync(_fileName, o, cancellationToken);
    }
}