using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Conflux.Database.Entities;

public class ApplicationUser : IdentityUser {
    [StringLength(64), Required]
    public required string DisplayName { get; set; }
    
    [StringLength(maximumLength: 32)]
    public string? Pronouns { get; set; }
    
    [StringLength(255)]
    public string? Bio { get; set; }
    
    public DateTime CreatedAt { get; set; }
}