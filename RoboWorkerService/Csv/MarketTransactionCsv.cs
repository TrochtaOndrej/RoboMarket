using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using RoboWorkerService.Interfaces;

namespace RoboWorkerService.Csv;

public class MarketTransactionCsv<T> : IMarketTransactionCsv<T>
{
    private readonly string _fileNameCsv;

    private static CsvConfiguration _csvConfiguration
    {
        get
        {
            var con = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture);
            con.HasHeaderRecord = true;
            
            return con;
        }
    }

    public MarketTransactionCsv(IAppRobo appRobo)
    {
        ;
        _fileNameCsv = appRobo.Config.ReportPath + typeof(T).Name;
    }

    private void WriteToFile<T>(T records)
    {
        using (var writer = new StreamWriter(_fileNameCsv))
        using (var csv = new CsvWriter(writer, _csvConfiguration))
        {
            csv.WriteHeader<T>();
            csv.WriteRecord(records);
            csv.NextRecord();
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
            csv.WriteHeader<T>();
            csv.WriteRecord(records);
            csv.NextRecord();
        }
    }
}