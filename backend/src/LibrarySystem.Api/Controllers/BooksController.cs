using LibrarySystem.Application.Books;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/books")]
public sealed class BooksController : ControllerBase
{
    private readonly IBookService _books;

    public BooksController(IBookService books)
    {
        _books = books;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _books.ListAsync(User.GetUserRole(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _books.GetByIdAsync(id, User.GetUserRole(), cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertBookRequest request, CancellationToken cancellationToken)
    {
        var result = await _books.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? this.ToCreatedResult($"/api/v1/books/{result.Value!.Id}", result)
            : this.ToActionResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertBookRequest request, CancellationToken cancellationToken)
    {
        var result = await _books.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _books.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }
}
