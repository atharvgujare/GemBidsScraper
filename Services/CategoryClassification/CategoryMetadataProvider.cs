namespace GemBidScraper.Services.CategoryClassification;

public static class CategoryMetadataProvider
{
    public static readonly List<CategoryMetadata> Categories = new()
    {
        new()
        {
            Key = "IT",
            Title = "IT & Electronics",
            Icon = "computer",
            Color = "#2563EB"
        },

        new()
        {
            Key = "MEDICAL",
            Title = "Medical Equipment",
            Icon = "hospital",
            Color = "#DC2626"
        },

        new()
        {
            Key = "VEHICLE",
            Title = "Vehicle & Transport",
            Icon = "truck",
            Color = "#059669"
        },

        new()
        {
            Key = "ELECTRICAL",
            Title = "Electrical",
            Icon = "zap",
            Color = "#F59E0B"
        },

        new()
        {
            Key = "MECHANICAL",
            Title = "Mechanical",
            Icon = "settings",
            Color = "#7C3AED"
        },

        new()
        {
            Key = "CONSTRUCTION",
            Title = "Construction",
            Icon = "building",
            Color = "#EA580C"
        },

        new()
        {
            Key = "OFFICE",
            Title = "Office Supplies",
            Icon = "briefcase",
            Color = "#0891B2"
        },

        new()
        {
            Key = "SECURITY",
            Title = "Security",
            Icon = "shield",
            Color = "#BE123C"
        },

        new()
        {
            Key = "SERVICES",
            Title = "Services",
            Icon = "wrench",
            Color = "#4F46E5"
        },

        new()
        {
            Key = "DEFENCE",
            Title = "Defence",
            Icon = "shield-check",
            Color = "#374151"
        },

        new()
        {
            Key = "OTHER",
            Title = "Other",
            Icon = "boxes",
            Color = "#6B7280"
        }
    };
}