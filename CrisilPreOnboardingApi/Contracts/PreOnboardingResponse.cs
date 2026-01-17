namespace CrisilPreOnboardingApi.Contracts;

public sealed class PreOnboardingResponse
{
    public long Id { get; set; }
    public string External_Candidate_Id { get; set; } = "";
    public string Crisil_Offer_Id { get; set; } = "";
    public string? Joining_Status { get; set; }
    public DateOnly Joining_Date { get; set; }
    public string First_Name { get; set; } = "";
    public string Last_Name { get; set; } = "";
    public DateOnly Date_Of_Birth { get; set; }
    public string? Gender { get; set; }
    public string? Nationality { get; set; }
    public string Personal_Email { get; set; } = "";
    public string? Mobile_Country_Code { get; set; }
    public string Mobile_Number { get; set; } = "";

    public AddressDto? Address { get; set; }
    public JobDto? Job { get; set; }
    public PayDto? Pay { get; set; }
    public KycDto? Kyc { get; set; }
    public EmergencyContactDto? Emergency_Contact { get; set; }

    public DateTime Created_Utc { get; set; }
    public DateTime? Updated_Utc { get; set; }
}
