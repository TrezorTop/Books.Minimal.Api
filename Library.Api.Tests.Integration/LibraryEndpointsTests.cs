using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Minimal.Api;
using Minimal.Api.Entities;

namespace Library.Api.Tests.Integration;

public class LibraryEndpointsTests : IClassFixture<WebApplicationFactory<IApiMarker>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<IApiMarker> _factory;
    private readonly List<string> _createdIsbns = new List<string>();

    public LibraryEndpointsTests(WebApplicationFactory<IApiMarker> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateBook_CreatesBook_WhenDataIsCorrect()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        httpClient.DefaultRequestHeaders.Add("Authorization", "SecretKey");
        // Act
        var result = await httpClient.PostAsJsonAsync("books", book);
        var createdBook = await result.Content.ReadFromJsonAsync<Book>();
        _createdIsbns.Add(book.Isbn);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        createdBook.Should().BeEquivalentTo(book);
        result.Headers.Location.Should().Be($"books/{book.Isbn}");
    }

    [Fact]
    public async Task CreateBook_Fails_WhenIsbnIsInvalid()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        book.Isbn = "INVALID VALUE";
        httpClient.DefaultRequestHeaders.Add("Authorization", "SecretKey");

        // Act
        var result = await httpClient.PostAsJsonAsync("books", book);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("Invalid ISBN-13 value");
    }

    [Fact]
    public async Task CreateBook_Fails_WhenBookExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        httpClient.DefaultRequestHeaders.Add("Authorization", "SecretKey");

        // Act
        await httpClient.PostAsJsonAsync("books", book);
        var result = await httpClient.PostAsJsonAsync("books", book);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();
        _createdIsbns.Add(book.Isbn);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("Already exists");
    }

    [Fact]
    public async Task GetBook_ReturnBook_WhenBookExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        httpClient.DefaultRequestHeaders.Add("Authorization", "SecretKey");
        await httpClient.PostAsJsonAsync("books", book);
        _createdIsbns.Add(book.Isbn);

        // Act
        var result = await httpClient.GetAsync($"books/{book.Isbn}");
        var existingBook = await result.Content.ReadFromJsonAsync<Book>();

        // Assert
        existingBook.Should().BeEquivalentTo(book);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBook_ReturnsNotFound_WhenBookDoesNotExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();

        // Act
        var result = await httpClient.GetAsync($"books/invalid_isbn");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllBook_ReturnsAllBooks_WhenBooksExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();

        // Act
        var result = await httpClient.GetAsync($"books");
        var books = await result.Content.ReadFromJsonAsync<IEnumerable<Book>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        books.Should().BeAssignableTo<IEnumerable<Book>>();
    }
    
    [Fact]
    public async Task SearchBooks_ReturnsBooks_WhenTitleMatches()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook("Test Title");
        httpClient.DefaultRequestHeaders.Add("Authorization", "SecretKey");
        await httpClient.PostAsJsonAsync("books", book);
        _createdIsbns.Add(book.Isbn);

        // Act
        var result = await httpClient.GetAsync($"books?searchTerm=st Tit");
        var existingBooks = await result.Content.ReadFromJsonAsync<IEnumerable<Book>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        existingBooks.Should().BeAssignableTo<IEnumerable<Book>>();
    }

    private static Book GenerateBook(string title = "Default Title")
    {
        return new Book
        {
            Isbn = GenerateIsbn(),
            Title = title,
            Author = "Author",
            ShortDescription = "ShortDescription",
            PageCount = Random.Shared.Next(1, 1000)
        };
    }

    private static string GenerateIsbn()
    {
        return $"{Random.Shared.Next(100, 999)}-{Random.Shared.Next(1_000_000_000, 1_999_999_999)}";
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        var httpClient = _factory.CreateClient();

        foreach (var isbn in _createdIsbns)
        {
            await httpClient.DeleteAsync($"books/{isbn}");
        }
    }
}