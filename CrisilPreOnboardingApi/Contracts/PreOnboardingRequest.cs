namespace CrisilPreOnboardingApi.Contracts;

public sealed class PreOnboardingRequest
{
    public string? External_Candidate_Id { get; set; }   // external_candidate_id
    public string? Crisil_Offer_Id { get; set; }         // crisil_offer_id
    public string? Joining_Status { get; set; }          // joining_status
    public string? Joining_Date { get; set; }          // joining_date
    public string? First_Name { get; set; }              // first_name
    public string? Last_Name { get; set; }               // last_name
    public string? Date_Of_Birth { get; set; }         // date_of_birth
    public string? Gender { get; set; }                  // gender
    public string? Nationality { get; set; }             // nationality
    public string? Personal_Email { get; set; }          // personal_email
    public string? Mobile_Country_Code { get; set; }     // mobile_country_code
    public string? Mobile_Number { get; set; }           // mobile_number


    public AddressDto? Address { get; set; }
    public JobDto? Job { get; set; }
    public PayDto? Pay { get; set; }
    public KycDto? Kyc { get; set; }
    public EmergencyContactDto? Emergency_Contact { get; set; }
}

public sealed class AddressDto
{
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Postal_Code { get; set; }
    public string? Country { get; set; }
}

public sealed class JobDto
{
    public string? Designation_Title { get; set; }
    public string? Department_Code { get; set; }
    public string? Cost_Center_Code { get; set; }
    public string? Grade { get; set; }
    public string? Location_Code { get; set; }
    public string? Work_Location_Type { get; set; }  // Onsite/Remote/Hybrid etc.
    public string? Manager_Employee_Id { get; set; }
    public string? Manager_Email { get; set; }
    public string? Employee_Type { get; set; }       // Contract/Permanent etc.
    public string? Probation_End_Date { get; set; }
}

public sealed class PayDto
{
    public decimal? Ctc_Annual_In_Inr { get; set; }
    public string? Payroll_Cycle { get; set; } // Monthly etc.
}

public sealed class KycDto
{
    public string? Pan { get; set; }
    public string? Aadhaar_Last4 { get; set; }
    public string? Uan { get; set; }
    public string? Esi_Number { get; set; }
}

public sealed class EmergencyContactDto
{
    public string? Name { get; set; }
    public string? Relationship { get; set; }
    public string? Phone { get; set; }
}
