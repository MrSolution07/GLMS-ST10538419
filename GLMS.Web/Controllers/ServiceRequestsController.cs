using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Services;
using GLMS.Web.Services.Strategies;
using GLMS.Web.ViewModels;

namespace GLMS.Web.Controllers;

public class ServiceRequestsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrencyService _currencyService;
    private readonly ServiceRequestValidationStrategy _validationStrategy;
    private readonly ILogger<ServiceRequestsController> _logger;

    public ServiceRequestsController(
        ApplicationDbContext context,
        ICurrencyService currencyService,
        ServiceRequestValidationStrategy validationStrategy,
        ILogger<ServiceRequestsController> logger)
    {
        _context            = context;
        _currencyService    = currencyService;
        _validationStrategy = validationStrategy;
        _logger             = logger;
    }

    public async Task<IActionResult> Index()
    {
        var requests = await _context.ServiceRequests
            .Include(sr => sr.Contract)
            .ThenInclude(c => c!.Client)
            .OrderByDescending(sr => sr.RaisedOn)
            .ToListAsync();
        return View(requests);
    }

    public async Task<IActionResult> Details(int id)
    {
        var request = await _context.ServiceRequests
            .Include(sr => sr.Contract)
            .ThenInclude(c => c!.Client)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (request is null) return NotFound();
        return View(request);
    }

    public async Task<IActionResult> Create(int? contractId)
    {
        var rate = await _currencyService.GetUsdToZarRateAsync();

        var vm = new ServiceRequestCreateViewModel
        {
            ContractId      = contractId ?? 0,
            ExchangeRate    = rate,
            ContractOptions = await GetActiveContractSelectListAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.ExchangeRate    = await _currencyService.GetUsdToZarRateAsync();
            vm.ContractOptions = await GetActiveContractSelectListAsync();
            return View(vm);
        }

        var contract = await _context.Contracts.FindAsync(vm.ContractId);
        if (contract is null)
        {
            ModelState.AddModelError("", "Selected contract not found.");
            vm.ContractOptions = await GetActiveContractSelectListAsync();
            return View(vm);
        }

        var rate = await _currencyService.GetUsdToZarRateAsync();
        var zarAmount = _currencyService.ConvertUsdToZar(vm.CostUsd, rate);

        var serviceRequest = new ServiceRequest
        {
            ContractId       = vm.ContractId,
            Description      = vm.Description,
            CostUsd          = vm.CostUsd,
            CostZar          = zarAmount,
            ExchangeRateUsed = rate,
            Status           = ServiceRequestStatus.Pending,
            RaisedOn         = DateTime.UtcNow
        };

        // Strategy pattern — validates the business rule before persisting
        var validationResult = _validationStrategy.Validate((contract, serviceRequest));
        if (!validationResult.IsValid)
        {
            ModelState.AddModelError("", validationResult.ErrorMessage!);
            vm.ExchangeRate    = rate;
            vm.ContractOptions = await GetActiveContractSelectListAsync();
            return View(vm);
        }

        _context.ServiceRequests.Add(serviceRequest);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Service request raised successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var request = await _context.ServiceRequests
            .Include(sr => sr.Contract)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (request is null) return NotFound();
        return View(request);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var request = await _context.ServiceRequests.FindAsync(id);
        if (request is not null)
        {
            _context.ServiceRequests.Remove(request);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Service request deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// AJAX endpoint — returns the live ZAR equivalent for a given USD amount.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetZarAmount(decimal usdAmount)
    {
        var rate = await _currencyService.GetUsdToZarRateAsync();
        var zar  = _currencyService.ConvertUsdToZar(usdAmount, rate);
        return Json(new { zar = zar.ToString("F2"), rate = rate.ToString("F4") });
    }

    private async Task<IEnumerable<SelectListItem>> GetActiveContractSelectListAsync()
    {
        return await _context.Contracts
            .Where(c => c.Status == ContractStatus.Active)
            .Include(c => c.Client)
            .OrderBy(c => c.Title)
            .Select(c => new SelectListItem(
                $"{c.Title} ({c.Client!.Name})",
                c.Id.ToString()))
            .ToListAsync();
    }
}
