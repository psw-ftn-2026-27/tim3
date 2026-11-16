using AutoMapper;
using Social.Application.Blogs;
using Social.Domain.Blogs;

namespace Social.Application;

public sealed class SocialMapperProfile : Profile
{
    public SocialMapperProfile()
    {
        CreateMap<Blog, BlogDto>()
            .ForCtorParam(nameof(BlogDto.CurrentUserVote), options => options.MapFrom(_ => (VoteType?)null))
            .ForCtorParam(nameof(BlogDto.Score), options => options.MapFrom(blog => blog.Votes.Count(vote => vote.VoteType == VoteType.Up) - blog.Votes.Count(vote => vote.VoteType == VoteType.Down)));
        CreateMap<Comment, CommentDto>()
            .ForCtorParam(nameof(CommentDto.AuthorDisplayName), options => options.MapFrom(_ => string.Empty));
    }
}
