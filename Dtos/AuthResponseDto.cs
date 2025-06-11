namespace TestTaskAton.Dtos;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public InfoUserDto? UserInfo { get; set; } 
}