using System;
using System.ComponentModel.DataAnnotations;

namespace SocialMedia.Application.CustomValidations;

public class EnumValueAttribute : ValidationAttribute
{

    private Type _enumType;
    private bool _isRequired;
    public EnumValueAttribute(Type enumType, bool isRequired = false)
    {
        _enumType = enumType;
        _isRequired = isRequired;
    }
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not null)
        {
            if (Enum.IsDefined(_enumType, value))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage ?? $"Invlaid {validationContext.MemberName} field value");
        }
        if (_isRequired)
        {
            return new ValidationResult(ErrorMessage ?? $"{validationContext.MemberName} field is required");
        }
        return ValidationResult.Success;
    }

}
