using GastroLeinefeldeMenuParser.Services;
using GastroLeinefeldeMenuParser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient<WebsiteClient>();
        services.AddTransient<IMenuParser, MenuParser>();
        services.AddTransient<ExportService>();
        services.AddTransient<ApiDetector>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var config = host.Services.GetRequiredService<IConfiguration>();

try
{
    var url = config["AppSettings:Url"] ?? "https://essen-auf-raedern-eichsfeld.de/tagesangebot";
    var exportPath = config["AppSettings:ExportPath"] ?? "./exports";
    var enableExport = bool.TryParse(config["AppSettings:EnableExport"], out var export) ? export : true;
    var fetchTime = DateTime.Now;

    logger.LogInformation("🚀 Starte Gastro Leinefelde Menu Parser");
    logger.LogInformation("📡 URL: {Url}", url);

    using var scope = host.Services.CreateScope();
    var client = scope.ServiceProvider.GetRequiredService<WebsiteClient>();
    var parser = scope.ServiceProvider.GetRequiredService<IMenuParser>();
    var exportService = scope.ServiceProvider.GetRequiredService<ExportService>();
    var apiDetector = scope.ServiceProvider.GetRequiredService<ApiDetector>();

    logger.LogInformation("⬇️ Lade HTML von {Url}...", url);
    var html = await client.FetchHtmlAsync(url);
    logger.LogInformation("✅ HTML geladen, Größe: {Size} bytes", html.Length);

    logger.LogInformation("🔍 Parse Menu...");
    var meals = await parser.ParseMenuAsync(html);
    var mealList = meals.ToList();
    logger.LogInformation("✅ {Count} Gerichte gefunden", mealList.Count);

    if (!mealList.Any())
    {
        Console.WriteLine("⚠️ Keine Gerichte gefunden.");
        return 0;
    }

    // API Detection
    logger.LogInformation("🔎 Suche nach API...");
    var apiInfo = await apiDetector.DetectApiAsync(url);
    if (apiInfo != null)
    {
        logger.LogInformation("✅ API gefunden: {Url} (Typ: {Type})", apiInfo.Url, apiInfo.Type);
        ConsoleOutput.PrintApiInfo(apiInfo.Url, apiInfo.Type);
    }

    // Console Output
    ConsoleOutput.PrintMeals(mealList, fetchTime);

    // Export
    if (enableExport)
    {
        logger.LogInformation("💾 Exportiere Daten...");
        Directory.CreateDirectory(exportPath);

        var jsonPath = Path.Combine(exportPath, $"menu_export_{fetchTime:yyyyMMdd_HHmmss}.json");
        var csvPath = Path.Combine(exportPath, $"menu_export_{fetchTime:yyyyMMdd_HHmmss}.csv");

        await exportService.ExportToJsonAsync(mealList, jsonPath);
        await exportService.ExportToCsvAsync(mealList, csvPath);

        ConsoleOutput.PrintExportInfo(jsonPath, csvPath);
        logger.LogInformation("✅ Export abgeschlossen");
    }

    logger.LogInformation("✅ Programm erfolgreich beendet");
    return 0;
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ Fehler beim Ausführen des Programms");
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n❌ Fehler: {ex.Message}");
    Console.ResetColor();
    return 1;
}
finally
{
    Console.WriteLine("\nDrücken Sie eine beliebige Taste zum Beenden...");
    Console.ReadKey();
}