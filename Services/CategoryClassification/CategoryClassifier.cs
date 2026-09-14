using System.Text.RegularExpressions;
using GemBidScraper.Services.CategoryClassification;

public class CategoryClassifier
{
    private readonly List<CategoryDefinition> _categories;

    public CategoryClassifier()
    {
        _categories = CategoryKeywordProvider.Categories;
    }

    public CategoryResult Classify(
        string? title,
        string? description)
    {
        string text = NormalizeText(
            $"{title} {description}");

        if (string.IsNullOrWhiteSpace(text))
        {
            return CreateOtherResult();
        }

        string? bestCategoryKey = null;
        string? bestSubCategoryKey = null;

        int bestScore = 0;
        int bestLongestKeywordScore = 0;
        int bestMatchedKeywordCount = 0;

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
                    {
                        continue;
                    }

                    string normalizedKeyword =
                        NormalizeText(keyword);

                    if (string.IsNullOrWhiteSpace(
                            normalizedKeyword))
                    {
                        continue;
                    }

                    if (!ContainsWholeWord(
                            text,
                            normalizedKeyword))
                    {
                        continue;
                    }

                    matchedKeywordCount++;

                    int keywordScore =
                        CalculateKeywordScore(
                            normalizedKeyword);

                    /*
                     * Give the strongest importance to the
                     * most specific keyword in this subcategory.
                     *
                     * Additional matches provide only a small
                     * supporting score, preventing generic words
                     * from overpowering a specific phrase.
                     */
                    if (keywordScore > longestKeywordScore)
                    {
                        longestKeywordScore =
                            keywordScore;
                    }

                    currentScore +=
                        keywordScore <= 30
                            ? 5
                            : keywordScore / 10;
                }

                if (matchedKeywordCount == 0)
                {
                    continue;
                }

                /*
                 * The longest/specific keyword is the main score.
                 * Other matching keywords are supporting evidence.
                 */
                currentScore +=
                    longestKeywordScore * 10;

                bool isBetterMatch =
                    currentScore > bestScore
                    ||
                    (
                        currentScore == bestScore
                        &&
                        longestKeywordScore >
                            bestLongestKeywordScore
                    )
                    ||
                    (
                        currentScore == bestScore
                        &&
                        longestKeywordScore ==
                            bestLongestKeywordScore
                        &&
                        matchedKeywordCount >
                            bestMatchedKeywordCount
                    );

                if (!isBetterMatch)
                {
                    continue;
                }

                bestScore = currentScore;
                bestLongestKeywordScore =
                    longestKeywordScore;
                bestMatchedKeywordCount =
                    matchedKeywordCount;

                bestCategoryKey = category.Key;
                bestSubCategoryKey =
                    subCategory.Key;
            }
        }

        if (string.IsNullOrWhiteSpace(
                bestCategoryKey)
            ||
            string.IsNullOrWhiteSpace(
                bestSubCategoryKey))
        {
            return CreateOtherResult();
        }

        return new CategoryResult
        {
            CategoryKey = bestCategoryKey,
            CategorySubKey = bestSubCategoryKey
        };
    }

    private static string NormalizeText(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string normalized = value
            .ToLowerInvariant()
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

    private static bool ContainsWholeWord(
        string text,
        string keyword)
    {
        if (string.IsNullOrWhiteSpace(text)
            ||
            string.IsNullOrWhiteSpace(keyword))
        {
            return false;
        }

        string pattern =
            $@"(?<!\w){Regex.Escape(keyword)}(?!\w)";

        return Regex.IsMatch(
            text,
            pattern,
            RegexOptions.IgnoreCase
            |
            RegexOptions.CultureInvariant);
    }

    private static int CalculateKeywordScore(
        string keyword)
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

    private static CategoryResult CreateOtherResult()
    {
        return new CategoryResult
        {
            CategoryKey = "OTHER",
            CategorySubKey = "OTHER"
        };
    }
}