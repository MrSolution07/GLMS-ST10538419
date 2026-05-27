using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Models;

namespace GLMS.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalClients         = await _context.Clients.CountAsync();
        ViewBag.TotalContracts       = await _context.Contracts.CountAsync();
        ViewBag.ActiveContracts      = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Active);
        ViewBag.PendingRequests      = await _context.ServiceRequests.CountAsync(sr => sr.Status == ServiceRequestStatus.Pending);
        ViewBag.RecentContracts      = await _context.Contracts
            .Include(c => c.Client)
            .OrderByDescending(c => c.CreatedOn)
            .Take(5)
            .ToListAsync();
        return View();
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
