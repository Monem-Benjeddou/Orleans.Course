using OrleansCourse.Abstractions;

namespace OrleansCourse.Grains;

public class ClassCatalogGrain([PersistentState("classCatalog", "ClassStorage")] IPersistentState<List<Guid>> classIds)
    : Grain, IClassCatalogGrain
{
    public Task<List<Guid>> GetAllClassIds()
    {
        return Task.FromResult(classIds.State ?? new List<Guid>());
    }

    public async Task AddClassId(Guid classId)
    {
        if (!classIds.State.Contains(classId))
        {
            classIds.State.Add(classId);
            await classIds.WriteStateAsync();
        }
    }

    public async Task RemoveClassId(Guid classId)
    {
        if (classIds.State.Contains(classId))
        {
            classIds.State.Remove(classId);
            await classIds.WriteStateAsync();
        }
    }
}