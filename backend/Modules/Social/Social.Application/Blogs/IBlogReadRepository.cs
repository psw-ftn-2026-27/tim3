using Shared.Domain;

namespace Social.Application.Blogs;

public interface IBlogReadRepository
{
    Task<BlogDto?> GetByIdAsync(Guid id, Guid currentUserId);
    Task<List<BlogDto>> GetByAuthorAsync(Guid authorId);
    Task<PageResult<BlogDto>> GetPublishedAsync(int page, int pageSize);
}
