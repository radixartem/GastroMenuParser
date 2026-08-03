using System.Globalization;
using System.Text.RegularExpressions;
using GastroLeinefeldeMenuParser.Models;
using HtmlAgilityPack;

namespace GastroLeinefeldeMenuParser.Services;

public class MenuParser : IMenuParser
{
    public Task<IEnumerable<Meal>> ParseMenuAsync(string htmlContent)
    {
        var meals = new List<Meal>();
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // Alle Textknoten finden
        var textNodes = doc.DocumentNode.SelectNodes("//text()");
        if (textNodes == null)
            return Task.FromResult<IEnumerable<Meal>>(meals);

        // Text sammeln und leere Zeilen filtern
        var lines = textNodes
            .Select(n => n.InnerText.Trim())
            .Where(t => !string.IsNullOrEmpty(t) && !t.StartsWith("++"))
            .ToList();

        string currentCategory = "";
        var currentMeal = new List<string>();

        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            // Kategorien erkennen
            if (line.Contains("Angebot des Tages"))
            {
                if (currentMeal.Any())
                {
                    meals.Add(ParseMealFromLines(currentMeal, currentCategory));
                    currentMeal.Clear();
                }
                currentCategory = "Angebot des Tages";
                continue;
            }
            else if (line.Contains("Unsere Klassiker"))
            {
                if (currentMeal.Any())
                {
                    meals.Add(ParseMealFromLines(currentMeal, currentCategory));
                    currentMeal.Clear();
                }
                currentCategory = "Unsere Klassiker";
                continue;
            }
            else if (line.Contains("Die nächsten Tage") || line.Contains("Angebote vom"))
            {
                break;
            }

            // In einer Kategorie
            if (!string.IsNullOrEmpty(currentCategory))
            {
                // Preis
                if (IsPrice(line))
                {
                    currentMeal.Add(line);
                    if (currentMeal.Count >= 2)
                    {
                        meals.Add(ParseMealFromLines(currentMeal, currentCategory));
                        currentMeal.Clear();
                    }
                    continue;
                }

                // Zeit
                if (IsPreparationTime(line))
                {
                    currentMeal.Add(line);
                    continue;
                }

                // Name
                if (!IsDateRange(line) && !line.Contains("Angebote vom"))
                {
                    if (currentMeal.Any() && IsPrice(currentMeal.Last()))
                    {
                        meals.Add(ParseMealFromLines(currentMeal, currentCategory));
                        currentMeal.Clear();
                    }
                    currentMeal.Add(line);
                }
            }
        }

        // Letztes Gericht
        if (currentMeal.Any())
        {
            meals.Add(ParseMealFromLines(currentMeal, currentCategory));
        }

        return Task.FromResult<IEnumerable<Meal>>(meals);
    }

    private bool IsPrice(string text)
    {
        return Regex.IsMatch(text, @"^(\d+[.,]\d{2})\s*[€]?$");
    }

    private bool IsPreparationTime(string text)
    {
        return Regex.IsMatch(text, @"^(\d+)\s*(Minuten?|min)$");
    }

    private bool IsDateRange(string text)
    {
        return Regex.IsMatch(text, @"\d{2}\.\d{2}\.\d{4}");
    }

    private Meal ParseMealFromLines(List<string> lines, string category)
    {
        var meal = new Meal
        {
            Category = category,
            Name = "",
            Status = null,
            Price = null,
            PreparationTime = null
        };

        var fullText = string.Join(" ", lines);
        fullText = Regex.Replace(fullText, @"\s+", " ").Trim();

        // Status in Sternchen
        var statusMatch = Regex.Match(fullText, @"\*(?<status>[A-ZÄÖÜ]+)\*");
        if (statusMatch.Success)
        {
            meal.Status = statusMatch.Groups["status"].Value switch
            {
                "ANGEBOT" => "Angebot",
                "KIDSMENÜ" => "Kidsmenü",
                "AUS" => "Ausverkauft",
                _ => statusMatch.Groups["status"].Value
            };
            fullText = fullText.Replace(statusMatch.Value, "").Trim();
        }

        // Preis
        var priceMatch = Regex.Match(fullText, @"(\d+[.,]\d{2})\s*[€]?");
        if (priceMatch.Success)
        {
            var priceStr = priceMatch.Groups[1].Value.Replace('.', ',');
            if (decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.GetCultureInfo("de-DE"), out var price))
                meal.Price = price;
            fullText = fullText.Replace(priceMatch.Value, "").Trim();
        }

        // Zeit
        var timeMatch = Regex.Match(fullText, @"(\d+)\s*(Minuten?|min)");
        if (timeMatch.Success)
        {
            meal.PreparationTime = timeMatch.Value.Trim();
            fullText = fullText.Replace(timeMatch.Value, "").Trim();
        }

        // Aufräumen
        fullText = Regex.Replace(fullText, @"[\[\]\(\)\*]", " ");
        fullText = Regex.Replace(fullText, @"\s+", " ").Trim();

        // Name speichern
        if (!string.IsNullOrEmpty(fullText) &&
            !Regex.IsMatch(fullText, @"^\d+[.,]\d+$") &&
            !fullText.Contains("Angebote vom"))
        {
            meal.Name = fullText;
        }

        return meal;
    }
}