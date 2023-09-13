using Microsoft.EntityFrameworkCore;

namespace Minimal.Api.Entities;

[PrimaryKey(nameof(Isbn))]
public class Book
{
    public required string Isbn { get; set; }

    public required string Title { get; set; }

    public required string? Author { get; set; }

    public required string? ShortDescription { get; set; }

    public required int? PageCount { get; set; }

    public DateTime? ReleaseDate { get; set; }
}