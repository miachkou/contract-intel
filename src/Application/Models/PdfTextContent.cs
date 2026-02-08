namespace Application.Models;

public class PdfTextContent
{
    public string FullText { get; set; } = string.Empty;
    public IReadOnlyList<PageText> Pages { get; set; } = Array.Empty<PageText>();
}

public class PageText
{
    public int PageNumber { get; set; }
    public string Text { get; set; } = string.Empty;
}
