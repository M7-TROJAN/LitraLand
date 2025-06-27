using LitraLand.Domain.Entities.Library;

namespace LitraLand.Application.Common.Interfaces.Repositories;
public interface IBookCopyRepository : IBaseRepository<BookCopy>
{
    void SetAllAsNotAvailable(int bookId);
}