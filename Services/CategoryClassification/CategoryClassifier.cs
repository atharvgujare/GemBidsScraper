using System.Text.RegularExpressions;
using GemBidScraper.Services.CategoryClassification;

public class CategoryClassifier
{
    private readonly List<CategoryDefinition> _categories;

    private static readonly (string Category, string SubCategory, string[] Phrases)[] StrongRules =
    {
        // =====================================================
        // DEFENCE & MILITARY
        // =====================================================
        (
            "DEFENCE",
            "WEAPONS_AMMO",
            new[]
            {
                "star plate 3 star",
                "star plate 2 star",
                "star plate 1 star",
                "star plate 4 star",
                "star plate for ids",
                "star plate for cds",
                "star plate for cas",
                "star plate air marshal",
                "star plate air vice marshal",
                "star plate air commodore",
                "procurement of star plates",
                "star plate",
                "shooting range",
                "ammunition",
                "bullet proof jacket",
                "target retriever system"
            }
        ),
        (
            "DEFENCE",
            "VEHICLES_MILITARY",
            new[]
            {
                "amc of beml tatra vehicle",
                "amc of beml tatra",
                "amc of tatra vehicle",
                "amc of tatra",
                "beml tatra",
                "tatra 815",
                "tatra vehicle",
                "armoured vehicle",
                "mine protected vehicle",
                "infantry combat vehicle"
            }
        ),

        // =====================================================
        // CHEMICALS & INDUSTRIAL GASES
        // =====================================================
        (
            "CHEMICALS",
            "INDUSTRIAL_CHEMICALS",
            new[]
            {
                "refrigerant gas",
                "freon gas r 404",
                "freon gas r 404a",
                "freon gas",
                "chlorodifluoro methane",
                "chlorodifluoromethane",
                "precipitated barium carbonate",
                "cyanoacrylate adhesive"
            }
        ),

        // =====================================================
        // AUTOMOBILE & VEHICLE SPARES
        // =====================================================
        (
            "AUTOMOBILE",
            "SPARES",
            new[]
            {
                "oem spares for automobiles",
                "lead storage battery",
                "lead acid storage battery",
                "clutch withdrawal",
                "door glass channel",
                "pin low speed select guide",
                "lv7 tata",
                "tata shim",
                "seat cover",
                "side view mirror",
                "brake chamber",
                "brake booster",
                "front brake pipe",
                "cam sprocket",
                "cam shaft",
                "door glass mechanism",
                "tata safari",
                "mahindra scorpio"
            }
        ),
        (
            "AUTOMOBILE",
            "VEHICLES",
            new[]
            {
                "repair and overhauling service cars",
                "repair and overhauling service vehicles",
                "overhauling service cars",
                "passenger car",
                "goods transport service",
                "vehicle loading",
                "e cart for goods",
                "forklifts",
                "forklift"
            }
        ),
        (
            "AUTOMOBILE",
            "TYRES",
            new[]
            {
                "otr tyres",
                "pneumatic tyres",
                "tubeless tyre",
                "tyre pressure",
                "tyre inflator"
            }
        ),

        // =====================================================
        // GENERAL SERVICES & MANPOWER
        // =====================================================
        (
            "SERVICES_GENERAL",
            "MANPOWER",
            new[]
            {
                "manpower outsourcing",
                "skilled manpower",
                "unskilled manpower",
                "highly skilled manpower",
                "security guard outsourcing"
            }
        ),

        // =====================================================
        // IT - OPERATING SYSTEMS
        // =====================================================
        (
            "IT",
            "OPERATING_SYSTEMS",
            new[]
            {
                "operating system software v3",
                "operating system software",
                "windows 11 pro",
                "windows 11 professional",
                "windows 10 pro",
                "windows 10 professional",
                "windows server 2022",
                "windows server 2019",
                "red hat enterprise linux",
                "ubuntu server"
            }
        ),

        // =====================================================
        // IT - EXCHANGE SERVERS
        // =====================================================
        (
            "IT",
            "EXCHANGE_SERVERS",
            new[]
            {
                "microsoft exchange server",
                "exchange server",
                "exchange online",
                "exchange migration",
                "hybrid exchange",
                "exchange hybrid"
            }
        ),

        // =====================================================
        // IT - MAIL SOLUTIONS
        // =====================================================
        (
            "IT",
            "MAIL_SOLUTIONS",
            new[]
            {
                "enterprise email solutions",
                "secure email gateway",
                "corporate email solution",
                "cloud email solution",
                "email security solutions"
            }
        ),

        // =====================================================
        // IT - FIREWALL & SECURITY
        // =====================================================
        (
            "IT",
            "FIREWALL_SECURITY",
            new[]
            {
                "next generation firewall",
                "ngfw",
                "unified threat management",
                "fortinet firewall",
                "palo alto firewall",
                "checkpoint firewall",
                "sophos firewall",
                "hardware firewall appliance"
            }
        ),

        // =====================================================
        // IT - NETWORKING
        // =====================================================
        (
            "IT",
            "NETWORKING",
            new[]
            {
                "core router",
                "edge router",
                "layer 3 switch",
                "layer 2 switch",
                "managed switch",
                "poe switch"
            }
        ),

        // =====================================================
        // IT - ENTERPRISE SOFTWARE
        // =====================================================
        (
            "IT",
            "ENTERPRISE_SOFTWARE",
            new[]
            {
                "microsoft office",
                "office productivity suite software",
                "it software for office use",
                "enterprise software",
                "sap s 4hana",
                "sap erp"
            }
        ),

        // =====================================================
        // IT - STORAGE
        // =====================================================
        (
            "IT",
            "STORAGE",
            new[]
            {
                "augmentation of storage",
                "commvault backup system",
                "enterprise storage area network",
                "san storage",
                "storage area network",
                "network attached storage",
                "nas storage array"
            }
        ),

        // =====================================================
        // IT - HARDWARE & PERIPHERALS
        // =====================================================
        (
            "IT",
            "HARDWARE",
            new[]
            {
                "desktop computers",
                "laptop",
                "computerised",
                "m4 chip laptop",
                "smart classroom setup",
                "tinkering lab",
                "workstations"
            }
        ),

        // =====================================================
        // IT - PRINTING
        // =====================================================
        (
            "IT",
            "PRINTING",
            new[]
            {
                "computer printers",
                "multifunction machines",
                "multifunction printer",
                "scanner printer",
                "toner cartridges",
                "ink cartridges"
            }
        ),

        // =====================================================
        // IT - CCTV
        // =====================================================
        (
            "IT",
            "CCTV",
            new[]
            {
                "cctv surveillance",
                "cctv camera",
                "remote video monitoring",
                "integrated security and surveillance"
            }
        ),

        // =====================================================
        // MECHANICAL
        // =====================================================
        (
            "MECHANICAL",
            "MACHINE_TOOLS",
            new[]
            {
                "buffing machine",
                "buffing wheel",
                "brush cutter machine",
                "sewing machine",
                "boring of",
                "grinding wheel",
                "machine tool"
            }
        ),
        (
            "MECHANICAL",
            "SPARES_GENERIC",
            new[]
            {
                "mech assembly",
                "mechanical assembly",
                "stud assembly",
                "shaft assembly",
                "travel motor assembly",
                "bell crank",
                "hook",
                "sprocket",
                "chain tensioner",
                "bearing assy",
                "air drier unit"
            }
        ),

        // =====================================================
        // SPORTS
        // =====================================================
        (
            "SPORTS",
            "SPORTS_EQUIPMENT",
            new[]
            {
                "resistance loop bands",
                "weightlifting bar",
                "gymnastic mats",
                "pole vault",
                "javelin",
                "recurve bow",
                "boxing gloves"
            }
        ),

        // =====================================================
        // MEDICAL
        // =====================================================
        (
            "MEDICAL",
            "EQUIPMENT_DEVICES",
            new[]
            {
                "video laryngoscope",
                "laryngoscope",
                "fluid warmer",
                "suction machine",
                "stethoscope",
                "defibrillator",
                "hemostatic agent"
            }
        ),

        // =====================================================
        // ELECTRICAL
        // =====================================================
        (
            "ELECTRICAL",
            "CABLES",
            new[]
            {
                "hdmi cable",
                "coaxial cable",
                "ecg trunk cable",
                "ecg lead wire"
            }
        )
    };

    public CategoryClassifier()
    {
        _categories = CategoryKeywordProvider.Categories;
    }

    /// <summary>
    /// Legacy classification overload for backward compatibility.
    /// </summary>
    public CategoryResult Classify(
        string? title,
        string? description)
    {
        return Classify(new BidClassificationInput
        {
            CardItemName = title,
            Specification = description
        });
    }

    /// <summary>
    /// Structured classification using multi-field weighted scoring,
    /// context guards, and non-IT domain protections.
    /// </summary>
    public CategoryResult Classify(BidClassificationInput input)
    {
        string primaryText = NormalizeText(
            $"{input.RelevantNotificationCategory} {input.ItemCategory} {input.PrimaryProductCategory}");

        string titleText = NormalizeText(
            $"{input.CardItemName} {input.BOQTitle}");

        string similarText = NormalizeText(input.SimilarCategory);

        string specText = NormalizeText(input.Specification);

        string fullText = NormalizeText(
            $"{primaryText} {titleText} {similarText} {specText}");

        if (string.IsNullOrWhiteSpace(fullText))
        {
            return CreateOtherResult();
        }

        // 1. Try Strong Rules on High-Priority fields first
        var primaryStrong = TryClassifyByStrongRule(primaryText);
        if (primaryStrong != null)
        {
            return primaryStrong;
        }

        var titleStrong = TryClassifyByStrongRule(titleText);
        if (titleStrong != null)
        {
            return titleStrong;
        }

        var fullStrong = TryClassifyByStrongRule(fullText);
        if (fullStrong != null)
        {
            // Check context guard on strong rule result if it's IT
            if (!(fullStrong.CategoryKey == "IT" && IsDefinitivelyNonIT(input, fullText)))
            {
                return fullStrong;
            }
        }

        // 2. Field-Weighted Keyword Scoring
        string? bestCategoryKey = null;
        string? bestSubCategoryKey = null;
        int bestScore = 0;
        int bestLongestKeywordScore = 0;
        int bestMatchedKeywordCount = 0;

        string? fallbackNonITCategoryKey = null;
        string? fallbackNonITSubCategoryKey = null;
        int fallbackNonITScore = 0;

        bool definitivelyNonIT = IsDefinitivelyNonIT(input, fullText);

        foreach (var category in _categories)
        {
            foreach (var subCategory in category.SubCategories)
            {
                int currentScore = 0;
                int matchedKeywordCount = 0;
                int longestKeywordScore = 0;

                foreach (var keyword in subCategory.Keywords)
                {
                    if (string.IsNullOrWhiteSpace(keyword))
                        continue;

                    string normalizedKeyword = NormalizeText(keyword);
                    if (normalizedKeyword.Length <= 1)
                        continue;

                    if (IsDangerousKeyword(normalizedKeyword))
                        continue;

                    // Evaluate matching across fields with weights:
                    // Primary (Notification/Item/Product Category): Weight 10
                    // Title (CardItemName/BOQTitle): Weight 6
                    // Similar Category: Weight 3
                    // Specification: Weight 1
                    int fieldWeight = 0;
                    if (ContainsWholeWord(primaryText, normalizedKeyword))
                    {
                        fieldWeight = 10;
                    }
                    else if (ContainsWholeWord(titleText, normalizedKeyword))
                    {
                        fieldWeight = 6;
                    }
                    else if (ContainsWholeWord(similarText, normalizedKeyword))
                    {
                        fieldWeight = 3;
                    }
                    else if (ContainsWholeWord(specText, normalizedKeyword))
                    {
                        fieldWeight = 1;
                    }

                    if (fieldWeight == 0)
                        continue;

                    // Apply context guards
                    if (!IsValidKeywordMatch(normalizedKeyword, category.Key, subCategory.Key, fullText))
                    {
                        continue;
                    }

                    matchedKeywordCount++;

                    int keywordScore = CalculateKeywordScore(normalizedKeyword);
                    int weightedKeywordScore = keywordScore * fieldWeight;

                    if (weightedKeywordScore > longestKeywordScore)
                    {
                        longestKeywordScore = weightedKeywordScore;
                    }

                    currentScore += weightedKeywordScore <= 30
                        ? 5
                        : weightedKeywordScore / 10;
                }

                if (matchedKeywordCount == 0)
                    continue;

                currentScore += longestKeywordScore * 10;

                // Track best non-IT fallback in case definitivelyNonIT is true
                if (category.Key != "IT" && currentScore > fallbackNonITScore)
                {
                    fallbackNonITScore = currentScore;
                    fallbackNonITCategoryKey = category.Key;
                    fallbackNonITSubCategoryKey = subCategory.Key;
                }

                // If this is IT, but the bid is definitively Non-IT, do not let IT take the lead
                if (category.Key == "IT" && definitivelyNonIT)
                {
                    continue;
                }

                bool isBetterMatch =
                    currentScore > bestScore ||
                    (currentScore == bestScore && longestKeywordScore > bestLongestKeywordScore) ||
                    (currentScore == bestScore && longestKeywordScore == bestLongestKeywordScore && matchedKeywordCount > bestMatchedKeywordCount);

                if (!isBetterMatch)
                    continue;

                bestScore = currentScore;
                bestLongestKeywordScore = longestKeywordScore;
                bestMatchedKeywordCount = matchedKeywordCount;
                bestCategoryKey = category.Key;
                bestSubCategoryKey = subCategory.Key;
            }
        }

        if (string.IsNullOrWhiteSpace(bestCategoryKey) || string.IsNullOrWhiteSpace(bestSubCategoryKey))
        {
            if (definitivelyNonIT && !string.IsNullOrWhiteSpace(fallbackNonITCategoryKey))
            {
                return CreateResult(fallbackNonITCategoryKey, fallbackNonITSubCategoryKey!);
            }

            return CreateOtherResult();
        }

        return CreateResult(bestCategoryKey, bestSubCategoryKey);
    }

    private static CategoryResult? TryClassifyByStrongRule(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        foreach (var rule in StrongRules)
        {
            foreach (var phrase in rule.Phrases)
            {
                if (ContainsWholeWord(text, NormalizeText(phrase)))
                {
                    return CreateResult(rule.Category, rule.SubCategory);
                }
            }
        }

        return null;
    }

    public static string NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string normalized = value.ToLowerInvariant();

        // Preserve technical tokens before stripping punctuation
        normalized = Regex.Replace(normalized, @"\bc\+\+", "cpp");
        normalized = Regex.Replace(normalized, @"\bc#", "csharp");
        normalized = Regex.Replace(normalized, @"\b\.net\b", "dotnet");
        normalized = Regex.Replace(normalized, @"\.net", "dotnet");

        normalized = normalized
            .Replace("_", " ")
            .Replace("-", " ")
            .Replace("/", " ")
            .Replace("\\", " ");

        normalized = Regex.Replace(
            normalized,
            @"[^\p{L}\p{N}\s]",
            " ");

        normalized = Regex.Replace(
            normalized,
            @"\s+",
            " ");

        return normalized.Trim();
    }

    private static bool ContainsWholeWord(string text, string keyword)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(keyword))
        {
            return false;
        }

        string pattern = $@"(?<!\w){Regex.Escape(keyword)}(?!\w)";

        return Regex.IsMatch(
            text,
            pattern,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static bool IsValidKeywordMatch(
        string normalizedKeyword,
        string categoryKey,
        string subCategoryKey,
        string fullNormalizedText)
    {
        // 1. Never match single-character tokens (prevents 'c', 'r', 'v' false positives)
        if (normalizedKeyword.Length <= 1)
        {
            return false;
        }

        // 2. Ambiguity guard for 'ids'
        if (normalizedKeyword == "ids")
        {
            // If military insignia / defence staff context is present, this is NOT cybersecurity IDS
            if (ContainsAny(fullNormalizedText, "star plate", "cds", "cas", "air marshal", "staff", "headquarters", "hq ids", "tri services", "mod", "insignia", "rank"))
            {
                return false;
            }

            // Must have cybersecurity indicators to count as IDS
            if (!ContainsAny(fullNormalizedText, "intrusion", "detection", "threat", "firewall", "security", "cyber", "suricata", "snort", "sensor", "packet"))
            {
                return false;
            }
        }

        // 3. Ambiguity guard for 'ips'
        if (normalizedKeyword == "ips")
        {
            if (ContainsAny(fullNormalizedText, "police", "officer", "cadre", "civil service"))
            {
                return false;
            }

            if (!ContainsAny(fullNormalizedText, "intrusion", "prevention", "firewall", "security", "threat", "cyber", "ngfw", "sensor"))
            {
                return false;
            }
        }

        // 4. Ambiguity guard for 'os'
        if (normalizedKeyword == "os")
        {
            if (ContainsAny(fullNormalizedText, "ordnance", "supervisor", "officer", "offset"))
            {
                return false;
            }

            if (!ContainsAny(fullNormalizedText, "operating system", "windows", "linux", "server", "desktop", "license", "rhel", "ubuntu", "distro", "hypervisor"))
            {
                return false;
            }
        }

        // 5. Ambiguity guard for 'dlp'
        if (normalizedKeyword == "dlp")
        {
            if (ContainsAny(fullNormalizedText, "defect liability", "liability period", "projector"))
            {
                return false;
            }

            if (!ContainsAny(fullNormalizedText, "data loss", "leak prevention", "endpoint", "compliance", "security"))
            {
                return false;
            }
        }

        // 6. Ambiguity guard for IT Storage
        if (categoryKey == "IT" && subCategoryKey == "STORAGE")
        {
            // Non-IT storage indicators disqualify IT > STORAGE
            if (ContainsAny(fullNormalizedText, "battery", "lead acid", "lead storage", "cold storage", "water storage", "storage tank", "storage shed", "drum", "cylinder"))
            {
                return false;
            }

            // Generic "storage" requires IT storage context
            if (normalizedKeyword == "storage" && !ContainsAny(fullNormalizedText, "san", "nas", "backup", "cloud", "server", "array", "nvme", "sas", "sata", "ssd", "hdd", "raid", "tape", "commvault", "veeam", "netapp", "terabyte", "petabyte", "tb", "pb"))
            {
                return false;
            }
        }

        // 7. Ambiguity guard for IT Services / AMC
        if (categoryKey == "IT" && subCategoryKey == "SERVICES")
        {
            // If automotive or heavy mechanical/civil is present without IT qualifiers, disqualify IT > SERVICES
            if (ContainsAny(fullNormalizedText, "vehicle", "tatra", "car", "truck", "bus", "automobile", "engine", "lift", "elevator", "plumbing", "civil", "horticulture"))
            {
                if (!ContainsAny(fullNormalizedText, "computer", "server", "laptop", "network", "firewall", "software", "switch", "router", "cctv", "ups", "it equipment", "data center"))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool IsDefinitivelyNonIT(BidClassificationInput input, string fullNormalizedText)
    {
        string primaryNormalized = NormalizeText(
            $"{input.RelevantNotificationCategory} {input.ItemCategory} {input.PrimaryProductCategory}");

        // Automotive / vehicle spare parts
        if (ContainsAny(primaryNormalized,
            "oem spares for automobiles", "spares for automobiles", "automobile spares",
            "vehicle spares", "lock washer", "needle roller bearings", "storage battery",
            "lead storage battery", "lead acid storage battery", "refrigerant gas"))
        {
            // Check if there is genuine IT software / hardware in primary fields
            if (!ContainsAny(primaryNormalized, "software", "computer", "server", "laptop", "operating system", "firewall", "router", "switch"))
            {
                return true;
            }
        }

        string cardNormalized = NormalizeText($"{input.CardItemName} {input.BOQTitle}");
        if (ContainsAny(cardNormalized,
            "amc of beml tatra", "amc of tatra", "tatra vehicle", "beml tatra",
            "star plate for ids", "star plate 3 star", "star plate 2 star", "star plate",
            "freon gas", "freon gas r 404"))
        {
            if (!ContainsAny(cardNormalized, "software", "computer", "server", "firewall", "router", "switch"))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsAny(string text, params string[] phrases)
    {
        foreach (var phrase in phrases)
        {
            if (ContainsWholeWord(text, phrase))
            {
                return true;
            }
        }
        return false;
    }

    private static int CalculateKeywordScore(string keyword)
    {
        int wordCount = keyword.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries)
            .Length;

        int characterCount = keyword.Length;

        return wordCount switch
        {
            >= 5 => 100 + characterCount,
            4 => 80 + characterCount,
            3 => 60 + characterCount,
            2 => 40 + characterCount,
            _ => 20 + characterCount
        };
    }

    private static bool IsDangerousKeyword(string keyword)
    {
        return keyword is
            "go" or
            "assembly" or
            "required" or
            "complete" or
            "available" or
            "repair" or
            "maintenance" or
            "installation" or
            "service";
    }

    private static CategoryResult CreateResult(
        string categoryKey,
        string subCategoryKey)
    {
        return new CategoryResult
        {
            CategoryKey = categoryKey,
            CategorySubKey = subCategoryKey
        };
    }

    private static CategoryResult CreateOtherResult()
    {
        return CreateResult(
            "OTHER",
            "OTHER");
    }
}
