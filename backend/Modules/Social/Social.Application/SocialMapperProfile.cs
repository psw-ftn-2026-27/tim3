using AutoMapper;
using Social.Application.Blogs;
using Social.Domain.Blogs;

namespace Social.Application;

public sealed class SocialMapperProfile : Profile
{
    public SocialMapperProfile()
    {
        CreateMap<Blog, BlogDto>()
            .ForCtorParam(nameof(BlogDto.CurrentUserVote), options => options.MapFrom(_ => (VoteType?)null));
        CreateMap<Comment, CommentDto>();
    }
}
