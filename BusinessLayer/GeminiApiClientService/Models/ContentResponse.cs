namespace BusinessLayer.Models.ContentResponse;

internal sealed class ContentResponse
{
    public Candidate[]? Candidates { get; set; }
    public PromptFeedback? PromptFeedback { get; set; }
}
internal sealed class PromptFeedback
{
    public SaftetyRating[]? SafetyRating { get; set; }
}
internal sealed class Candidate
{
    public Content? Content { get; set; }
    public string? FinishReason { get; set; }
    public int Index { get; set; }
    public SaftetyRating[] SafetyRating { get; set; }
}

internal sealed class Content
{
    public Part[]? Parts { get; set; }
    public string? Role { get; set; }
}

internal sealed class Part
{
    public string? Text { get; set; }
}

internal sealed class SaftetyRating
{
    public string? Category { get; set; }
    public string? Probability { get; set; }
}