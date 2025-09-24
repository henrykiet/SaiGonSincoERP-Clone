using AutoMapper;
using reportSystem01.Server.Data;
using reportSystem01.Shared;
using User = reportSystem01.Server.Data.User;

namespace reportSystem01.Server.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDto>();
        }
    }
}
