using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS.Web.ViewModels;

public class ServiceRequestCreateViewModel
{
    [Required]
    [Display(Name = "Contract")]
    public int ContractId { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Amount (USD)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive.")]
    public decimal CostUsd { get; set; }

    [BindNever]
    [Display(Name = "Estimated ZAR (auto-calculated)")]
    public decimal CostZar { get; set; }

    [BindNever]
    [Display(Name = "Live USD→ZAR Rate")]
    public decimal ExchangeRate { get; set; }

    public IEnumerable<SelectListItem> ContractOptions { get; set; } = Enumerable.Empty<SelectListItem>();
}
