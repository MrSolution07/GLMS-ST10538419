using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Web.Models;

public class ServiceRequest
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Contract")]
    public int ContractId { get; set; }

    public Contract? Contract { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Amount (USD)")]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal CostUsd { get; set; }

    [Display(Name = "Amount (ZAR)")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostZar { get; set; }

    [Display(Name = "Exchange Rate Used")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal ExchangeRateUsed { get; set; }

    [Required]
    [Display(Name = "Status")]
    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

    [Display(Name = "Raised On")]
    public DateTime RaisedOn { get; set; } = DateTime.UtcNow;
}
