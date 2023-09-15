using Microsoft.EntityFrameworkCore;
using Minimal.Api.DbContext;
using Minimal.Api.Entities;

namespace Minimal.Api.Services;

public class BookService : IBookService
{
    private readonly BooksContext _booksContext;


    public BookService(BooksContext booksContext)
    {
        _booksContext = booksContext;
    }

    public async Task<Book?> GetByIsbnAsync(string isbn)
    {
        return await _booksContext.Books.FirstOrDefaultAsync(b => b.Isbn == isbn);
    }

    public async Task<IEnumerable<Book>> SearchByTitleAsync(string title)
    {
        var upperCaseTitle = title.ToUpper();

        return await _booksContext.Books.Where(b => b.Title.ToUpper().Contains(upperCaseTitle)).ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        return await _booksContext.Books.ToListAsync();
    }

    public async Task<bool> CreateAsync(Book book)
    {
        var oldBook = await GetByIsbnAsync(book.Isbn);

        if (oldBook != null) return false;

        _booksContext.Add(book);
        await _booksContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        _booksContext.Books.Update(book);
        await _booksContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(string isbn)
    {
        var book = await _booksContext.Books.FirstOrDefaultAsync(b => b.Isbn == isbn);

        if (book is null) return false;

        _booksContext.Books.Remove(book);
        await _booksContext.SaveChangesAsync();

        return true;
    }
}