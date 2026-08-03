# Gastro Menu Parser

Konsolenanwendung zum Parsen des täglichen Menüs von `essen-auf-raedern-eichsfeld.de`.

## Voraussetzungen
- .NET 9 SDK
- Internetzugang

## Starten
```bash
dotnet restore
dotnet build
dotnet run

## Verwendete Bibliotheken

Verwendete Bibliotheken
HtmlAgilityPack – HTML-Parsing

Microsoft.Extensions.Hosting – Dependency Injection

Microsoft.Extensions.Logging – Logging

CsvHelper – CSV-Export

Beschreibung der Lösung
Die Anwendung ist in unabhängige Dienste aufgeteilt:

WebsiteClient: Lädt HTML

MenuParser: Extrahiert Gerichte

ExportService: Exportiert als JSON/CSV

ConsoleOutput: Formatiert die Ausgabe

Annahmen
Die HTML-Struktur bleibt stabil (Kategorien in <h2>)

Preise sind im Format "X,XX €"

Statusangaben in Sternchen

Bekannte Einschränkungen
"Die nächsten Tage" werden nicht geparst

Bei Änderungen der HTML-Struktur muss MenuParser angepasst werden

Anpassungen bei Website-Änderungen
XPath-Selektoren in MenuParser aktualisieren

Reguläre Ausdrücke für Preise/Status anpassen

text

---

## ** Dockerfile (optional)**

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "GastroLeinefeldeMenuParser.dll"]
Запуск приложения
bash
# Сборка
dotnet build

# Запуск
dotnet run

# Docker
docker build -t gastro-menu-parser .
docker run --rm -v ${PWD}/exports:/app/exports gastro-menu-parser

