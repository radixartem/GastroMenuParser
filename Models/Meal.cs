namespace GastroLeinefeldeMenuParser.Models;

public class Meal
{
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string? Status { get; set; }
    public string? PreparationTime { get; set; }
    public DateTime? Date { get; set; }
    public string? DayOfWeek { get; set; }
    
    public override string ToString()
    {
        return $"{Name} - {Price?.ToString("F2") ?? "Preis nicht angegeben"} €";
    }
}