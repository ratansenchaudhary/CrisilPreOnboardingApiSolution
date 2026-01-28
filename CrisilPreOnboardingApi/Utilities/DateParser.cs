using System.Globalization;

namespace CrisilPreOnboardingApi.Utilities
{
    public static class DateParser
    {
        private const string Format = "dd-MM-yyyy";

        public static DateOnly ParseRequired(string value, string fieldName)
        {
            if (!DateOnly.TryParseExact(
                    value,
                    Format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
            {
                throw new FormatException($"{fieldName} must be in dd-MM-yyyy format");
            }

            return date;
        }

        public static DateOnly? ParseOptional(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (!DateOnly.TryParseExact(
                    value,
                    Format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
            {
                throw new FormatException($"{fieldName} must be in dd-MM-yyyy format");
            }

            return date;
        }
    }
}


