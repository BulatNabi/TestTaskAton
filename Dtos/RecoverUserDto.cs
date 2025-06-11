using System.ComponentModel.DataAnnotations;

namespace TestTaskAton.Dtos;

public class RecoverUserDto
{ 
        [Required(ErrorMessage = "Логин пользователя обязателен.")]
        public string Login { get; set; } = string.Empty;
}