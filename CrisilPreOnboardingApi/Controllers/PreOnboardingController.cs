using System.Text.Json;
using CrisilPreOnboardingApi.Contracts;
using CrisilPreOnboardingApi.Data;
using CrisilPreOnboardingApi.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrisilPreOnboardingApi.Controllers;

[ApiController]
[Route("api/v1/pre-onboarding")]
public sealed class PreOnboardingController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;
    private readonly IValidator<PreOnboardingRequest> _validator;

    public PreOnboardingController(IConfiguration config, AppDbContext db, IValidator<PreOnboardingRequest> validator)
    {
        _config = config;
        _db = db;
        _validator = validator;
    }

    // ---------------- POST ----------------
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PreOnboardingRequest request, CancellationToken ct)
    {
        var (authOk, authResult) = ValidateAuthHeaders();
        if (!authOk) return authResult!;

        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(f => new ApiFieldError
            {
                Field = ToSnakeCasePath(f.PropertyName),
                ErrorCode = string.IsNullOrWhiteSpace(f.ErrorCode) ? "INVALID" : f.ErrorCode,
                Message = f.ErrorMessage
            }).ToList();

            return BadRequest(new ApiErrorResponse
            {
                Code = "VALIDATION_FAILED",
                Message = "One or more validation errors occurred.",
                Errors = errors,
                TraceId = HttpContext.TraceIdentifier
            });
        }

        // Capture raw request JSON (audit)
        string? rawRequestJson = null;
        if (Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, leaveOpen: true);
            rawRequestJson = await reader.ReadToEndAsync();
            Request.Body.Position = 0;
            if (rawRequestJson.Length > 20000) rawRequestJson = rawRequestJson[..20000] + "...(truncated)";
        }

        var entity = new PreOnboardingEntity
        {
            ExternalCandidateId = request.External_Candidate_Id!,
            CrisilOfferId = request.Crisil_Offer_Id!,
            JoiningStatus = request.Joining_Status,
            JoiningDate = request.Joining_Date!.Value,
            FirstName = request.First_Name!,
            LastName = request.Last_Name!,
            DateOfBirth = request.Date_Of_Birth!.Value,
            Gender = request.Gender,
            Nationality = request.Nationality,
            PersonalEmail = request.Personal_Email!,
            MobileCountryCode = request.Mobile_Country_Code,
            MobileNumber = request.Mobile_Number!,
            AddressJson = request.Address is null ? null : JsonSerializer.Serialize(request.Address),
            JobJson = request.Job is null ? null : JsonSerializer.Serialize(request.Job),
            PayJson = request.Pay is null ? null : JsonSerializer.Serialize(request.Pay),
            KycJson = request.Kyc is null ? null : JsonSerializer.Serialize(request.Kyc),
            EmergencyContactJson = request.Emergency_Contact is null ? null : JsonSerializer.Serialize(request.Emergency_Contact),
            RawRequestJson = rawRequestJson,
            CreatedBy = Request.Headers[_config["Auth:CompanyCodeHeaderName"] ?? "CompanyCode"].FirstOrDefault()
        };

        try
        {
            _db.PreOnboardings.Add(entity);
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Conflict(new ApiErrorResponse
            {
                Code = "DUPLICATE_REQUEST",
                Message = "A record already exists for the given external_candidate_id and crisil_offer_id.",
                Errors = new()
                {
                    new ApiFieldError { Field = "external_candidate_id", ErrorCode = "DUPLICATE", Message = "external_candidate_id already used with this crisil_offer_id." },
                    new ApiFieldError { Field = "crisil_offer_id", ErrorCode = "DUPLICATE", Message = "crisil_offer_id already used with this external_candidate_id." }
                },
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(new
        {
            code = "SUCCESS",
            message = "Pre-onboarding request saved successfully.",
            data = new
            {
                id = entity.Id,
                external_candidate_id = entity.ExternalCandidateId,
                crisil_offer_id = entity.CrisilOfferId
            },
            traceId = HttpContext.TraceIdentifier
        });
    }

    // ---------------- GET by Id ----------------
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById([FromRoute] long id, CancellationToken ct)
    {
        var (authOk, authResult) = ValidateAuthHeaders();
        if (!authOk) return authResult!;

        var entity = await _db.PreOnboardings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return NotFound(new ApiErrorResponse
            {
                Code = "NOT_FOUND",
                Message = "Record not found.",
                Errors = new() { new ApiFieldError { Field = "id", ErrorCode = "NOT_FOUND", Message = "No record found for the given id." } },
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(new
        {
            code = "SUCCESS",
            message = "Record fetched successfully.",
            data = Map(entity),
            traceId = HttpContext.TraceIdentifier
        });
    }

    // ---------------- GET search + paging ----------------
    // /api/v1/pre-onboarding?external_candidate_id=E123&crisil_offer_id=O1&from=2026-01-01&to=2026-01-31&page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery(Name = "external_candidate_id")] string? externalCandidateId,
        [FromQuery(Name = "crisil_offer_id")] string? crisilOfferId,
        [FromQuery(Name = "from")] DateOnly? from,
        [FromQuery(Name = "to")] DateOnly? to,
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "pageSize")] int pageSize = 20,
        CancellationToken ct = default)
    {
        var (authOk, authResult) = ValidateAuthHeaders();
        if (!authOk) return authResult!;

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 200) pageSize = 200;

        var q = _db.PreOnboardings.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(externalCandidateId))
            q = q.Where(x => x.ExternalCandidateId == externalCandidateId);

        if (!string.IsNullOrWhiteSpace(crisilOfferId))
            q = q.Where(x => x.CrisilOfferId == crisilOfferId);

        if (from is not null)
            q = q.Where(x => x.JoiningDate >= from.Value);

        if (to is not null)
            q = q.Where(x => x.JoiningDate <= to.Value);

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Ok(new
        {
            code = "SUCCESS",
            message = "Search completed successfully.",
            data = new
            {
                page,
                pageSize,
                total,
                items = items.Select(Map)
            },
            traceId = HttpContext.TraceIdentifier
        });
    }

    private (bool ok, IActionResult? result) ValidateAuthHeaders()
    {
        var tokenHeaderName = _config["Auth:TokenHeaderName"] ?? "Token";
        var companyHeaderName = _config["Auth:CompanyCodeHeaderName"] ?? "CompanyCode";
        var expectedToken = _config["Auth:ExpectedToken"];
        var expectedCompany = _config["Auth:ExpectedCompanyCode"];

        var token = Request.Headers[tokenHeaderName].FirstOrDefault();
        var company = Request.Headers[companyHeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(company))
        {
            return (false, Unauthorized(new ApiErrorResponse
            {
                Code = "UNAUTHORIZED",
                Message = "Missing authentication headers.",
                Errors = new()
                {
                    new ApiFieldError { Field = tokenHeaderName, ErrorCode = "REQUIRED", Message = $"{tokenHeaderName} header is required." },
                    new ApiFieldError { Field = companyHeaderName, ErrorCode = "REQUIRED", Message = $"{companyHeaderName} header is required." }
                },
                TraceId = HttpContext.TraceIdentifier
            }));
        }

        if (!string.Equals(token, expectedToken, StringComparison.Ordinal) ||
            !string.Equals(company, expectedCompany, StringComparison.OrdinalIgnoreCase))
        {
            return (false, Unauthorized(new ApiErrorResponse
            {
                Code = "UNAUTHORIZED",
                Message = "Invalid Token or CompanyCode.",
                Errors = new()
                {
                    new ApiFieldError { Field = tokenHeaderName, ErrorCode = "INVALID", Message = "Invalid Token." },
                    new ApiFieldError { Field = companyHeaderName, ErrorCode = "INVALID", Message = "Invalid CompanyCode." }
                },
                TraceId = HttpContext.TraceIdentifier
            }));
        }

        return (true, null);
    }

    private static PreOnboardingResponse Map(PreOnboardingEntity entity)
    {
        static T? Deserialize<T>(string? json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try { return JsonSerializer.Deserialize<T>(json); }
            catch { return null; }
        }

        return new PreOnboardingResponse
        {
            Id = entity.Id,
            External_Candidate_Id = entity.ExternalCandidateId,
            Crisil_Offer_Id = entity.CrisilOfferId,
            Joining_Status = entity.JoiningStatus,
            Joining_Date = entity.JoiningDate,
            First_Name = entity.FirstName,
            Last_Name = entity.LastName,
            Date_Of_Birth = entity.DateOfBirth,
            Gender = entity.Gender,
            Nationality = entity.Nationality,
            Personal_Email = entity.PersonalEmail,
            Mobile_Country_Code = entity.MobileCountryCode,
            Mobile_Number = entity.MobileNumber,
            Address = Deserialize<AddressDto>(entity.AddressJson),
            Job = Deserialize<JobDto>(entity.JobJson),
            Pay = Deserialize<PayDto>(entity.PayJson),
            Kyc = Deserialize<KycDto>(entity.KycJson),
            Emergency_Contact = Deserialize<EmergencyContactDto>(entity.EmergencyContactJson),
            Created_Utc = entity.CreatedUtc,
            Updated_Utc = entity.UpdatedUtc
        };
    }

    private static string ToSnakeCasePath(string dotPath)
    {
        var parts = dotPath.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(".", parts.Select(ToSnakeCase));
    }

    private static string ToSnakeCase(string input)
    {
        if (input.Contains('_')) return input.ToLowerInvariant();
        var chars = new List<char>(input.Length + 10);
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c) && i > 0) chars.Add('_');
            chars.Add(char.ToLowerInvariant(c));
        }
        return new string(chars.ToArray());
    }
}
