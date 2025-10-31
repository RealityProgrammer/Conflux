using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SomeChattingPlatform.Database.Models;

public sealed class User {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }
    
    [MaxLength(64, ErrorMessage = "Name cannot be longer than 64 characters.")]
    public string Name { get; set; }
    
    [EmailAddress]
    [MaxLength(320, ErrorMessage = "Email cannot be longer than 320 characters.")]
    public string Email { get; set; }
    
    public string Password { get; set; }
}