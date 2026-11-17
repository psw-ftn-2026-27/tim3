using Identity.Contracts;
using Shared.Domain;
using Shared.Domain.Exceptions;
using Social.Application.Blogs;
using Social.Domain.Blogs;

namespace Social.Application.BlogReading;

public sealed class BlogReadingQueries
{
    private readonly IBlogReadRepository _blogReadRepository;
    private readonly IIdentityApi _identityApi;

    public BlogReadingQueries(IBlogReadRepository blogReadRepository, IIdentityApi identityApi)
    {
        _blogReadRepository = blogReadRepository;
        _identityApi = identityApi;
    }

    public async Task<BlogDto> GetByIdAsync(Guid id, Guid currentUserId)
    {
        var blog = await _blogReadRepository.GetByIdAsync(id, currentUserId);
        if (blog is null || blog.Status == BlogStatus.Draft)
        {
            throw new NotFoundException($"Blog {id} does not exist.");
        }

        return blog with { Comments = await WithAuthorNamesAsync(blog.Comments) };
    }

    private async Task<List<CommentDto>> WithAuthorNamesAsync(List<CommentDto> comments)
    {
        var userIds = comments.Select(comment => comment.UserId).Distinct().ToList();
        var names = await _identityApi.GetDisplayNamesAsync(userIds);
        var namesById = names.ToDictionary(name => name.UserId, name => name.DisplayName);

        return comments
            .Select(comment => comment with { AuthorDisplayName = namesById.GetValueOrDefault(comment.UserId, "") })
            .ToList();
    }

    public Task<PageResult<BlogDto>> GetPublishedAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _blogReadRepository.GetPublishedAsync(page, pageSize);
    }
}
