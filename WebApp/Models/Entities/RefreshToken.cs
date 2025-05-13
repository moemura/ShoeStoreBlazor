using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models.Entities;

public class RefreshToken
{
    [Key]
    public string Token { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public DateTime ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }

    [ForeignKey("UserId")]
    public virtual AppUser User { get; set; } = null!;
} 