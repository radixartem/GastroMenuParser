using GastroLeinefeldeMenuParser.Models;

namespace GastroLeinefeldeMenuParser.Services;

public interface IMenuParser
{
    Task<IEnumerable<Meal>> ParseMenuAsync(string htmlContent);
}