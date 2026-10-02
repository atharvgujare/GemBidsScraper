using GemBidScraper.Attributes;
using GemBidScraper.Models;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GemBidScraper.GeMBIdMapper
{
    public static class GeMBidMapper_2
    {
        // -------------------------------------------------------
        // Compiled regexes reused across every mapping call
        // -------------------------------------------------------
        private static readonly Regex DigitsOnlyRegex = new(@"[^\d]", RegexOptions.Compiled);
        private static readonly Regex DecimalOnlyRegex = new(@"[^\d.]", RegexOptions.Compiled);

        // decimal(18,2) in SQL Server can hold at most 16 integer digits +
        // 2 decimal digits. A loose keyword match (see step 3 below) can
        // pull a garbage 20+ digit fragment (IFSC code glued to a phone
        // number, an account number, etc.) into a numeric column.
        // decimal.TryParse happily accepts that (native decimal supports
        // ~28-29 digits), but SqlClient throws "Arithmetic overflow error
        // converting numeric to data type numeric" at insert time and, if
        // you're inside a MERGE/batch transaction, rolls back everything
        // in that batch. Reject out-of-range values here instead — a null
        // field beats losing 1000 good rows to one bad one.
        private const decimal MaxSafeDecimal18_2 = 9999999999999999.99m;
        private const decimal MinSafeDecimal18_2 = -9999999999999999.99m;

        // -------------------------------------------------------
        // Reflection metadata cache — computed once per T
        // -------------------------------------------------------
        private sealed class TypeMap
        {
            public PropertyInfo? CreatedOn;
            public PropertyInfo? PdfUrl;
            public PropertyInfo? JsonData;
            public PropertyInfo? TrainingModule;
            public (PropertyInfo Property, string NormalizedName)[] Properties = Array.Empty<(PropertyInfo, string)>();
        }

        private static readonly ConcurrentDictionary<Type, TypeMap> _typeMapCache = new();

        private static TypeMap GetTypeMap(Type type)
        {
            return _typeMapCache.GetOrAdd(type, t =>
            {
                var map = new TypeMap
                {
                    CreatedOn = t.GetProperty("CreatedOn"),
                    PdfUrl = t.GetProperty("PdfUrl"),
                    JsonData = t.GetProperty("JsonData"),
                    TrainingModule = t.GetProperty("TrainingModule")
                };

                var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.Name != "Id")
                            .ToArray();

                var list = new (PropertyInfo, string)[props.Length];

                for (int i = 0; i < props.Length; i++)
                {
                    var property = props[i];
                    var attribute = property.GetCustomAttribute<JsonFieldAttribute>();

                    string modelName = attribute != null
                        ? Normalize(attribute.Name)
                        : Normalize(property.Name);

                    list[i] = (property, modelName);
                }

                map.Properties = list;

                return map;
            });
        }

        private static readonly Dictionary<string, string[]> Alias = new()
        {
            // =========================
            // Bid
            // =========================

            ["bidnumber"] = new[]
     {
        "Bid Number"
    },

            ["bidopeningdatetime"] = new[]
{
    "Bid Opening Date/Time",
    "Bid Opening Date",
    "Opening Date/Time",
    "Tender Opening Date",
    "Technical Bid Opening Date",
    "Technical Opening Date",
    "Opening Time"
},

            ["bidvaliditydays"] = new[]
{
    "Bid Offer Validity (From End Date)",
    "Bid Validity",
    "Bid Validity Days",
    "Offer Validity",
    "Offer Validity (Days)"
},

            ["typeofbid"] = new[]
     {
        "Type of Bid"
    },

            ["evaluationmethod"] = new[]
     {
        "Evaluation Method"
    },

            // =========================
            // Buyer
            // =========================

            ["ministry"] = new[]
{
    "Ministry/State Name",
    "Ministry Name",
    "Ministry"
},

            ["departmentname"] = new[]
{
    "Department Name",
    "Department"
},

            ["organisationname"] = new[]
{
    "Organisation Name",
    "Organization Name",
    "Organisation",
    "Organization"
},

            ["officename"] = new[]
{
    "Office Name",
    "Office",
    "Buyer Office",
    "Purchaser Office"
},

            ["contactdetailsofgrievanceredressal"] = new[]
     {
        "Contact details of Grievance redressal",
        "Contact details of Grievance redressal officer"
    },

            // =========================
            // Product
            // =========================

            ["totalquantity"] = new[]
{
    "Total Quantity",
    "Quantity",
    "Total Qty",
    "Qty"
},

            ["itemcategory"] = new[]
{
    "Item Category",
    "Category",
    "Item Name",
    "Product Category"
},

            ["boqTitle"] = new[]
                 {
                    "BOQ Title",
                    "BOQ",
                    "boqTitle"
                },

            ["primaryproductcategory"] = new[]
     {
        "Primary Product Category",
        "Primary product category"
    },

            ["similarcategory"] = new[]
     {
        "Similar Category"
    },

            ["contractperiod"] = new[]
     {
        "Contract Period"
    },

            // =========================
            // Eligibility
            // =========================

            ["minimumaverageannualturnover"] = new[]
     {
        "Minimum Average Annual Turnover",
        "Minimum Average Annual Turnover of the bidder (For 3 Years)"
    },

            ["oemaverageturnover"] = new[]
     {
        "OEM Average Turnover"
    },

            ["yearsofpastexperiencerequired"] = new[]
     {
        "Years of Past Experience Required",
        "Years of Past Experience Required for same/similar service"
    },

            ["pastexperiencerequired"] = new[]
     {
        "Past Experience Required"
    },

            ["documentrequiredfromseller"] = new[]
     {
        "Document Required from Seller",
        "Document required from seller"
    },

            // =========================
            // Auto Extension
            // =========================

            ["minimumnumberofbidsrequiredtodisableautomaticbidextension"] = new[]
     {
        "Minimum number of bids required to disable automatic bid extension"
    },

            ["numberofdaysforwhichbidwouldbeautoextended"] = new[]
     {
        "Number of days for which Bid would be auto--extended",
        "Number of days for which Bid would be auto-extended"
    },

            ["numberofautoextensioncount"] = new[]
     {
        "Number of Auto Extension count"
    },

            // =========================
            // Reverse Auction
            // =========================

            ["bidtoraenabled"] = new[]
     {
        "Bid to RA enabled"
    },

            ["raqualificationrule"] = new[]
     {
        "RA Qualification Rule"
    },

            // =========================
            // Inspection
            // =========================

            ["inspectionrequired"] = new[]
     {
        "Inspection Required",
        "Inspection Required (By Empanelled Inspection Authority / Agencies pre-registered with GeM)"
    },

            ["inspectionbybuyerownagency"] = new[]
     {
        "Inspection By Buyer Own Agency"
    },

            ["inspectiontype"] = new[]
     {
        "Inspection Type"
    },

            ["inspectionagency"] = new[]
     {
        "Inspection Agency"
    },

            // =========================
            // Financial
            // =========================

            ["estimatedbidvalue"] = new[]
     {
        "Estimated Bid Value",
        "Estimated Value"
    },

            ["emdamount"] = new[]
     {
        "EMD Amount"
    },

            ["epbgpercentage"] = new[]
     {
        "ePBG Percentage",
        "ePBG Percentage(%)"
    },

            ["epbgdurationmonths"] = new[]
     {
        "Duration of ePBG required (Months).",
        "Duration of ePBG required (Months)"
    },

            ["advisorybank"] = new[]
     {
        "Advisory Bank"
    },

            // =========================
            // Preference
            // =========================

            ["msepurchasepreference"] = new[]
     {
        "MSE Purchase Preference"
    },

            ["miipurchasepreference"] = new[]
     {
        "MII Purchase Preference"
    },

            ["purchasepreferencepercentage"] = new[]
     {
        "L1+X% / Purchase Preference to MII sellers availabele upto price within L1+X%",
        "Percentage of Bid quantity/amount for MSE OEMs/ Service Provider Purchase preference"
    },

            ["maximumpurchasepreferencepercentage"] = new[]
     {
        "Maximum Percentage of Bid quantity for MII purchase preference",
        "Maximum Percentage of Bid quantity for MSE OEMs/ Service Provider Purchase preference"
    },

            // =========================
            // Legal
            // =========================

            ["arbitrationclause"] = new[]
     {
        "Arbitration Clause"
    },

            ["mediationclause"] = new[]
     {
        "Mediation Clause"
    },

            // =========================
            // Technical
            // =========================

            ["technicalspecificationjson"] = new[]
                {
                    "Technical Specification",
                    "Technical Specifications",
                    "Technical Specification Details"
                },

            ["specification"] = new[]
                {
                    "Specification",
                    "Technical Specification",
                    "Buyer Specification"
                },

            ["specificationparametername"] = new[]
                {
                    "Specification Parameter Name",
                    "Parameter",
                    "Parameter Name"
                },

            ["values"] = new[]
                {
                    "Values",
                    "Value"
                },

            ["physicalcharacteristics"] = new[]
     {
        "Physical Characteristics"
    },

            ["material"] = new[]
                {
                    "Material",
                    "Material Type",
                    "Base Material"
                },

            ["surface"] = new[]
                {
                    "Surface",
                    "Surface Finish",
                    "Finish",
                    "Coating"
                },

            ["layers"] = new[]
            {
                "Layers",
                "Layer"
            },

            ["warrantytext"] = new[]
                {
                    "Warranty",
                    "Warranty Text",
                    "Warranty Period"
                },

            ["servicerequirement"] = new[]
                {
                    "Service Requirement",
                    "Requirement",
                    "Scope of Work"
                },

            ["serviceinclusions"] = new[]
                {
                    "Service Inclusions",
                    "Included Services"
                },
            ["trainingmodule"] = new[]
            {
                "Training Module",
                "Training"
            },

            // =========================
            // GeM Search
            // =========================

            ["gemarptssearchedstrings"] = new[]
     {
        "GeMARPTS / Searched Strings used in GeMARPTS"
    },

            ["gemarptssearchedresults"] = new[]
     {
        "GeMARPTS / Searched Result generated in GeMARPTS"
    },

            ["relevantcategoriesselectedfornotification"] = new[]
     {
        "Relevant Categories selected for notification"
    },

            // =========================
            // Misc
            // =========================

            ["pastperformance"] = new[]
     {
        "Past Performance"
    },

            ["paymenttimelines"] = new[]
     {
        "Payment Timelines"
    },

            ["autocracdays"] = new[]
     {
        "Auto CRAC Days"
    },

            ["financialdocumentrequired"] = new[]
     {
        "Financial Document Required"
    },

            ["required"] = new[]
     {
        "Required"
    },

            ["itcavailabletobuyer"] = new[]
     {
        "ITC available to buyer"
    },

            ["miicompliance"] = new[]
     {
        "MII Compliance"
    },

            ["buyerspecificationdocument"] = new[]
     {
        "Buyer Specification Document"
    },

            ["boqdetaildocument"] = new[]
     {
        "BOQ Detail Document"
    },

            ["specificationdocument"] = new[]
     {
        "Specification Document"
    },
            ["bidenddatetime"] = new[]
            {
                "Bid End Date/Time",
                "Bid End Date",
                "Bid Closing Date",
                "Bid Closing Date/Time",
                "Closing Date/Time",
                "Tender Closing Date",
                "Last Date of Submission",
                "Bid Submission End Date",
                "Bid Submission End Date/Time"
            },
            ["consigneename"] = new[]
                {
                    "Consignee",
                    "Consignee Name",
                    "Consignee Reporting/Officer",
                    "Reporting Officer"
                },

            ["consigneeaddress"] = new[]
                    {
                        "Consignee Address",
                        "Delivery Address"
                    },

            ["consigneequantity"] = new[]
                {
                    "Consignee Quantity",
                    "Quantity",
                    "Qty"
                },

            ["location"] = new[]
                {
                    "Location",
                    "City",
                    "Consignee City",
                    "Delivery City",
                    "Delivery Location"
                },
        };

        public static T Map<T>(
    string? pdfUrl,
    JsonElement data,
    string? bidNumber,
    string? itemName,
    string? quantity,
    string? ministry,
    string? department,
    string? startDate,
    string? endDate) where T : new()
        {
            T model = MapCore<T>(pdfUrl, data);

            var type = typeof(T);

            type.GetProperty("BidNumber")?.SetValue(model, bidNumber);

            type.GetProperty("CardItemName")?.SetValue(model, itemName);

            if (int.TryParse(quantity, out int qty))
                type.GetProperty("CardQuantity")?.SetValue(model, qty);

            type.GetProperty("CardMinistry")?.SetValue(model, ministry);

            type.GetProperty("CardDepartment")?.SetValue(model, department);

            if (DateTime.TryParse(startDate, out DateTime start))
                type.GetProperty("CardStartDate")?.SetValue(model, start);

            if (DateTime.TryParse(endDate, out DateTime end))
                type.GetProperty("CardEndDate")?.SetValue(model, end);

            return model;
        }

        /// <summary>
        /// Preferred entry point. Avoids the old pattern of hand-building a
        /// JSON string ($$"""{"PdfUrl":"{{item.PdfUrl}}", ...}""") and
        /// re-parsing it — which was both slower (re-serialize + re-parse)
        /// and unsafe (a PdfUrl containing a quote or backslash would
        /// produce invalid JSON / mis-parse). Pass the pieces straight
        /// through instead.
        /// </summary>
        public static T Map<T>(string? pdfUrl, JsonElement data) where T : new()
        {
            return MapCore<T>(pdfUrl, data);
        }

        private static T MapCore<T>(string? pdfUrl, JsonElement? data) where T : new()
        {
            T model = new();

            var typeMap = GetTypeMap(typeof(T));

            // ----------------------------
            // Set CreatedOn automatically
            // ----------------------------
            if (typeMap.CreatedOn != null &&
                typeMap.CreatedOn.PropertyType == typeof(DateTime))
            {
                typeMap.CreatedOn.SetValue(model, DateTime.Now);
            }

            // ----------------------------
            // Set PdfUrl automatically
            // ----------------------------
            if (pdfUrl != null && typeMap.PdfUrl != null)
            {
                typeMap.PdfUrl.SetValue(model, pdfUrl);
            }

            if (data == null)
                return model;

            JsonElement dataElement = data.Value;

            // ----------------------------
            // Store complete JSON
            // ----------------------------
            typeMap.JsonData?.SetValue(model, dataElement.GetRawText());

            var jsonDictionary = new Dictionary<string, string>();

            foreach (var property in dataElement.EnumerateObject())
            {
                string key = Normalize(property.Name);

                jsonDictionary[key] = property.Value.ToString();

                foreach (var alias in Alias)
                {
                    foreach (var aliasName in alias.Value)
                    {
                        if (Normalize(aliasName) == key)
                        {
                            jsonDictionary[alias.Key] = property.Value.ToString();
                        }
                    }
                }
            }



            foreach (var (property, modelName) in typeMap.Properties)
            {
                string? value = null;

                // ------------------------------------
                // 1. Exact Match
                // ------------------------------------
                if (jsonDictionary.TryGetValue(modelName, out var exact))
                {
                    value = exact;
                }

                // ------------------------------------
                // 2. Alias Match
                // ------------------------------------
                else if (Alias.TryGetValue(modelName, out var aliases))
                {
                    foreach (var alias in aliases)
                    {
                        var normalizedAlias = Normalize(alias);

                        if (jsonDictionary.TryGetValue(normalizedAlias, out var aliasValue))
                        {
                            value = aliasValue;
                            break;
                        }
                    }
                }

                // ------------------------------------
                // 3. Keyword Match (STRING PROPERTIES ONLY)
                // ------------------------------------
                // A loose "contains" match across every JSON field is
                // useful for free-text columns (Specification, Material,
                // ServiceRequirement, etc.) but is dangerous for numeric /
                // date / bool columns: it can match an unrelated field
                // (an IFSC code, an account number, a phone number
                // fragment) and hand it to ConvertValue, which will strip
                // non-digits and produce a huge/garbage number. That
                // garbage can then overflow the destination SQL column at
                // insert time and roll back an entire batch. Only strings
                // are safe to fill this way — numeric/date/bool columns
                // must come from an exact or alias match.
                if (value == null && property.PropertyType == typeof(string))
                {
                    foreach (var jsonField in jsonDictionary)
                    {
                        foreach (var keyword in modelName.Split(
                                     new[] { "name", "percentage", "required" },
                                     StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (keyword.Length < 4)
                                continue;

                            if (jsonField.Key.Contains(keyword))
                            {
                                value = jsonField.Value;
                                break;
                            }
                        }

                        if (value != null)
                            break;
                    }
                }

                if (value == null)
                    continue;

                property.SetValue(
                    model,
                    ConvertValue(value, property.PropertyType)
                );
            }

            // =======================================
            // Special Mapping (Fallback Only)
            // =======================================

            SetIfNull(model, "ConsigneeName", FindConsigneeName(jsonDictionary));
            SetIfNull(model, "ConsigneeAddress", FindConsigneeAddress(jsonDictionary));
            SetIfNull(model, "ConsigneeQuantity", FindConsigneeQuantity(jsonDictionary));
            SetIfNull(model, "Location", FindLocation(jsonDictionary, model));

            SetIfNull(model, "Material", FindMaterial(jsonDictionary));
            SetIfNull(model, "Surface", FindSurface(jsonDictionary));
            SetIfNull(model, "Layers", FindLayers(jsonDictionary));

            SetIfNull(model, "WarrantyText", FindWarranty(jsonDictionary));

            SetIfNull(model, "ServiceRequirement", FindServiceRequirement(jsonDictionary));
            SetIfNull(model, "ServiceInclusions", FindServiceInclusions(jsonDictionary));

            SetIfNull(model, "TrainingModule", FindTraining(jsonDictionary));

            // Training
            typeMap.TrainingModule?.SetValue(model, FindTraining(jsonDictionary));

            return model;
        }

        private static string Normalize(string value)
        {
            return value
                .Replace(" ", "")
                .Replace("/", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace(".", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("%", "")
                .Replace(":", "")
                .Replace(",", "")
                .Replace("?", "")
                .Trim()
                .ToLowerInvariant();
        }

        private static void SetIfNull<T>(
    T model,
    string propertyName,
    object? value)
        {
            if (value == null)
                return;

            var property = typeof(T).GetProperty(propertyName);

            if (property == null)
                return;

            var currentValue = property.GetValue(model);

            if (currentValue == null ||
                string.IsNullOrWhiteSpace(currentValue.ToString()))
            {
                property.SetValue(model, value);
            }
        }
        private static bool ContainsKeyword(string source, params string[] keywords)
        {
            source = Normalize(source);

            foreach (var keyword in keywords)
            {
                if (source.Contains(Normalize(keyword)))
                    return true;
            }

            return false;
        }

        private static string? FindConsigneeName(
     Dictionary<string, string> json)
        {
            foreach (var item in json)
            {
                if (ContainsKeyword(
                    item.Key,
                    "Consignee Name",
                    "Consignee Reporting/Officer",
                    "Reporting Officer"))
                {
                    return item.Value;
                }
            }

            return null;
        }

        private static string? FindConsigneeAddress(
     Dictionary<string, string> json)
        {
            foreach (var item in json)
            {
                if (ContainsKeyword(
                    item.Key,
                    "Consignee Address",
                    "Delivery Address"))
                {
                    return item.Value;
                }
            }

            return null;
        }

        private static string? FindConsigneeQuantity(
     Dictionary<string, string> json)
        {
            foreach (var item in json)
            {
                if (ContainsKeyword(
                    item.Key,
                    "Consignee Quantity",
                    "Quantity"))
                {
                    return item.Value;
                }
            }

            return null;
        }

        private static string? FindLocation(
            Dictionary<string, string> json,
            object model)
        {
            foreach (var item in json)
            {
                if (ContainsKeyword(
                    item.Key,
                    "Location",
                    "City",
                    "Delivery Location",
                    "Consignee City"))
                {
                    if (!string.IsNullOrWhiteSpace(item.Value))
                        return item.Value.Trim();
                }
            }

            if (model is GeMBidExtract extract)
            {
                return ResolveCity(extract.ConsigneeAddress)
                    ?? ResolveCity(extract.ConsigneeName)
                    ?? ResolveCity(extract.OfficeName);
            }

            return null;
        }

        private static readonly Dictionary<string, string> PinPrefixMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["110"] = "New Delhi", ["121"] = "Faridabad", ["122"] = "Gurugram", ["123"] = "Rewari", ["124"] = "Rohtak",
            ["125"] = "Hisar", ["126"] = "Jind", ["127"] = "Bhiwani", ["131"] = "Sonipat", ["132"] = "Panipat",
            ["133"] = "Ambala", ["134"] = "Panchkula", ["135"] = "Yamunanagar", ["136"] = "Kurukshetra", ["140"] = "Mohali",
            ["141"] = "Ludhiana", ["142"] = "Moga", ["143"] = "Amritsar", ["144"] = "Jalandhar", ["145"] = "Pathankot",
            ["146"] = "Hoshiarpur", ["147"] = "Patiala", ["148"] = "Sangrur", ["151"] = "Bathinda", ["152"] = "Firozpur",
            ["160"] = "Chandigarh", ["171"] = "Shimla", ["172"] = "Solan", ["173"] = "Solan", ["174"] = "Bilaspur",
            ["175"] = "Mandi", ["176"] = "Kangra", ["177"] = "Hamirpur", ["180"] = "Jammu", ["181"] = "Samba",
            ["182"] = "Udhampur", ["184"] = "Kathua", ["185"] = "Rajouri", ["190"] = "Srinagar", ["191"] = "Budgam",
            ["192"] = "Anantnag", ["193"] = "Baramulla", ["194"] = "Leh", ["201"] = "Noida", ["202"] = "Aligarh",
            ["203"] = "Bulandshahr", ["204"] = "Hathras", ["205"] = "Mainpuri", ["206"] = "Etawah", ["207"] = "Etah",
            ["208"] = "Kanpur", ["209"] = "Unnao", ["210"] = "Banda", ["211"] = "Prayagraj", ["212"] = "Kaushambi",
            ["221"] = "Varanasi", ["222"] = "Jaunpur", ["223"] = "Azamgarh", ["224"] = "Ayodhya", ["225"] = "Barabanki",
            ["226"] = "Lucknow", ["227"] = "Amethi", ["228"] = "Sultanpur", ["229"] = "Raebareli", ["230"] = "Pratapgarh",
            ["231"] = "Mirzapur", ["232"] = "Chandauli", ["233"] = "Ghazipur", ["241"] = "Hardoi", ["242"] = "Shahjahanpur",
            ["243"] = "Bareilly", ["244"] = "Moradabad", ["245"] = "Hapur", ["246"] = "Bijnor", ["247"] = "Saharanpur",
            ["248"] = "Dehradun", ["249"] = "Haridwar", ["250"] = "Meerut", ["251"] = "Muzaffarnagar", ["261"] = "Sitapur",
            ["262"] = "Lakhimpur Kheri", ["263"] = "Nainital", ["271"] = "Gonda", ["272"] = "Basti", ["273"] = "Gorakhpur",
            ["274"] = "Deoria", ["281"] = "Mathura", ["282"] = "Agra", ["283"] = "Firozabad", ["284"] = "Jhansi",
            ["285"] = "Jalaun", ["301"] = "Alwar", ["302"] = "Jaipur", ["303"] = "Jaipur", ["304"] = "Tonk",
            ["305"] = "Ajmer", ["311"] = "Bhilwara", ["312"] = "Chittorgarh", ["313"] = "Udaipur", ["314"] = "Dungarpur",
            ["321"] = "Bharatpur", ["322"] = "Sawai Madhopur", ["324"] = "Kota", ["325"] = "Baran", ["326"] = "Jhalawar",
            ["331"] = "Churu", ["332"] = "Sikar", ["333"] = "Jhunjhunu", ["334"] = "Bikaner", ["335"] = "Sri Ganganagar",
            ["341"] = "Nagaur", ["342"] = "Jodhpur", ["343"] = "Jalore", ["344"] = "Barmer", ["345"] = "Jaisalmer",
            ["360"] = "Rajkot", ["361"] = "Jamnagar", ["362"] = "Junagadh", ["363"] = "Surendranagar", ["364"] = "Bhavnagar",
            ["365"] = "Amreli", ["370"] = "Kutch", ["380"] = "Ahmedabad", ["382"] = "Gandhinagar", ["383"] = "Sabarkantha",
            ["384"] = "Mehsana", ["385"] = "Banaskantha", ["387"] = "Kheda", ["388"] = "Anand", ["389"] = "Panchmahal",
            ["390"] = "Vadodara", ["391"] = "Vadodara", ["392"] = "Bharuch", ["393"] = "Bharuch", ["394"] = "Surat",
            ["395"] = "Surat", ["396"] = "Valsad", ["400"] = "Mumbai", ["401"] = "Thane", ["402"] = "Raigad",
            ["403"] = "Goa", ["410"] = "Pune", ["411"] = "Pune", ["412"] = "Pune", ["413"] = "Solapur",
            ["414"] = "Ahmednagar", ["415"] = "Satara", ["416"] = "Kolhapur", ["421"] = "Kalyan", ["422"] = "Nashik",
            ["423"] = "Malegaon", ["424"] = "Dhule", ["425"] = "Jalgaon", ["431"] = "Chhatrapati Sambhajinagar", ["440"] = "Nagpur",
            ["441"] = "Nagpur", ["442"] = "Wardha", ["443"] = "Buldhana", ["444"] = "Amravati", ["445"] = "Yavatmal",
            ["450"] = "Khandwa", ["451"] = "Khargone", ["452"] = "Indore", ["453"] = "Indore", ["454"] = "Dhar",
            ["455"] = "Dewas", ["456"] = "Ujjain", ["457"] = "Ratlam", ["458"] = "Mandsaur", ["460"] = "Betul",
            ["461"] = "Hoshangabad", ["462"] = "Bhopal", ["464"] = "Vidisha", ["465"] = "Shajapur", ["466"] = "Sehore",
            ["470"] = "Sagar", ["471"] = "Chhatarpur", ["472"] = "Tikamgarh", ["473"] = "Guna", ["474"] = "Gwalior",
            ["475"] = "Gwalior", ["476"] = "Morena", ["477"] = "Bhind", ["480"] = "Chhindwara", ["481"] = "Balaghat",
            ["482"] = "Jabalpur", ["483"] = "Jabalpur", ["484"] = "Shahdol", ["485"] = "Satna", ["486"] = "Rewa",
            ["490"] = "Bhilai", ["491"] = "Durg", ["492"] = "Raipur", ["493"] = "Raipur", ["494"] = "Jagdalpur",
            ["495"] = "Bilaspur", ["496"] = "Raigarh", ["497"] = "Ambikapur", ["500"] = "Hyderabad", ["501"] = "Rangareddy",
            ["502"] = "Sangareddy", ["503"] = "Nizamabad", ["504"] = "Adilabad", ["505"] = "Karimnagar", ["506"] = "Warangal",
            ["507"] = "Khammam", ["508"] = "Nalgonda", ["509"] = "Mahbubnagar", ["515"] = "Anantapur", ["516"] = "Kadapa",
            ["517"] = "Tirupati", ["518"] = "Kurnool", ["520"] = "Vijayawada", ["521"] = "Krishna", ["522"] = "Guntur",
            ["523"] = "Prakasam", ["524"] = "Nellore", ["530"] = "Visakhapatnam", ["531"] = "Visakhapatnam", ["532"] = "Srikakulam",
            ["533"] = "East Godavari", ["534"] = "West Godavari", ["535"] = "Vizianagaram", ["560"] = "Bengaluru", ["561"] = "Bengaluru Rural",
            ["562"] = "Bengaluru Rural", ["563"] = "Kolar", ["570"] = "Mysuru", ["571"] = "Mandya", ["572"] = "Tumakuru",
            ["573"] = "Hassan", ["574"] = "Dakshina Kannada", ["575"] = "Mangaluru", ["576"] = "Udupi", ["577"] = "Shivamogga",
            ["580"] = "Hubballi-Dharwad", ["581"] = "Uttara Kannada", ["582"] = "Gadag", ["583"] = "Ballari", ["584"] = "Raichur",
            ["585"] = "Kalaburagi", ["586"] = "Vijayapura", ["587"] = "Bagalkote", ["590"] = "Belagavi", ["591"] = "Belagavi",
            ["600"] = "Chennai", ["601"] = "Tiruvallur", ["602"] = "Kanchipuram", ["603"] = "Chengalpattu", ["604"] = "Viluppuram",
            ["605"] = "Puducherry", ["606"] = "Tiruvannamalai", ["607"] = "Cuddalore", ["608"] = "Cuddalore", ["609"] = "Mayiladuthurai",
            ["610"] = "Tiruvarur", ["611"] = "Nagapattinam", ["612"] = "Thanjavur", ["613"] = "Thanjavur", ["614"] = "Pudukkottai",
            ["620"] = "Tiruchirappalli", ["621"] = "Perambalur", ["622"] = "Pudukkottai", ["624"] = "Dindigul", ["625"] = "Madurai",
            ["626"] = "Virudhunagar", ["627"] = "Tirunelveli", ["628"] = "Thoothukudi", ["629"] = "Kanyakumari", ["630"] = "Sivaganga",
            ["631"] = "Ranipet", ["632"] = "Vellore", ["635"] = "Krishnagiri", ["636"] = "Salem", ["637"] = "Namakkal",
            ["638"] = "Erode", ["639"] = "Karur", ["641"] = "Coimbatore", ["642"] = "Tiruppur", ["643"] = "Nilgiris",
            ["670"] = "Kannur", ["671"] = "Kasaragod", ["673"] = "Kozhikode", ["676"] = "Malappuram", ["678"] = "Palakkad",
            ["679"] = "Palakkad", ["680"] = "Thrissur", ["682"] = "Kochi", ["683"] = "Ernakulam", ["685"] = "Idukki",
            ["686"] = "Kottayam", ["688"] = "Alappuzha", ["689"] = "Pathanamthitta", ["690"] = "Alappuzha", ["691"] = "Kollam",
            ["695"] = "Thiruvananthapuram", ["700"] = "Kolkata", ["711"] = "Howrah", ["712"] = "Hooghly", ["713"] = "Asansol",
            ["721"] = "Midnapore", ["722"] = "Bankura", ["723"] = "Purulia", ["731"] = "Birbhum", ["732"] = "Malda",
            ["733"] = "Uttar Dinajpur", ["734"] = "Siliguri", ["735"] = "Jalpaiguri", ["736"] = "Cooch Behar", ["741"] = "Nadia",
            ["742"] = "Murshidabad", ["743"] = "North 24 Parganas", ["744"] = "Port Blair", ["751"] = "Bhubaneswar", ["752"] = "Puri",
            ["753"] = "Cuttack", ["754"] = "Paradip", ["755"] = "Jajpur", ["756"] = "Baleswar", ["757"] = "Mayurbhanj",
            ["758"] = "Kendujhar", ["759"] = "Dhenkanal", ["760"] = "Ganjam", ["761"] = "Ganjam", ["764"] = "Koraput",
            ["765"] = "Rayagada", ["766"] = "Kalahandi", ["767"] = "Balangir", ["768"] = "Sambalpur", ["769"] = "Rourkela",
            ["770"] = "Sundargarh", ["781"] = "Guwahati", ["782"] = "Nagaon", ["783"] = "Goalpara", ["784"] = "Tezpur",
            ["785"] = "Jorhat", ["786"] = "Dibrugarh", ["787"] = "Lakhimpur", ["788"] = "Silchar", ["790"] = "Tawang",
            ["791"] = "Itanagar", ["792"] = "Pasighat", ["793"] = "Shillong", ["795"] = "Imphal", ["796"] = "Aizawl",
            ["797"] = "Kohima", ["798"] = "Dimapur", ["799"] = "Agartala", ["800"] = "Patna", ["801"] = "Patna",
            ["802"] = "Bhojpur", ["803"] = "Nalanda", ["804"] = "Jehanabad", ["805"] = "Nawada", ["811"] = "Munger",
            ["812"] = "Bhagalpur", ["813"] = "Banka", ["814"] = "Deoghar", ["815"] = "Giridih", ["816"] = "Dumka",
            ["821"] = "Rohtas", ["823"] = "Gaya", ["824"] = "Aurangabad", ["825"] = "Hazaribagh", ["826"] = "Dhanbad",
            ["827"] = "Bokaro", ["828"] = "Dhanbad", ["829"] = "Ramgarh", ["831"] = "Jamshedpur", ["832"] = "Jamshedpur",
            ["833"] = "Chaibasa", ["834"] = "Ranchi", ["835"] = "Ranchi", ["841"] = "Saran", ["842"] = "Muzaffarpur",
            ["843"] = "Sitamarhi", ["844"] = "Vaishali", ["845"] = "East Champaran", ["846"] = "Darbhanga", ["847"] = "Madhubani",
            ["848"] = "Samastipur", ["851"] = "Begusarai", ["852"] = "Saharsa", ["853"] = "Khagaria", ["854"] = "Purnia",
            ["855"] = "Kishanganj"
        };

        private static readonly string[] KnownCities = new[]
        {
            "Dr. B.R. Ambedkar Konaseema", "Chhatrapati Sambhajinagar", "South Salmara-Mankachar", "Jayashankar Bhupalpally",
            "Gaurela Pendra Marwahi", "South West Khasi Hills", "Kumuram Bheem Asifabad", "Alluri Sitharama Raju",
            "South West Garo Hills", "Parvathipuram Manyam", "Bhadradri Kothagudem", "Lower Dibang Valley",
            "Upper Dibang Valley", "Kamrup Metropolitan", "Sarangarh Bilaigarh", "Seraikela Kharsawan",
            "Yadadri Bhuvanagiri", "Gautam Buddha Nagar", "Andaman and Nicobar", "North 24 Paraganas",
            "South 24 Paraganas", "Thiruvananthapuram", "West Karbi Anglong", "East Jaintia Hills",
            "West Jaintia Hills", "Medchal Malkajgiri", "North 24 Parganas", "South 24 Parganas",
            "Udham Singh Nagar", "Paschim Bardhaman", "Paschim Medinipur", "Andaman & Nicobar",
            "Pimpri Chinchwad", "Hubballi-Dharwad", "Dakshina Kannada", "Lahaul and Spiti",
            "East Khasi Hills", "North Garo Hills", "South Garo Hills", "West Khasi Hills",
            "Didwana Kuchaman", "Khairthal Tijara", "Jogulamba Gadwal", "Rajanna Sircilla",
            "Sant Kabir Nagar", "Dakshin Dinajpur", "North East Delhi", "North West Delhi",
            "South East Delhi", "South West Delhi", "Bengaluru Urban", "Bengaluru Rural",
            "Lower Subansiri", "Upper Subansiri", "Devbhumi Dwarka", "Chamarajanagara",
            "Chikkaballapura", "East Garo Hills", "West Garo Hills", "Fatehgarh Sahib",
            "Kotputli Behror", "Tiruchirappalli", "Purba Bardhaman", "Purba Medinipur",
            "Sri Ganganagar", "Uttara Kannada", "Sri Sathya Sai", "East Champaran",
            "West Champaran", "Janjgir Champa", "Chhota Udaipur", "East Singhbhum",
            "West Singhbhum", "Chikkamagaluru", "Pathanamthitta", "Sawai Madhopur",
            "Mayiladuthurai", "Ramanathapuram", "Tiruvannamalai", "Ambedkar Nagar",
            "Siddharthnagar", "Uttar Dinajpur", "Greater Noida", "Visakhapatnam",
            "Muzaffarnagar", "East Godavari", "West Godavari", "Pakke Kessang",
            "Karbi Anglong", "Surendranagar", "Charkhi Dadri", "Churachandpur",
            "Jagatsinghpur", "Gangapur City", "Neem Ka Thana", "North Tripura",
            "South Tripura", "Pauri Garhwal", "Tehri Garhwal", "Central Delhi",
            "Chengalpattu", "Secunderabad", "Shahjahanpur", "Vizianagaram",
            "Kurung Kumey", "Baloda Bazar", "Manendragarh", "Mohla Manpur",
            "Mahendragarh", "Vijayanagara", "Narmadapuram", "Chumoukedima",
            "Kallakurichi", "Nagapattinam", "Virudhunagar", "Nagarkurnool",
            "West Tripura", "Kanpur Dehat", "Kanpur Nagar", "Navi Mumbai",
            "Ranga Reddy", "Pondicherry", "Bhubaneswar", "Tirunelveli",
            "Rajahmundry", "Muzaffarpur", "Thoothukudi", "Bulandshahr",
            "Yamunanagar", "Gandhinagar", "Lower Siang", "Upper Siang",
            "West Kameng", "Rajnandgaon", "Banaskantha", "Gir Somnath",
            "Sabarkantha", "Kurukshetra", "Dharamshala", "Chitradurga",
            "Hoshangabad", "Narsinghpur", "Ahilyanagar", "Imphal East",
            "Imphal West", "Nabarangpur", "Chittorgarh", "Hanumangarh",
            "Kanchipuram", "Kanyakumari", "Krishnagiri", "Pudukkottai",
            "Mahabubabad", "Mahbubnagar", "Farrukhabad", "Maharajganj",
            "Pithoragarh", "Rudraprayag", "Cooch Behar", "Murshidabad",
            "North Delhi", "South Delhi", "Lakshadweep", "Rangareddy",
            "Rangareddi", "Puducherry", "Aurangabad", "Coimbatore",
            "Vijayawada", "Chandigarh", "Trivandrum", "Saharanpur",
            "Jamshedpur", "Ahmednagar", "Vijayapura", "Shivamogga",
            "Chandrapur", "Karimnagar", "Port Blair", "Sindhudurg",
            "Anakapalli", "Srikakulam", "Papum Pare", "West Siang",
            "Bongaigaon", "Dima Hasao", "Hailakandi", "Kishanganj",
            "Lakhisarai", "Samastipur", "Sheikhpura", "Khairagarh",
            "Mahasamund", "Narayanpur", "Panchmahal", "Hazaribagh",
            "Davanagere", "Kalaburagi", "Ramanagara", "Malappuram",
            "Agar Malwa", "Ashoknagar", "Chhatarpur", "Chhindwara",
            "Gadchiroli", "Tamenglong", "Tengnoupal", "Mokokchung",
            "Jharsuguda", "Kendrapara", "Malkangiri", "Mayurbhanj",
            "Subarnapur", "Sundargarh", "Hoshiarpur", "Kapurthala",
            "Malerkotla", "Nawanshahr", "Tarn Taran", "Ganganagar",
            "Pratapgarh", "Dharmapuri", "Perambalur", "Tirupathur",
            "Tiruvallur", "Viluppuram", "Mancherial", "Narayanpet",
            "Peddapalli", "Sangareddy", "Wanaparthy", "Hanamkonda",
            "Sepahijala", "Chitrakoot", "Kushinagar", "Uttarkashi",
            "Alipurduar", "Darjeeling", "Jalpaiguri", "East Delhi",
            "West Delhi", "New Delhi", "Ahmedabad", "Bengaluru",
            "Bangalore", "Hyderabad", "Ghaziabad", "Faridabad",
            "Prayagraj", "Allahabad", "Moradabad", "Jalandhar",
            "Firozabad", "Bhavnagar", "Bardhaman", "Mangaluru",
            "Mangalore", "Kozhikode", "Bhagalpur", "Nizamabad",
            "Darbhanga", "Anantapur", "Bharatpur", "Begusarai",
            "Tuticorin", "Nagercoil", "Thanjavur", "Kharagpur",
            "Dibrugarh", "Kavaratti", "Jagdalpur", "Cuddalore",
            "Ratnagiri", "Annamayya", "Konaseema", "Changlang",
            "Kra Daadi", "Biswanath", "Charaideo", "Karimganj",
            "Kokrajhar", "Lakhimpur", "Sivasagar", "Gopalganj",
            "Jehanabad", "Madhepura", "Madhubani", "Sitamarhi",
            "Balrampur", "Dantewada", "Gariaband", "Kabirdham",
            "Kondagaon", "Ambikapur", "North Goa", "South Goa",
            "Mahisagar", "Porbandar", "Fatehabad", "Panchkula",
            "Bandipora", "Baramulla", "Ganderbal", "Lohardaga",
            "Sahebganj", "Bagalkote", "Alappuzha", "Ernakulam",
            "Kasaragod", "Alirajpur", "Burhanpur", "Singrauli",
            "Tikamgarh", "Pandhurna", "Nandurbar", "Osmanabad",
            "Dharashiv", "Bishnupur", "Kangpokpi", "Hnahthial",
            "Lawngtlai", "Zunheboto", "Dhenkanal", "Berhampur",
            "Kalahandi", "Kandhamal", "Kendujhar", "Sambalpur",
            "Ferozepur", "Gurdaspur", "SAS Nagar", "Pathankot",
            "SBS Nagar", "Dungarpur", "Jaisalmer", "Jhunjhunu",
            "Rajsamand", "Gyalshing", "Sivaganga", "Tiruvarur",
            "Kamareddy", "Vikarabad", "Barabanki", "Chandauli",
            "Kaushambi", "Raebareli", "Shravasti", "Sonbhadra",
            "Sultanpur", "Bageshwar", "Champawat", "Rishikesh",
            "Kalimpong", "Calcutta", "Vadodara", "Ludhiana",
            "Varanasi", "Srinagar", "Amritsar", "Jabalpur",
            "Guwahati", "Bareilly", "Gurugram", "Warangal",
            "Amravati", "Dehradun", "Durgapur", "Rourkela",
            "Kolhapur", "Jamnagar", "Siliguri", "Belagavi",
            "Malegaon", "Agartala", "Bhilwara", "Bilaspur",
            "Junagadh", "Thrissur", "Kakinada", "Bathinda",
            "Mirzapur", "Haridwar", "Dindigul", "Chittoor",
            "Ishapore", "Shillong", "Itanagar", "Silvassa",
            "Udhampur", "Pasighat", "Tinsukia", "Prakasam",
            "Tirupati", "Leparada", "Longding", "Shi Yomi",
            "Goalpara", "Golaghat", "Morigaon", "Sonitpur",
            "Udalguri", "Khagaria", "Vaishali", "Bemetara",
            "Dhamtari", "Surajpur", "Aravalli", "Palanpur",
            "Hamirpur", "Anantnag", "Kishtwar", "Gulbarga",
            "Madikeri", "Tumakuru", "Alleppey", "Kottayam",
            "Palakkad", "Balaghat", "Khargone", "Mandsaur",
            "Shajapur", "Shivpuri", "Bhandara", "Buldhana",
            "Parbhani", "Yavatmal", "Kakching", "Pherzawl",
            "Senapati", "Champhai", "Khawzawl", "Serchhip",
            "Longleng", "Shamator", "Tseminyu", "Tuensang",
            "Balangir", "Balasore", "Baleswar", "Gajapati",
            "Keonjhar", "Baripada", "Nayagarh", "Rayagada",
            "Faridkot", "Rupnagar", "Anupgarh", "Banswara",
            "Jhalawar", "Salumbar", "Sanchore", "Shahpura",
            "Ariyalur", "Namakkal", "Nilgiris", "Tiruppur",
            "Adilabad", "Nalgonda", "Siddipet", "Suryapet",
            "Azamgarh", "Bahraich", "Fatehpur", "Ghazipur",
            "Lalitpur", "Mainpuri", "Pilibhit", "Nainital",
            "Haldwani", "Jhargram", "Shahdara", "Karaikal",
            "Kolkata", "Chennai", "Lucknow", "Dhanbad",
            "Gwalior", "Jodhpur", "Madurai", "Solapur",
            "Gurgaon", "Aligarh", "Bikaner", "Cuttack",
            "Asansol", "Burdwan", "Belgaum", "Jalgaon",
            "Udaipur", "Calicut", "Kurnool", "Bellary",
            "Ballari", "Patiala", "Mathura", "Bijapur",
            "Shimoga", "Trichur", "Panipat", "Sonipat",
            "Raichur", "Katihar", "Sambhal", "Deoghar",
            "Khandwa", "Bhiwani", "Navsari", "Silchar",
            "Dimapur", "Gangtok", "Paradip", "Palghar",
            "Bapatla", "Nandyal", "Palnadu", "Barpeta",
            "Chirang", "Darrang", "Dhemaji", "Nalbari",
            "Bhojpur", "Nalanda", "Saharsa", "Sheohar",
            "Jashpur", "Mungeli", "Raigarh", "Surguja",
            "Bharuch", "Veraval", "Mehsana", "Narmada",
            "Jhajjar", "Kaithal", "Kinnaur", "Sirmaur",
            "Kupwara", "Pulwama", "Rajouri", "Shopian",
            "Giridih", "Jamtara", "Koderma", "Latehar",
            "Ramgarh", "Simdega", "Dharwad", "Wayanad",
            "Anuppur", "Barwani", "Dindori", "Neemuch",
            "Rajgarh", "Shahdol", "Sheopur", "Vidisha",
            "Mauganj", "Hingoli", "Chandel", "Jiribam",
            "Kamjong", "Ri Bhoi", "Kolasib", "Lunglei",
            "Saitual", "Kiphire", "Niuland", "Bargarh",
            "Bhadrak", "Deogarh", "Khordha", "Koraput",
            "Nuapada", "Sonepur", "Barnala", "Fazilka",
            "Muktsar", "Sangrur", "Balotra", "Dholpur",
            "Karauli", "Phalodi", "Pakyong", "Ranipet",
            "Tenkasi", "Vellore", "Jagtial", "Jangaon",
            "Khammam", "Unakoti", "Auraiya", "Ayodhya",
            "Baghpat", "Bhadohi", "Hathras", "Jaunpur",
            "Kannauj", "Kasganj", "Sitapur", "Chamoli",
            "Roorkee", "Bankura", "Birbhum", "Hooghly",
            "Purulia", "Andaman", "Nicobar", "Madras",
            "Mumbai", "Bombay", "Jaipur", "Kanpur",
            "Nagpur", "Indore", "Bhopal", "Nashik",
            "Meerut", "Rajkot", "Ranchi", "Howrah",
            "Raipur", "Mysuru", "Mysore", "Guntur",
            "Bhilai", "Cochin", "Nanded", "Ujjain",
            "Jhansi", "Bokaro", "Rohtak", "Kollam",
            "Satara", "Rampur", "Aizawl", "Karnal",
            "Imphal", "Ratlam", "Etawah", "Ongole",
            "Haldia", "Morena", "Jorhat", "Tezpur",
            "Kohima", "Kargil", "Ankola", "Mohali",
            "Panvel", "Sangli", "Kadapa", "Namsai",
            "Tawang", "Cachar", "Dhubri", "Kamrup",
            "Majuli", "Nagaon", "Araria", "Kaimur",
            "Munger", "Nawada", "Purnia", "Rohtas",
            "Supaul", "Bastar", "Kanker", "Koriya",
            "Panaji", "Margao", "Mapusa", "Amreli",
            "Valsad", "Ambala", "Palwal", "Rewari",
            "Chamba", "Kangra", "Shimla", "Budgam",
            "Kathua", "Kulgam", "Poonch", "Ramban",
            "Chatra", "Garhwa", "Khunti", "Palamu",
            "Hassan", "Haveri", "Kodagu", "Koppal",
            "Mandya", "Tumkur", "Karwar", "Yadgir",
            "Idukki", "Kannur", "Jhabua", "Mandla",
            "Niwari", "Raisen", "Sehore", "Umaria",
            "Maihar", "Gondia", "Wardha", "Washim",
            "Ukhrul", "Noklak", "Ganjam", "Jajpur",
            "Barmer", "Beawar", "Jalore", "Nagaur",
            "Sirohi", "Mangan", "Namchi", "Soreng",
            "Trichy", "Mulugu", "Nirmal", "Dhalai",
            "Gomati", "Khowai", "Amethi", "Amroha",
            "Ballia", "Bijnor", "Budaun", "Deoria",
            "Hardoi", "Jalaun", "Mahoba", "Shamli",
            "Almora", "Poona", "Thane", "Patna",
            "Salem", "Noida", "Kochi", "Ajmer",
            "Akola", "Jammu", "Erode", "Latur",
            "Dhule", "Korba", "Alwar", "Dewas",
            "Jalna", "Sagar", "Hapur", "Arrah",
            "Sikar", "Malda", "Anand", "Bhind",
            "Daman", "Eluru", "Anjaw", "Kamle",
            "Lohit", "Siang", "Tirap", "Baksa",
            "Hojai", "Arwal", "Banka", "Buxar",
            "Jamui", "Saran", "Siwan", "Balod",
            "Sakti", "Sukma", "Ponda", "Botad",
            "Dahod", "Kheda", "Kutch", "Morbi",
            "Patan", "Vyara", "Mewat", "Sirsa",
            "Kullu", "Mandi", "Solan", "Reasi",
            "Samba", "Dumka", "Godda", "Gumla",
            "Pakur", "Bidar", "Gadag", "Kolar",
            "Udupi", "Betul", "Damoh", "Datia",
            "Harda", "Katni", "Panna", "Satna",
            "Seoni", "Sidhi", "Noney", "Mamit",
            "Saiha", "Peren", "Wokha", "Angul",
            "Boudh", "Mansa", "Ropar", "Baran",
            "Bundi", "Churu", "Dausa", "Kekri",
            "Karur", "Theni", "Medak", "Banda",
            "Basti", "Gonda", "Kheri", "Unnao",
            "Nadia", "Yanam", "Pune", "Agra",
            "Kota", "Gaya", "Durg", "Rewa",
            "Pali", "Puri", "Dang", "Bhuj",
            "Tapi", "Jind", "Doda", "Dhar",
            "Guna", "Beed", "Phek", "Moga",
            "Deeg", "Dudu", "Tonk", "Ooty",
            "Etah", "Orai", "Mahe", "Diu",
            "Leh", "NTR", "Nuh", "Una",
            "Mon", "Mau"
        };

        public static string? ResolveCity(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            // 0. Special check: Any Delhi district -> New Delhi
            if (Regex.IsMatch(text, @"\bdelhi\b", RegexOptions.IgnoreCase))
                return "New Delhi";

            // 1. Check for masked Defence addresses like ***********Uttara Kannada
            if (text.Contains('*'))
            {
                var unmasked = Regex.Replace(text, @"[\*]+", " ").Trim();
                foreach (var city in KnownCities)
                {
                    if (Regex.IsMatch(unmasked, $@"\b{Regex.Escape(city)}\b", RegexOptions.IgnoreCase))
                        return NormalizeCityName(city);
                }
                if (unmasked.Length >= 3 && unmasked.Length <= 30 && Regex.IsMatch(unmasked, @"^[A-Za-z ]+$"))
                    return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(unmasked.ToLower());
            }

            // 2. 6-digit Indian PIN Code extraction
            var pinMatch = Regex.Match(text, @"\b([1-9][0-9]{5})\b");
            if (pinMatch.Success)
            {
                var prefix = pinMatch.Groups[1].Value.Substring(0, 3);
                if (PinPrefixMap.TryGetValue(prefix, out var mappedCity))
                    return mappedCity;
            }

            // 3. Known cities pattern matching
            foreach (var city in KnownCities)
            {
                if (Regex.IsMatch(text, $@"\b{Regex.Escape(city)}\b", RegexOptions.IgnoreCase))
                    return NormalizeCityName(city);
            }

            return null;
        }

        private static string NormalizeCityName(string city)
        {
            if (city.Equals("Raanchi", StringComparison.OrdinalIgnoreCase)) return "Ranchi";
            if (city.Equals("Rangareddi", StringComparison.OrdinalIgnoreCase) || city.Equals("Ranga Reddy", StringComparison.OrdinalIgnoreCase)) return "Rangareddy";
            if (city.Equals("Burdwan", StringComparison.OrdinalIgnoreCase)) return "Bardhaman";
            if (city.Equals("Bangalore", StringComparison.OrdinalIgnoreCase)) return "Bengaluru";
            if (city.Equals("Bombay", StringComparison.OrdinalIgnoreCase)) return "Mumbai";
            if (city.Equals("Calcutta", StringComparison.OrdinalIgnoreCase)) return "Kolkata";
            if (city.Equals("Madras", StringComparison.OrdinalIgnoreCase)) return "Chennai";
            if (city.Equals("Gurgaon", StringComparison.OrdinalIgnoreCase)) return "Gurugram";
            if (city.Equals("Bellary", StringComparison.OrdinalIgnoreCase)) return "Ballari";
            if (city.Equals("Belgaum", StringComparison.OrdinalIgnoreCase)) return "Belagavi";
            if (city.Equals("Mysore", StringComparison.OrdinalIgnoreCase)) return "Mysuru";
            if (city.Equals("Poona", StringComparison.OrdinalIgnoreCase)) return "Pune";
            if (city.Equals("Trivandrum", StringComparison.OrdinalIgnoreCase)) return "Thiruvananthapuram";
            if (city.Equals("Cochin", StringComparison.OrdinalIgnoreCase)) return "Kochi";
            if (city.Equals("Calicut", StringComparison.OrdinalIgnoreCase)) return "Kozhikode";
            if (city.IndexOf("Andaman", StringComparison.OrdinalIgnoreCase) >= 0 || city.IndexOf("Nicobar", StringComparison.OrdinalIgnoreCase) >= 0) return "Port Blair";
            if (city.Equals("Lakshadweep", StringComparison.OrdinalIgnoreCase)) return "Kavaratti";
            return city;
        }

        private static string? FindMaterial(
    Dictionary<string, string> json)
        {
            foreach (var item in json)
            {
                if (ContainsKeyword(
                        item.Key,
                        "Material",
                        "Material Type",
                        "Base Material"))
                {
                    return item.Value;
                }
            }

            return null;
        }

        private static string? FindSurface(
    Dictionary<string, string> json)
        {
            foreach (var item in json)
            {
                if (ContainsKeyword(
                        item.Key,
                        "Surface",
                        "Surface Finish",
                        "Finish",
                        "Coating"))
                {
                    return item.Value;
                }
            }

            return null;
        }

        private static string? FindLayers(
    Dictionary<string, string> json)
        {
            foreach (var item in json)
            {
                if (ContainsKeyword(
                        item.Key,
                        "Layer",
                        "Layers"))
                {
                    return item.Value;
                }
            }

            return null;
        }
        private static string? FindWarranty(
    Dictionary<string, string> json)
        {
            foreach (var item in json)
            {
                if (ContainsKeyword(
                        item.Key,
                        "Warranty",
                        "Warranty Period",
                        "Warranty Text"))
                {
                    return item.Value;
                }
            }

            return null;
        }
        private static string? FindServiceRequirement(
    Dictionary<string, string> json)
        {
            foreach (var item in json)
            {
                if (ContainsKeyword(
                        item.Key,
                        "Service Requirement",
                        "Requirement",
                        "Scope of Work"))
                {
                    return item.Value;
                }
            }

            return null;
        }
        private static string? FindServiceInclusions(
    Dictionary<string, string> json)
        {
            foreach (var item in json)
            {
                if (ContainsKeyword(
                        item.Key,
                        "Service Inclusion",
                        "Included Services"))
                {
                    return item.Value;
                }
            }

            return null;
        }
        private static string? FindTraining(
    Dictionary<string, string> json)
        {
            foreach (var item in json)
            {
                if (ContainsKeyword(
                        item.Key,
                        "Training",
                        "Training Module"))
                {
                    return item.Value;
                }
            }

            return null;
        }

        private static object? ConvertValue(string? value, Type type)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            Type target = Nullable.GetUnderlyingType(type) ?? type;

            try
            {
                if (target == typeof(string))
                    return value;

                if (target == typeof(int))
                {
                    value = DigitsOnlyRegex.Replace(value, "");

                    if (string.IsNullOrEmpty(value))
                        return null;

                    // Guard against int overflow too — a long garbage
                    // digit string (e.g. 24 digits) will fail int.TryParse
                    // outright, which is fine, but keep this explicit so
                    // future column-type changes don't silently regress.
                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i))
                        return i;

                    return null;
                }

                if (target == typeof(decimal))
                {
                    value = value.Replace(",", "");

                    value = DecimalOnlyRegex.Replace(value, "");

                    if (string.IsNullOrEmpty(value) || value == ".")
                        return null;

                    if (decimal.TryParse(
                        value,
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out decimal d))
                    {
                        // decimal.TryParse accepts values with far more
                        // precision than a decimal(18,2) SQL column can
                        // hold. Reject anything out of range instead of
                        // letting SqlClient throw at insert time and
                        // potentially roll back an entire batch of good
                        // rows along with it.
                        if (d > MaxSafeDecimal18_2 || d < MinSafeDecimal18_2)
                            return null;

                        return d;
                    }

                    return null;
                }

                if (target == typeof(bool))
                {
                    value = value.ToLowerInvariant();

                    if (value.Contains("not required"))
                        return false;

                    if (value.Contains("required"))
                        return true;

                    if (value.Contains("yes"))
                        return true;

                    if (value.Contains("no"))
                        return false;

                    if (value.Contains("complete"))
                        return true;

                    if (value.Contains("available"))
                        return true;

                    if (value.Contains("enabled"))
                        return true;

                    if (value.Contains("disabled"))
                        return false;

                    if (bool.TryParse(value, out bool b))
                        return b;

                    return null;
                }

                if (target == typeof(DateTime))
                {
                    string[] formats =
                    {
        "dd-MM-yyyy HH:mm:ss",
        "dd-MM-yyyy HH:mm",
        "dd-MM-yyyy",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-dd"
    };

                    if (DateTime.TryParseExact(
                            value,
                            formats,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateTime dt))
                    {
                        return dt;
                    }

                    if (DateTime.TryParse(value, out dt))
                        return dt;

                    return null;
                }
            }
            catch
            {
            }

            return value;
        }
    }
}