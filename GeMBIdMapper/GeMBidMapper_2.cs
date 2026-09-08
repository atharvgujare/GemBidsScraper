using GemBidScraper.Attributes;
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