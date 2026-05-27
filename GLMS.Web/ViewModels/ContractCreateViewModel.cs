using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using GLMS.Web.Models;

namespace GLMS.Web.ViewModels;

public class ContractCreateViewModel
{
    [Required]
    [Display(Name = "Client")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "Contract title is required.")]
    [StringLength(200)]
    [Display(Name = "Contract Title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "End Date")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1);

    [Required]
    [Display(Name = "Service Level")]
    public ServiceLevel ServiceLevel { get; set; } = ServiceLevel.Standard;

    [Display(Name = "Signed Agreement (PDF only)")]
    public IFormFile? SignedAgreement { get; set; }

    public IEnumerable<SelectListItem> ClientOptions { get; set; } = Enumerable.Empty<SelectListItem>();
}
