namespace RoboWorkerService.Csv;

public interface IMarketTransactionCsv<T>
{
    /// <summary>  Zapise data do csv souboru </summary>
    /// <param name="fileName">pouze jmeno souboru napr test.csv</param>
    /// <param name="records">zaznam, ktery chceme pridat do csv</param>
    /// <typeparam name="T">typ zaznamu csv</typeparam>
    void WriteToFileCsv<T>(T records);

    void DeleteCsvFile();
    string FileNameCsv { get; }
}