using System.ComponentModel.DataAnnotations;
using GLMS.Web.Models;

namespace GLMS.Web.ViewModels;

public class ContractSearchViewModel
{
    [Display(Name = "From Date")]
    [DataType(DataType.Date)]
    public DateTime? StartDateFrom { get; set; }

    [Display(Name = "To Date")]
    [DataType(DataType.Date)]
    public DateTime? StartDateTo { get; set; }

    [Display(Name = "Status")]
    public ContractStatus? Status { get; set; }

    [Display(Name = "Search Title")]
    public string? TitleKeyword { get; set; }

    public IEnumerable<Contract> Results { get; set; } = Enumerable.Empty<Contract>();
}
