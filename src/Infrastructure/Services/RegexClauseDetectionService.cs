using Application.Interfaces;
using Application.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Infrastructure.Services;

public class RegexClauseDetectionService : IClauseDetectionService
{
    private readonly ILogger<RegexClauseDetectionService> _logger;
    private readonly Dictionary<string, ClausePattern> _patterns;
    private const int ExcerptLength = 200;

    public RegexClauseDetectionService(ILogger<RegexClauseDetectionService> logger)
    {
        _logger = logger;
        _patterns = InitializePatterns();
    }

    public Task<ClauseDetectionResult> DetectClausesAsync(
        string fullText,
        IReadOnlyList<PageText>? pageTexts = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fullText))
        {
            return Task.FromResult(new ClauseDetectionResult());
        }

        try
        {
            var detectedClauses = new List<DetectedClause>();

            foreach (var (clauseType, pattern) in _patterns)
            {
                var matches = pattern.Regex.Matches(fullText);

                foreach (Match match in matches)
                {
                    var excerpt = ExtractExcerpt(fullText, match.Index, match.Length);
                    var pageNumber = FindPageNumber(match.Index, excerpt, pageTexts);

                    detectedClauses.Add(new DetectedClause
                    {
                        ClauseType = clauseType,
                        Excerpt = excerpt,
                        Confidence = pattern.Confidence,
                        PageNumber = pageNumber
                    });
                }
            }

            _logger.LogInformation("Detected {Count} clauses in text", detectedClauses.Count);

            return Task.FromResult(new ClauseDetectionResult
            {
                Clauses = detectedClauses
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting clauses");
            return Task.FromResult(new ClauseDetectionResult());
        }
    }

    private Dictionary<string, ClausePattern> InitializePatterns()
    {
        var regexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline;

        return new Dictionary<string, ClausePattern>
        {
            ["renewal"] = new ClausePattern(
                new Regex(@"\b(renew|renewal|extend|extension)\s+(term|period|clause|provision|agreement|contract)\b", regexOptions),
                0.75m
            ),

            ["auto_renewal"] = new ClausePattern(
                new Regex(@"\b(auto(matic(ally)?)?[\s-]*(renew|renewal|extend)|renew\s+automatic(ally)?|automatic(ally)?\s+(renew|renewal))\b", regexOptions),
                0.80m
            ),

            ["termination"] = new ClausePattern(
                new Regex(@"\b(terminat(e|ion)|cancel(lation)?|end(ing)?)\s+(clause|provision|notice|period|rights?|agreement|contract)\b|\b(notice\s+period|termination\s+notice)\s*[:\-]?\s*(\d+\s*(days?|months?|weeks?))\b", regexOptions),
                0.75m
            ),

            ["data_protection"] = new ClausePattern(
                new Regex(@"\b(data\s+protection|privacy|GDPR|personal\s+data|confidential\s+information|data\s+security|information\s+security)\s+(clause|provision|requirements?|obligations?|act|law|regulation)\b", regexOptions),
                0.70m
            ),

            ["liability_cap"] = new ClausePattern(
                new Regex(@"\b(liabilit(y|ies)|indemnit(y|ies))\s+(cap|limit(ation)?|ceiling|maximum)\b|\b(cap|limit)\s+(on|of)\s+(liabilit(y|ies)|indemnit(y|ies))\b|\bliabilit(y|ies)\s+shall\s+(not\s+)?exceed\b", regexOptions),
                0.80m
            ),

            ["governing_law"] = new ClausePattern(
                new Regex(@"\b(govern(ing|ed)\s+(by|under)|subject\s+to|construed\s+in\s+accordance\s+with)\s+(the\s+)?(laws?|jurisdiction)\s+(of|in)\b|\b(jurisdiction|venue)\s+clause\b", regexOptions),
                0.75m
            )
        };
    }

    private string ExtractExcerpt(string text, int matchIndex, int matchLength)
    {
        var halfLength = ExcerptLength / 2;
        var start = Math.Max(0, matchIndex - halfLength);
        var end = Math.Min(text.Length, matchIndex + matchLength + halfLength);
        var excerptLength = end - start;

        var excerpt = text.Substring(start, excerptLength).Trim();

        // Add ellipsis if excerpt doesn't start/end at text boundaries
        if (start > 0)
            excerpt = "..." + excerpt;
        if (end < text.Length)
            excerpt = excerpt + "...";

        return excerpt;
    }

    private int? FindPageNumber(int matchIndex, string excerpt, IReadOnlyList<PageText>? pageTexts)
    {
        if (pageTexts == null || pageTexts.Count == 0)
            return null;

        // Try to find which page contains this excerpt
        foreach (var page in pageTexts)
        {
            if (page.Text.Contains(excerpt.Trim('.'), StringComparison.OrdinalIgnoreCase))
            {
                return page.PageNumber;
            }
        }

        return null;
    }

    private record ClausePattern(Regex Regex, decimal Confidence);
}
