using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Services;
using GLMS.Web.Services.Factories;
using GLMS.Web.Services.Observers;
using GLMS.Web.Services.Strategies;
using GLMS.Web.ViewModels;

namespace GLMS.Web.Controllers;

public class ContractsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ContractFactoryResolver _factoryResolver;
    private readonly IFileService _fileService;
    private readonly FileValidationStrategy _fileValidator;
    private readonly IEnumerable<IContractObserver> _observers;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(
        ApplicationDbContext context,
        ContractFactoryResolver factoryResolver,
        IFileService fileService,
        FileValidationStrategy fileValidator,
        IEnumerable<IContractObserver> observers,
        ILogger<ContractsController> logger)
    {
        _context         = context;
        _factoryResolver = factoryResolver;
        _fileService     = fileService;
        _fileValidator   = fileValidator;
        _observers       = observers;
        _logger          = logger;
    }

    public async Task<IActionResult> Index(ContractSearchViewModel? search)
    {
        search ??= new ContractSearchViewModel();

        // LINQ search/filter by date range and status
        IQueryable<Contract> query = _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.ServiceRequests);

        if (search.StartDateFrom.HasValue)
            query = query.Where(c => c.StartDate >= search.StartDateFrom.Value);

        if (search.StartDateTo.HasValue)
            query = query.Where(c => c.StartDate <= search.StartDateTo.Value);

        if (search.Status.HasValue)
            query = query.Where(c => c.Status == search.Status.Value);

        if (!string.IsNullOrWhiteSpace(search.TitleKeyword))
            query = query.Where(c => c.Title.Contains(search.TitleKeyword));

        search.Results = await query.OrderByDescending(c => c.CreatedOn).ToListAsync();
        return View(search);
    }

    public async Task<IActionResult> Details(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.ServiceRequests)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract is null) return NotFound();
        return View(contract);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new ContractCreateViewModel
        {
            ClientOptions = await GetClientSelectListAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContractCreateViewModel vm)
    {
        if (vm.SignedAgreement is not null)
        {
            var fileResult = _fileValidator.Validate(vm.SignedAgreement);
            if (!fileResult.IsValid)
                ModelState.AddModelError(nameof(vm.SignedAgreement), fileResult.ErrorMessage!);
        }

        if (!ModelState.IsValid)
        {
            vm.ClientOptions = await GetClientSelectListAsync();
            return View(vm);
        }

        // Factory Method — resolves the correct factory and builds the Contract
        var factory  = _factoryResolver.Resolve(vm.ServiceLevel);
        var contract = factory.CreateContract(vm.ClientId, vm.Title, vm.StartDate, vm.EndDate);

        if (vm.SignedAgreement is not null)
        {
            var (path, fileName) = await _fileService.SaveSignedAgreementAsync(vm.SignedAgreement);
            contract.SignedAgreementPath     = path;
            contract.SignedAgreementFileName = fileName;
        }

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Contract '{contract.Title}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract is null) return NotFound();
        ViewBag.Clients = await GetClientSelectListAsync();
        return View(contract);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Contract contract, IFormFile? newAgreement)
    {
        if (id != contract.Id) return BadRequest();

        if (newAgreement is not null)
        {
            var fileResult = _fileValidator.Validate(newAgreement);
            if (!fileResult.IsValid)
                ModelState.AddModelError(nameof(newAgreement), fileResult.ErrorMessage!);
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Clients = await GetClientSelectListAsync();
            return View(contract);
        }

        var existing = await _context.Contracts.FindAsync(id);
        if (existing is null) return NotFound();

        var previousStatus = existing.Status;

        existing.Title       = contract.Title;
        existing.StartDate   = contract.StartDate;
        existing.EndDate     = contract.EndDate;
        existing.Status      = contract.Status;
        existing.ServiceLevel = contract.ServiceLevel;

        if (newAgreement is not null)
        {
            if (existing.SignedAgreementPath is not null)
                _fileService.DeleteFile(existing.SignedAgreementPath);

            var (path, fileName) = await _fileService.SaveSignedAgreementAsync(newAgreement);
            existing.SignedAgreementPath     = path;
            existing.SignedAgreementFileName = fileName;
        }

        await _context.SaveChangesAsync();

        // Observer pattern — notify all registered observers of the status change
        if (previousStatus != existing.Status)
        {
            await _context.Entry(existing).Reference(c => c.Client).LoadAsync();
            foreach (var observer in _observers)
                await observer.OnStatusChangedAsync(existing, previousStatus, existing.Status);
        }

        TempData["Success"] = "Contract updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ContractStatus newStatus)
    {
        var contract = await _context.Contracts
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract is null) return NotFound();

        var previousStatus = contract.Status;
        contract.Status = newStatus;
        await _context.SaveChangesAsync();

        foreach (var observer in _observers)
            await observer.OnStatusChangedAsync(contract, previousStatus, newStatus);

        TempData["Success"] = $"Contract status updated to '{newStatus}'.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> DownloadAgreement(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract is null || string.IsNullOrEmpty(contract.SignedAgreementPath))
            return NotFound();

        var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var fullPath    = Path.Combine(webRootPath, contract.SignedAgreementPath);

        if (!System.IO.File.Exists(fullPath)) return NotFound();

        var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        return File(fileBytes, "application/pdf", contract.SignedAgreementFileName ?? "agreement.pdf");
    }

    public async Task<IActionResult> Delete(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract is null) return NotFound();
        return View(contract);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract is not null)
        {
            if (contract.SignedAgreementPath is not null)
                _fileService.DeleteFile(contract.SignedAgreementPath);

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Contract deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<IEnumerable<SelectListItem>> GetClientSelectListAsync()
    {
        return await _context.Clients
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToListAsync();
    }
}
