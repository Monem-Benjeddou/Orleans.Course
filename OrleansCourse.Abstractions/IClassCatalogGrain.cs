namespace OrleansCourse.Abstractions;

public interface IClassCatalogGrain : IGrainWithIntegerKey
{
    Task<List<Guid>> GetAllClassIds();
    Task AddClassId(Guid classId);
    Task RemoveClassId(Guid classId);
}