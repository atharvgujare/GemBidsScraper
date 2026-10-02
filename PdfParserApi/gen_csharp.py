import sys
sys.path.append('c:/GemBidScraperA/GemBidScraper/GemBidScraper/GemBidScraper/PdfParserApi')
import city_resolver

pins = [f'["{k}"] = "{v}"' for k, v in city_resolver.PIN_PREFIX_MAP.items()]
cities = [f'"{c}"' for c in city_resolver.INDIAN_CITIES]

with open('c:/GemBidScraperA/GemBidScraper/GemBidScraper/GemBidScraper/PdfParserApi/scratch_csharp.txt', 'w', encoding='utf-8') as f:
    f.write('        private static readonly Dictionary<string, string> PinPrefixMap = new(StringComparer.OrdinalIgnoreCase)\n        {\n')
    # write in chunks of 5
    for i in range(0, len(pins), 5):
        chunk = ', '.join(pins[i:i+5])
        sep = ',' if i + 5 < len(pins) else ''
        f.write(f'            {chunk}{sep}\n')
    f.write('        };\n\n')

    f.write('        private static readonly string[] KnownCities = new[]\n        {\n')
    for i in range(0, len(cities), 4):
        chunk = ', '.join(cities[i:i+4])
        sep = ',' if i + 4 < len(cities) else ''
        f.write(f'            {chunk}{sep}\n')
    f.write('        };\n')

print('Success: wrote scratch_csharp.txt')
