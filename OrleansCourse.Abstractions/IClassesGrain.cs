using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Abstractions;

public interface IClassesGrain : IGrainWithStringKey
{
    Task<HashSet<Class>> GetAllClasses();

    Task AddOrUpdateClassAsync(Class classDetails);
}
