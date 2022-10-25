using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using RoboWorkerService.Interfaces;

namespace RoboWorkerService.Csv;

public class MarketTransactionCsv<T> : IMarketTransactionCsv<T>
{
    private readonly string _fileNameCsv;

    public MarketTransactionCsv(IAppRobo appRobo)
    {
        _fileNameCsv = appRobo.Config.ReportPath + typeof(T).Name;
    }

    private void WriteToFile<T>(T records)
    {
        using (var writer = new StreamWriter(_fileNameCsv))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecord(records);
        }
    }

    /// <summary>  Zapise data do csv souboru </summary>
    /// <param name="fileName">pouze jmeno souboru napr test.csv</param>
    /// <param name="records">zaznam, ktery chceme pridat do csv</param>
    /// <typeparam name="T">typ zaznamu csv</typeparam>
    public void WriteToFileCsv<T>(T records)
    {
        if (!File.Exists(_fileNameCsv))
        {
            WriteToFile<T>(records);
            return;
        }

        // Append to the file.
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            // Don't write the header again.
            HasHeaderRecord = false,
        };
        using (var stream = File.Open(_fileNameCsv, FileMode.Append))
        using (var writer = new StreamWriter(stream))
        using (var csv = new CsvWriter(writer, config))
        {
            csv.WriteRecord(records);
        }
    }
}