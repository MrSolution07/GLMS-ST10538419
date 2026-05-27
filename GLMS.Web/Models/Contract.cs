using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Web.Models;

public class Contract
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Client")]
    public int ClientId { get; set; }

    public Client? Client { get; set; }

    [Required(ErrorMessage = "Contract title is required.")]
    [StringLength(200)]
    [Display(Name = "Contract Title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required]
    [Display(Name = "End Date")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Required]
    [Display(Name = "Status")]
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Required]
    [Display(Name = "Service Level")]
    public ServiceLevel ServiceLevel { get; set; } = ServiceLevel.Standard;

    [Display(Name = "Signed Agreement (PDF)")]
    public string? SignedAgreementPath { get; set; }

    [Display(Name = "Original File Name")]
    public string? SignedAgreementFileName { get; set; }

    [Display(Name = "Created On")]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
