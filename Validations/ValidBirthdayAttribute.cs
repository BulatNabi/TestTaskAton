using System.ComponentModel.DataAnnotations;

namespace TestTaskAton.Validations;

public class ValidBirthdayAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not DateTime birthday) return new ValidationResult("Некорректный формат даты.");
        var now = DateTime.UtcNow; 

        if (birthday > now)
            return new ValidationResult("Дата рождения не может быть в будущем.");
            

        var age = now.Year - birthday.Year;
        if (birthday > now.AddYears(-age)) 
            age--;

        if (age > 120)
            return new ValidationResult("Возраст пользователя не может превышать 120 лет.");

        return ValidationResult.Success;

    }
}