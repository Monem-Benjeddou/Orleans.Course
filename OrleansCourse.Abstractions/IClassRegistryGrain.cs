namespace OrleansCourse.Abstractions;

public interface IClassRegistryGrain : IGrainWithIntegerKey
{
    Task AddClassId(Guid classId);
    Task RemoveClassId(Guid classId);
    Task<List<Guid>> GetAllClassIds();
}