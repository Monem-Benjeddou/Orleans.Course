using OrleansCourse.Abstractions;
using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Grains;

public class ClassRegistryGrain([PersistentState("classRegistry", "ClassRegistryStorage")] IPersistentState<ClassRegistryState> state) : Grain, IClassRegistryGrain
{
    private readonly HashSet<Guid> _classIds = new();

    public Task AddClassId(Guid classId)
    {
        _classIds.Add(classId);
        return Task.CompletedTask;
    }

    public Task RemoveClassId(Guid classId)
    {
        _classIds.Remove(classId);
        return Task.CompletedTask;
    }

    public Task<List<Guid>> GetAllClassIds()
    {
        return Task.FromResult(_classIds.ToList());
    }
}