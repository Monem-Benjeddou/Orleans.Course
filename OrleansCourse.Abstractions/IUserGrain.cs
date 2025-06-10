using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Abstractions;

public interface IUserGrain : IGrainWithGuidKey
{
    Task<User> GetUser();
    Task SetUser(User user);
    Task<List<Guid>> GetClassIds();
    Task AddClass(Guid classId);
    Task<bool> RemoveClass(Guid classId);
}