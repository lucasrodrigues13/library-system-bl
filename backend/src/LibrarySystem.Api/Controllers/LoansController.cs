using LibrarySystem.Api;
using LibrarySystem.Application.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/loans")]
public sealed class LoansController : ControllerBase
{
    private readonly ILoanService _loans;

    public LoansController(ILoanService loans)
    {
        _loans = loans;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _loans.ListAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _loans.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLoanRequest request, CancellationToken cancellationToken)
    {
        var result = await _loans.CreateAsync(request, User.GetUserId(), cancellationToken);
        return result.IsSuccess
            ? this.ToCreatedResult($"/api/v1/loans/{result.Value!.Id}", result)
            : this.ToActionResult(result);
    }

    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> Return(Guid id, CancellationToken cancellationToken)
    {
        var result = await _loans.ReturnAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }
}
