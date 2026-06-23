using System.Globalization;
using System.Text;

namespace CzechNewsMap.Api.Services;

public sealed class CzechLocationCatalog
{
    private static readonly LocationRule[] LocationRules =
    {
        new("Praha", 50.0755, 14.4378, "praha", "praze", "prahy", "prahou", "pražský", "pražská", "pražské"),
        new("Brno", 49.1951, 16.6068, "brno", "brně", "brna", "brnem", "brněnský", "brněnská", "brněnské"),
        new("Ostrava", 49.8209, 18.2625, "ostrava", "ostravě", "ostravy", "ostravský", "ostravská", "ostravské"),
        new("Plzeň", 49.7384, 13.3736, "plzeň", "plzni", "plzen", "plzeňský", "plzeňská", "plzeňské"),
        new("Liberec", 50.7671, 15.0562, "liberec", "liberci", "liberecký", "liberecká", "liberecké"),
        new("Olomouc", 49.5938, 17.2509, "olomouc", "olomouci", "olomoucký", "olomoucká", "olomoucké"),
        new("České Budějovice", 48.9745, 14.4743, "české budějovice", "českých budějovicích", "budějovice", "budějovicích"),
        new("Hradec Králové", 50.2104, 15.8252, "hradec králové", "hradci králové", "královéhradecký", "královéhradecká"),
        new("Pardubice", 50.0343, 15.7812, "pardubice", "pardubicích", "pardubický", "pardubická", "pardubické"),
        new("Ústí nad Labem", 50.6607, 14.0323, "ústí nad labem", "ústecký", "ústecká", "ústecké"),
        new("Karlovy Vary", 50.2319, 12.8710, "karlovy vary", "karlových varech", "karlovarský", "karlovarská"),
        new("Zlín", 49.2244, 17.6628, "zlín", "zlíně", "zlínský", "zlínská", "zlínské"),
        new("Jihlava", 49.3961, 15.5912, "jihlava", "jihlavě", "jihlavy", "jihlavský", "vysočina"),
        new("Kladno", 50.1431, 14.1052, "kladno", "kladně", "kladenska", "kladensko"),
        new("Mladá Boleslav", 50.4114, 14.9032, "mladá boleslav", "mladé boleslavi", "boleslav", "boleslavi"),
        new("Most", 50.5030, 13.6362, "mostecko", "mostecku", "okres most"),
        new("Opava", 49.9387, 17.9026, "opava", "opavě", "opavsko", "opavský"),
        new("Karviná", 49.8567, 18.5432, "karviná", "karviné", "karvinsko", "karvinský"),
        new("Havířov", 49.7800, 18.4369, "havířov", "havířově"),
        new("Frýdek-Místek", 49.6819, 18.3673, "frýdek místek", "frýdku místku", "frýdecko místecko"),
        new("Čelákovice", 50.1604, 14.7501, "čelákovice", "čelákovicích", "čelákovic"),
        new("Brandýs nad Labem", 50.1871, 14.6633, "brandýs nad labem", "brandýse nad labem", "brandýs", "brandýse"),
        new("Kolín", 50.0281, 15.2016, "kolín", "kolíně", "kolínsko", "kolínský"),
        new("Kutná Hora", 49.9484, 15.2682, "kutná hora", "kutné hoře", "kutnohorsko", "kutnohorský"),
        new("Příbram", 49.6899, 14.0104, "příbram", "příbrami", "příbramsko"),
        new("Beroun", 49.9638, 14.0720, "beroun", "berouně", "berounsko"),
        new("Mělník", 50.3513, 14.4741, "mělník", "mělníku", "mělnicko"),
        new("Nymburk", 50.1861, 15.0417, "nymburk", "nymburku", "nymbursko"),
        new("Rakovník", 50.1037, 13.7334, "rakovník", "rakovníku", "rakovnicko"),
        new("Tábor", 49.4144, 14.6578, "město tábor", "městě tábor", "táborsko", "táborský", "táborská"),
        new("Písek", 49.3088, 14.1475, "písek", "písku", "písecko"),
        new("Strakonice", 49.2614, 13.9024, "strakonice", "strakonicích", "strakonicko"),
        new("Český Krumlov", 48.8127, 14.3175, "český krumlov", "českém krumlově", "krumlov", "krumlově"),
        new("Třebíč", 49.2149, 15.8817, "třebíč", "třebíči", "třebíčsko"),
        new("Znojmo", 48.8555, 16.0488, "znojmo", "znojmě", "znojemsko"),
        new("Kroměříž", 49.2917, 17.3994, "kroměříž", "kroměříži", "kroměřížsko"),
        new("Uherské Hradiště", 49.0698, 17.4597, "uherské hradiště", "uherském hradišti", "hradiště"),
        new("Přerov", 49.4551, 17.4509, "přerov", "přerově", "přerovsko"),
        new("Prostějov", 49.4719, 17.1118, "prostějov", "prostějově", "prostějovsko"),
        new("Šumperk", 49.9653, 16.9706, "šumperk", "šumperku", "šumpersko"),
        new("Jeseník", 50.2294, 17.2046, "jeseník", "jeseníku", "jesenicko"),
        new("Teplice", 50.6404, 13.8245, "teplice", "teplicích", "teplicko"),
        new("Děčín", 50.7726, 14.2128, "děčín", "děčíně", "děčínsko"),
        new("Chomutov", 50.4605, 13.4178, "chomutov", "chomutově", "chomutovsko"),
        new("Litoměřice", 50.5335, 14.1318, "litoměřice", "litoměřicích", "litoměřicko"),
        new("Česká Lípa", 50.6855, 14.5376, "česká lípa", "české lípě", "českolipsko"),
        new("Jablonec nad Nisou", 50.7243, 15.1711, "jablonec nad nisou", "jablonci nad nisou", "jablonec", "jablonci"),
        new("Trutnov", 50.5610, 15.9127, "trutnov", "trutnově", "trutnovsko"),
        new("Náchod", 50.4167, 16.1629, "náchod", "náchodě", "náchodsko"),
        new("Chrudim", 49.9511, 15.7956, "chrudim", "chrudimi", "chrudimsko"),
        new("Svitavy", 49.7559, 16.4683, "svitavy", "svitavách", "svitavsko"),
        new("Vyškov", 49.2775, 16.9988, "vyškov", "vyškově", "vyškovsko"),
        new("Hodonín", 48.8489, 17.1324, "hodonín", "hodoníně", "hodonínsko"),
        new("Břeclav", 48.7589, 16.8820, "břeclav", "břeclavi", "břeclavsko"),
        new("Vsetín", 49.3387, 17.9962, "vsetín", "vsetíně", "vsetínsko"),
        new("Nový Jičín", 49.5944, 18.0103, "nový jičín", "novém jičíně", "novo jičínsko", "novo jičínsku"),
        new("Cheb", 50.0796, 12.3739, "cheb", "chebu", "chebsko"),
        new("Sokolov", 50.1813, 12.6401, "sokolov", "sokolově", "sokolovsko"),
        new("Domažlice", 49.4405, 12.9298, "domažlice", "domažlicích", "domažlicko"),
        new("Tachov", 49.7953, 12.6337, "tachov", "tachově", "tachovsko"),
        new("Rokycany", 49.7427, 13.5946, "rokycany", "rokycanech", "rokycansko"),
        new("Klatovy", 49.3956, 13.2951, "klatovy", "klatovech", "klatovsko"),
        new("Pelhřimov", 49.4313, 15.2234, "pelhřimov", "pelhřimově", "pelhřimovsko"),
        new("Havlíčkův Brod", 49.6079, 15.5807, "havlíčkův brod", "havlíčkově brodě", "havlíčkobrodsko"),
        new("Žďár nad Sázavou", 49.5626, 15.9392, "žďár nad sázavou", "žďáru nad sázavou", "žďársko"),
        new("Hodkovice nad Mohelkou", 50.6659, 15.0898, "hodkovice", "hodkovicích", "hodkovice nad mohelkou")
    };

    public LocationMatch? Find(string text)
    {
        var normalized = Normalize(text);

        foreach (var location in LocationRules)
        {
            if (location.Patterns.Any(pattern => ContainsNormalized(normalized, pattern)))
            {
                return new LocationMatch(location.Name, location.Latitude, location.Longitude);
            }
        }

        return null;
    }

    private static bool ContainsNormalized(string normalizedText, string pattern)
    {
        return normalizedText.Contains(Normalize(pattern));
    }

    private static string Normalize(string text)
    {
        var decomposed = (text ?? "").ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length + 2);
        builder.Append(' ');

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.IsLetterOrDigit(c) ? c : ' ');
        }

        builder.Append(' ');
        return " " + string.Join(" ", builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) + " ";
    }

    public sealed record LocationMatch(string Name, double Latitude, double Longitude);

    private sealed record LocationRule(string Name, double Latitude, double Longitude, params string[] Patterns);
}