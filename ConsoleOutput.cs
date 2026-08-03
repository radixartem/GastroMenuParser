using GastroLeinefeldeMenuParser.Models;
using System.Text;

namespace GastroLeinefeldeMenuParser;

public static class ConsoleOutput
{
    public static void PrintMeals(IEnumerable<Meal> meals, DateTime fetchTime)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine($"Speiseangebote Gastro Leinefelde");
        Console.WriteLine($"Abgerufen am: {fetchTime:dd.MM.yyyy HH:mm} Uhr");
        Console.WriteLine();

        if (!meals.Any())
        {
            Console.WriteLine("Keine Gerichte gefunden.");
            return;
        }

        var grouped = meals.GroupBy(m => m.Category);
        foreach (var group in grouped)
        {
            Console.WriteLine($"=== {group.Key} ===");
            Console.WriteLine();

            foreach (var meal in group)
            {
                // Status für "Angebot des Tages" in separater Zeile
                if (group.Key == "Angebot des Tages" && !string.IsNullOrEmpty(meal.Status))
                {
                    var statusDisplay = meal.Status.ToUpperInvariant() switch
                    {
                        "ANGEBOT" => "[ANGEBOT]",
                        "KIDSMENÜ" => "[KIDSMENÜ]",
                        _ => $"[{meal.Status.ToUpperInvariant()}]"
                    };
                    Console.WriteLine(statusDisplay);
                }

                // Name
                Console.WriteLine(meal.Name);

                // Preis
                if (meal.Price.HasValue)
                {
                    Console.WriteLine($"Preis: {meal.Price.Value:F2} €");
                }

                // Status: Ausverkauft für "Unsere Klassiker"
                if (group.Key == "Unsere Klassiker" && meal.Status == "Ausverkauft")
                {
                    Console.WriteLine("Status: Ausverkauft");
                }

                // Zubereitungszeit
                if (!string.IsNullOrEmpty(meal.PreparationTime))
                {
                    Console.WriteLine($"Zubereitungszeit: {meal.PreparationTime}");
                }

                Console.WriteLine();
            }
        }
    }

    public static void PrintApiInfo(string? apiUrl, string? apiType)
    {
        if (!string.IsNullOrEmpty(apiUrl))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n🔍 API gefunden: {apiUrl} (Typ: {apiType ?? "Unbekannt"})");
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    public static void PrintExportInfo(string jsonPath, string csvPath)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"📁 Exportiert nach JSON: {jsonPath}");
        Console.WriteLine($"📁 Exportiert nach CSV: {csvPath}");
        Console.ResetColor();
    }
}