using AutoMapper;

namespace OrleansCourse.App.Models.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, Abstractions.Models.User>().ReverseMap();

        CreateMap<Class, Abstractions.Models.Class>().ReverseMap();

        CreateMap<Student, Abstractions.Models.Student>().ReverseMap();
    }
}
