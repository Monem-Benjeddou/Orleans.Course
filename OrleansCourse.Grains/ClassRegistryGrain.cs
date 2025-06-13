using OrleansCourse.Abstractions;
using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Grains;

public class ClassRegistryGrain([PersistentState("classRegistry", "ClassRegistryStorage")] IPersistentState<ClassRegistryState> state) : Grain, IClassRegistryGrain
{

    public Task AddClassId(Guid classId)
    {
        state.State.ClassIds.Add(classId);
        return Task.CompletedTask;
    }

    public Task RemoveClassId(Guid classId)
    {
        state.State.ClassIds.Remove(classId);
        return Task.CompletedTask;
    }

    public Task<List<Guid>> GetAllClassIds()
    {
        return Task.FromResult(state.State.ClassIds.ToList());
    }
}