using System.Globalization;
using System.Text.Json;
using GastroLeinefeldeMenuParser.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace GastroLeinefeldeMenuParser.Services;

public class ExportService
{
    public async Task ExportToJsonAsync(IEnumerable<Meal> meals, string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(meals, options);
        await File.WriteAllTextAsync(filePath, json);
        Console.WriteLine($"✅ Exportiert nach JSON: {filePath}");
    }

    public async Task ExportToCsvAsync(IEnumerable<Meal> meals, string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ";"
        };

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, config);
        
        csv.WriteHeader<Meal>();
        csv.NextRecord();
        csv.WriteRecords(meals);
        
        await writer.FlushAsync();
        Console.WriteLine($"✅ Exportiert nach CSV: {filePath}");
    }
}