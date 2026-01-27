using CrisilPreOnboardingApi.Contracts;
using FluentValidation;

namespace CrisilPreOnboardingApi.Validation;

public sealed class PreOnboardingRequestValidator : AbstractValidator<PreOnboardingRequest>
{
    private static readonly HashSet<string> AllowedJoiningStatus = new(StringComparer.OrdinalIgnoreCase)
    {
        "OnboardingInitiated", "Joined", "Cancelled", "OnHold"
    };

    private static readonly HashSet<string> AllowedGender = new(StringComparer.OrdinalIgnoreCase)
    {
        "Male", "Female", "Other", "Do not prefer to disclose"
    };

    private static readonly HashSet<string> AllowedWorkLocationType = new(StringComparer.OrdinalIgnoreCase)
    {
        "Onsite", "Remote", "Hybrid"
    };

    private static readonly HashSet<string> AllowedPayrollCycle = new(StringComparer.OrdinalIgnoreCase)
    {
        "Monthly", "Annually"
    };

    private static readonly HashSet<string> AllowedEmployeeType = new(StringComparer.OrdinalIgnoreCase)
    {
        "Third Party", "Permanent", "Intern"
    };

    public PreOnboardingRequestValidator()
    {
        // Mandatory fields
        RuleFor(x => x.External_Candidate_Id)
            .NotEmpty().WithErrorCode("REQUIRED").WithMessage("external_candidate_id is mandatory.")
            .MaximumLength(50).WithErrorCode("MAX_LENGTH").WithMessage("external_candidate_id max length is 50.");

        RuleFor(x => x.Crisil_Offer_Id)
            .NotEmpty().WithErrorCode("REQUIRED").WithMessage("crisil_offer_id is mandatory.")
            .MaximumLength(50).WithErrorCode("MAX_LENGTH").WithMessage("crisil_offer_id max length is 50.");

        RuleFor(x => x.Joining_Date)
            .NotNull().WithErrorCode("REQUIRED").WithMessage("joining_date is mandatory.");

        RuleFor(x => x.First_Name)
            .NotEmpty().WithErrorCode("REQUIRED").WithMessage("first_name is mandatory.")
            .MaximumLength(100).WithErrorCode("MAX_LENGTH").WithMessage("first_name max length is 100.");

        RuleFor(x => x.Last_Name)
            .NotEmpty().WithErrorCode("REQUIRED").WithMessage("last_name is mandatory.")
            .MaximumLength(100).WithErrorCode("MAX_LENGTH").WithMessage("last_name max length is 100.");

        RuleFor(x => x.Date_Of_Birth)
            .NotNull().WithErrorCode("REQUIRED").WithMessage("date_of_birth is mandatory.")
            .Must(d => d is null || d.Value < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithErrorCode("INVALID_DATE")
            .WithMessage("date_of_birth must be in the past.");

        RuleFor(x => x.Personal_Email)
            .NotEmpty().WithErrorCode("REQUIRED").WithMessage("personal_email is mandatory.")
            .EmailAddress().WithErrorCode("INVALID_EMAIL").WithMessage("personal_email is not a valid email.");

        RuleFor(x => x.Mobile_Number)
            .NotEmpty().WithErrorCode("REQUIRED").WithMessage("mobile_number is mandatory.")
            .Matches(@"^\d{8,15}$").WithErrorCode("INVALID_MOBILE").WithMessage("mobile_number must be 8-15 digits.");

        // Other fields validation
        RuleFor(x => x.Joining_Status)
            .Must(v => string.IsNullOrWhiteSpace(v) || AllowedJoiningStatus.Contains(v))
            .WithErrorCode("INVALID_ENUM")
            .WithMessage("joining_status is invalid. Allowed: OnboardingInitiated/Joined/Cancelled/OnHold.");

        RuleFor(x => x.Gender)
            .Must(v => string.IsNullOrWhiteSpace(v) || AllowedGender.Contains(v))
            .WithErrorCode("INVALID_ENUM")
            .WithMessage("gender is invalid. Allowed: Male/Female/Other/Do not prefer to disclose.");

        RuleFor(x => x.Nationality)
            .MaximumLength(50).WithErrorCode("MAX_LENGTH").WithMessage("nationality max length is 50.")
            .When(x => !string.IsNullOrWhiteSpace(x.Nationality));

        RuleFor(x => x.Mobile_Country_Code)
            .Matches(@"^\+\d{1,4}$").WithErrorCode("INVALID_COUNTRY_CODE").WithMessage("mobile_country_code must be like +91.")
            .When(x => !string.IsNullOrWhiteSpace(x.Mobile_Country_Code));

        When(x => x.Address is not null, () =>
        {
            RuleFor(x => x.Address!.Line1)
                .NotEmpty().WithErrorCode("REQUIRED").WithMessage("address.line1 is required when address is provided.")
                .MaximumLength(200).WithErrorCode("MAX_LENGTH").WithMessage("address.line1 max length is 200.");

            RuleFor(x => x.Address!.City)
                .NotEmpty().WithErrorCode("REQUIRED").WithMessage("address.city is required when address is provided.")
                .MaximumLength(80).WithErrorCode("MAX_LENGTH").WithMessage("address.city max length is 80.");

            RuleFor(x => x.Address!.Postal_Code)
                .Matches(@"^\d{4,10}$").WithErrorCode("INVALID_POSTAL").WithMessage("address.postal_code must be 4-10 digits.")
                .When(x => !string.IsNullOrWhiteSpace(x.Address!.Postal_Code));
        });

        When(x => x.Job is not null, () =>
        {
            RuleFor(x => x.Job!.Manager_Email)
                .EmailAddress().WithErrorCode("INVALID_EMAIL").WithMessage("job.manager_email is not a valid email.")
                .When(x => !string.IsNullOrWhiteSpace(x.Job!.Manager_Email));

            RuleFor(x => x.Job!.Work_Location_Type)
                .Must(v => string.IsNullOrWhiteSpace(v) || AllowedWorkLocationType.Contains(v))
                .WithErrorCode("INVALID_ENUM")
                .WithMessage("job.work_location_type is invalid. Allowed: Onsite/Remote/Hybrid.");

            RuleFor(x => x.Job!.Employee_Type)
                .Must(v => string.IsNullOrWhiteSpace(v) || AllowedEmployeeType.Contains(v))
                .WithErrorCode("INVALID_ENUM")
                .WithMessage("job.employee_type is invalid. Allowed: Third Party/Permanent/Intern.");
        });

        When(x => x.Pay is not null, () =>
        {
            RuleFor(x => x.Pay!.Ctc_Annual_In_Inr)
                .GreaterThan(0).WithErrorCode("INVALID_RANGE").WithMessage("pay.ctc_annual_in_inr must be > 0.")
                .When(x => x.Pay!.Ctc_Annual_In_Inr is not null);

            RuleFor(x => x.Pay!.Payroll_Cycle)
                .Must(v => string.IsNullOrWhiteSpace(v) || AllowedPayrollCycle.Contains(v))
                .WithErrorCode("INVALID_ENUM")
                .WithMessage("pay.payroll_cycle is invalid. Allowed: Monthly/Weekly/BiWeekly.");
        });

        When(x => x.Kyc is not null, () =>
        {
            RuleFor(x => x.Kyc!.Pan)
                .Matches(@"^[A-Z]{5}\d{4}[A-Z]{1}$").WithErrorCode("INVALID_PAN").WithMessage("kyc.pan is invalid.")
                .When(x => !string.IsNullOrWhiteSpace(x.Kyc!.Pan));

            RuleFor(x => x.Kyc!.Aadhaar_Last4)
                .Matches(@"^\d{12}$").WithErrorCode("INVALID_AADHAAR_LAST4").WithMessage("kyc.aadhaar_last4 must be 12 digits.")
                .When(x => !string.IsNullOrWhiteSpace(x.Kyc!.Aadhaar_Last4));
        });

        When(x => x.Emergency_Contact is not null, () =>
        {
            RuleFor(x => x.Emergency_Contact!.Phone)
                .Matches(@"^\d{8,15}$").WithErrorCode("INVALID_MOBILE").WithMessage("emergency_contact.phone must be 8-15 digits.")
                .When(x => !string.IsNullOrWhiteSpace(x.Emergency_Contact!.Phone));
        });
    }
}
