namespace IncognitoPAS.DTOs;

/// <summary>
/// SECURITY-CRITICAL: This DTO is used for blind browsing by supervisors.
/// It MUST NEVER contain any student identity fields.
/// </summary>
public class BlindProposalDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public string? TechnicalStack { get; set; }
    public string? ProposalDocumentUrl { get; set; }
    public string ResearchAreaName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // ⛔ NO StudentId
    // ⛔ NO StudentName
    // ⛔ NO any identity field
}
