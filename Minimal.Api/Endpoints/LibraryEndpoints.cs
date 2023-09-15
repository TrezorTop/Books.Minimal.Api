using System.Net;
using System.Net.Mime;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Minimal.Api.DbContext;
using Minimal.Api.Endpoints.Internal;
using Minimal.Api.Entities;
using Minimal.Api.Services;

namespace Minimal.Api.Endpoints;

public class LibraryEndpoints : IEndpoints
{
    private const string BaseRoute = "books";
    private const string Tag = "Books";

    public static void AddServices(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection
            .AddScoped<IBookService, BookService>()
            .AddDbContext<BooksContext>(options => options.UseNpgsql());
    }

    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(BaseRoute, GetBooksAsync)
            .WithName("GetBooks")
            .Produces<IEnumerable<Book>>()
            .WithTags(Tag);


        app.MapGet($"{BaseRoute}/{{isbn}}", GetBookByIsbnAsync)
            .WithName("GetBook")
            .Produces<Book>()
            .Produces((int)HttpStatusCode.NotFound)
            .WithTags(Tag);


        app.MapPost(BaseRoute, CreateBookAsync)
            .WithName("CreateBook")
            .Accepts<Book>(MediaTypeNames.Application.Json)
            .Produces<Book>((int)HttpStatusCode.Created)
            .Produces<IEnumerable<ValidationFailure>>((int)HttpStatusCode.BadRequest)
            .WithTags(Tag);


        app.MapPut($"{BaseRoute}/{{isbn}}", UpdateBookAsync)
            .WithName("UpdateBook")
            .Accepts<Book>(MediaTypeNames.Application.Json)
            .Produces<Book>()
            .Produces<IEnumerable<ValidationFailure>>((int)HttpStatusCode.BadRequest)
            .WithTags(Tag);


        app.MapDelete($"{BaseRoute}/{{isbn}}", DeleteBookAsync)
            .WithName("DeleteBook")
            .Produces((int)HttpStatusCode.NoContent)
            .Produces((int)HttpStatusCode.NotFound)
            .WithTags(Tag);
    }

    private static async Task<IResult> GetBooksAsync(IBookService bookService, string? searchTerm)
    {
        return Results.Ok(searchTerm is null && string.IsNullOrWhiteSpace(searchTerm)
            ? await bookService.GetAllAsync()
            : await bookService.SearchByTitleAsync(searchTerm));
    }

    private static async Task<IResult> GetBookByIsbnAsync(string isbn, IBookService bookService)
    {
        var result = await bookService.GetByIsbnAsync(isbn);

        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    [Authorize]
    private static async Task<IResult> CreateBookAsync(Book book, IBookService bookService, IValidator<Book> validator)
    {
        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid) return Results.BadRequest(validationResult.Errors);

        var created = await bookService.CreateAsync(book);
        if (!created)
            return Results.BadRequest(new List<ValidationFailure>(new List<ValidationFailure> { new("Isbn", "Already exists") }));

        return Results.CreatedAtRoute("GetBook", new { isbn = book.Isbn }, book);
    }

    [Authorize]
    private static async Task<IResult> UpdateBookAsync(string isbn, Book book, IBookService bookService, IValidator<Book> validator)
    {
        book.Isbn = isbn;

        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid) return Results.BadRequest(validationResult.Errors);

        var updated = await bookService.UpdateAsync(book);

        return updated ? Results.Ok(book) : Results.NotFound();
    }

    [Authorize]
    private static async Task<IResult> DeleteBookAsync(string isbn, IBookService bookService)
    {
        var deleted = await bookService.DeleteAsync(isbn);

        return deleted ? Results.NoContent() : Results.NotFound();
    }
}