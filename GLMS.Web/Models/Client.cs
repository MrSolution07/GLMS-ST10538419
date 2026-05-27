using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models;

public class Client
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Client name is required.")]
    [StringLength(150, MinimumLength = 2)]
    [Display(Name = "Client Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact email is required.")]
    [EmailAddress]
    [Display(Name = "Contact Email")]
    public string ContactEmail { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Contact Phone")]
    public string? ContactPhone { get; set; }

    [Required(ErrorMessage = "Region is required.")]
    [StringLength(100)]
    public string Region { get; set; } = string.Empty;

    [Display(Name = "Registered On")]
    public DateTime RegisteredOn { get; set; } = DateTime.UtcNow;

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
