using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TestTaskAton.Models;

public class User : IdentityUser<Guid>
{
    public string Login { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;

    public int Gender { get; set; }

    public DateTime? Birthday { get; set; }

    public bool Admin { get; set; }

    public DateTime CreatedOn { get; set; }

    public string CreatedBy { get; set; } =  string.Empty;

    public DateTime ModifiedOn { get; set; }
    
    public string ModifiedBy { get; set; } = string.Empty;

    public DateTime? RevokedOn { get; set; }

    public string? RevokedBy { get; set; }
}