using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Entities
{
    public class Size
    {
        [Key]
        [MaxLength(5)]
        public string Id { get; set; }
    }
} 