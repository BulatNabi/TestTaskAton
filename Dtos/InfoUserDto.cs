namespace TestTaskAton.Dtos;

public class InfoUserDto
{
    public string Name { get; set; } = string.Empty;
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public bool IsActive { get; set; } 
}