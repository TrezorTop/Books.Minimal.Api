using FluentValidation;
using Minimal.Api.Entities;

namespace Minimal.Api.Validators;

public class BookValidator : AbstractValidator<Book>
{
    public BookValidator()
    {
        RuleFor(b => b.Isbn)
            .Matches(@"(?=.{17}$)97(?:8|9)([ -])\d{1,5}\1\d{1,7}\1\d{1,6}\1\d$")
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