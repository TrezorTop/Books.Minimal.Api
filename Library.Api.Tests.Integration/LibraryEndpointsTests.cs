using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Minimal.Api;
using Minimal.Api.Entities;

namespace Library.Api.Tests.Integration;

public class LibraryEndpointsTests : IClassFixture<WebApplicationFactory<IApiMarker>>
{
    private WebApplicationFactory<IApiMarker> _factory;

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
        var result = await httpClient.PostAsJsonAsync("/books", book);
        var createBook = await result.Content.ReadFromJsonAsync<Book>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        createBook.Should().BeEquivalentTo(book);
        result.Headers.Location.Should().Be($"/books/{book.Isbn}");
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
}