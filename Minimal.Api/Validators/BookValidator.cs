using FluentValidation;
using Minimal.Api.Entities;

namespace Minimal.Api.Validators;

public class BookValidator : AbstractValidator<Book>
{
    public BookValidator()
    {
        RuleFor(b => b.Isbn)
            .Matches(@"^\d{3}-[1-9]\d{9}$")
            .WithMessage("Invalid ISBN-13 value");

        RuleFor(b => b.Title)
            .NotEmpty();

        RuleFor(b => b.ShortDescription)
            .NotEmpty();

        RuleFor(b => b.Author)
            .NotEmpty();

        RuleFor(b => b.PageCount)
            .GreaterThan(0);
    }
}