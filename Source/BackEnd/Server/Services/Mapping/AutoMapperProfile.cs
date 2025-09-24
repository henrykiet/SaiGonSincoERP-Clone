using AutoMapper;
using reportSystem01.Server.Data;
using reportSystem01.Shared;

namespace reportSystem01.Server.Services.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserGroup, UserGroupDto>();
            CreateMap<UserGroupDto, UserGroup>();
        }
    }
}
