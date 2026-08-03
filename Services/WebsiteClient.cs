using System.Net;

namespace GastroLeinefeldeMenuParser.Services;

public class WebsiteClient : IDisposable
{
    private readonly HttpClient _httpClient;

    public WebsiteClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", 
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task<string> FetchHtmlAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var html = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(html))
                throw new InvalidOperationException("Leerer HTML-Inhalt empfangen.");
                
            return html;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new Exception($"Seite nicht gefunden (404): {url}", ex);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new Exception($"Server-Fehler (500) beim Zugriff auf {url}", ex);
        }
        catch (TaskCanceledException) when (_httpClient.Timeout != Timeout.InfiniteTimeSpan)
        {
            throw new Exception($"Zeitüberschreitung beim Laden von {url}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Fehler beim Laden der Seite: {ex.Message}", ex);
        }
    }

    public void Dispose() => _httpClient.Dispose();
}