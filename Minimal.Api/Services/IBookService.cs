using Minimal.Api.Entities;

namespace Minimal.Api.Services;

public interface IBookService
{
    public Task<Book?> GetByIsbnAsync(string isbn);

    public Task<IEnumerable<Book>> GetAllAsync();

    public Task<IEnumerable<Book>> SearchByTitleAsync(string title);

    public Task<bool> CreateAsync(Book book);

    public Task<bool> UpdateAsync(Book book);

    public Task<bool> DeleteAsync(string isbn);
}