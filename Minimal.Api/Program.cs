using System.Net;
using System.Net.Mime;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Minimal.Api.Auth;
using Minimal.Api.DbContext;
using Minimal.Api.Entities;
using Minimal.Api.Services;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    Args = args,
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<BooksContext>(options => options.UseNpgsql());
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<IBookService, BookService>();

builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
    .AddScheme<ApiKeySchemeOptions, ApiKeyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => { });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapGet("books", async (IBookService bookService, string? searchTerm) =>
        Results.Ok(
            searchTerm is null && string.IsNullOrWhiteSpace(searchTerm)
                ? await bookService.GetAllAsync()
                : await bookService.SearchByTitleAsync(searchTerm)
        ))
    .WithName("GetBooks")
    .Produces<IEnumerable<Book>>()
    .WithTags("Books");

app.MapGet("books/{isbn}", async (string isbn, IBookService bookService) =>
    {
        var result = await bookService.GetByIsbnAsync(isbn);

        return result is null ? Results.NotFound() : Results.Ok(result);
    })
    .WithName("GetBook")
    .Produces<Book>()
    .Produces((int)HttpStatusCode.NotFound)
    .WithTags("Books");

app.MapPost(
        "books",
        [Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
        async (Book book, IBookService bookService, IValidator<Book> validator
        ) =>
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


            return Results.CreatedAtRoute("GetBook", new { isbn = book.Isbn }, book);
        })
    .WithName("CreateBook")
    .Accepts<Book>(MediaTypeNames.Application.Json)
    .Produces<Book>((int)HttpStatusCode.Created)
    .Produces<IEnumerable<ValidationFailure>>((int)HttpStatusCode.BadRequest)
    .WithTags("Books");

app.MapPut("books/{isbn}", async (string isbn, Book book, IBookService bookService, IValidator<Book> validator) =>
    {
        book.Isbn = isbn;

        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid) return Results.BadRequest(validationResult.Errors);

        var updated = await bookService.UpdateAsync(book);

        return updated ? Results.Ok(book) : Results.NotFound();
    })
    .WithName("UpdateBook")
    .Accepts<Book>(MediaTypeNames.Application.Json)
    .Produces<Book>()
    .Produces<IEnumerable<ValidationFailure>>((int)HttpStatusCode.BadRequest)
    .WithTags("Books");

app.MapDelete("books/{isbn}", async (string isbn, IBookService bookService) =>
    {
        var deleted = await bookService.DeleteAsync(isbn);

        return deleted ? Results.NoContent() : Results.NotFound();
    })
    .WithName("DeleteBook")
    .Produces((int)HttpStatusCode.NoContent)
    .Produces((int)HttpStatusCode.NotFound)
    .WithTags("Books");

app.Run();