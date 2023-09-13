using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Minimal.Api.DbContext;
using Minimal.Api.Entities;
using Minimal.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<BooksContext>(options => options.UseNpgsql());
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("books", async (IBookService bookService, string? searchTerm) =>
    Results.Ok(
        searchTerm is null && string.IsNullOrWhiteSpace(searchTerm)
            ? await bookService.GetAllAsync()
            : await bookService.SearchByTitleAsync(searchTerm)
    ));

app.MapGet("books/{isbn}", async (string isbn, IBookService bookService) =>
{
    var result = await bookService.GetByIsbnAsync(isbn);

    return result is null ? Results.NotFound() : Results.Ok(result);
});

app.MapPost("books", async (Book book, IBookService bookService, IValidator<Book> validator) =>
{
    var validationResult = await validator.ValidateAsync(book);
    if (!validationResult.IsValid) return Results.BadRequest(validationResult.Errors);

    var created = await bookService.CreateAsync(book);
    if (!created)
        return Results.BadRequest(
            new List<ValidationFailure>(new List<ValidationFailure>
            {
                new("Isbn", "Already exists")
            })
        );


    return Results.Created($"/books/{book.Isbn}", book);
});

app.MapPut("books/{isbn}", async (string isbn, Book book, IBookService bookService, IValidator<Book> validator) =>
{
    book.Isbn = isbn;

    var validationResult = await validator.ValidateAsync(book);
    if (!validationResult.IsValid) return Results.BadRequest(validationResult.Errors);

    var updated = await bookService.UpdateAsync(book);

    return updated ? Results.Ok(book) : Results.NotFound();
});

app.MapDelete("books/{isbn}", async (string isbn, IBookService bookService) =>
{
    var deleted = await bookService.DeleteAsync(isbn);

    return deleted ? Results.NoContent() : Results.NotFound();
});

app.Run();