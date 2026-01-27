namespace CrisilPreOnboardingApi.Data;

public sealed class PreOnboardingEntity
{
    public long Id { get; set; }

    public string ExternalCandidateId { get; set; } = "";
    public string CrisilOfferId { get; set; } = "";
    public string? JoiningStatus { get; set; }
    public DateOnly JoiningDate { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateOnly DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Nationality { get; set; }
    public string PersonalEmail { get; set; } = "";
    public string? MobileCountryCode { get; set; }
    public string MobileNumber { get; set; } = "";

    // Store nested objects as JSON
    public string? AddressJson { get; set; }
    public string? JobJson { get; set; }
    public string? PayJson { get; set; }
    public string? KycJson { get; set; }
    public string? EmergencyContactJson { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? RawRequestJson { get; set; }
    public string Status { get; set; } = "Active"; // ✅ NEW
}
