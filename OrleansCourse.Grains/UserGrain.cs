using OrleansCourse.Abstractions;
using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Grains;

public class UserGrain([PersistentState("user", "UserStorage")] IPersistentState<User> userState)
    : Grain, IUserGrain
{
    public Task<User> GetUser() => Task.FromResult(userState.State);

    public async Task SetUser(User user)
    {
        userState.State = user;
        await userState.WriteStateAsync();
    }

    public Task<List<Guid>> GetClassIds() => Task.FromResult(userState.State.ClassIds);

    public int GetTotalClassesCount() => userState.State.ClassIds.Count;

    public async Task<List<Class>> GetClassesDetails()
    {
        var classes = new List<Class>();

        foreach (var classId in userState.State.ClassIds)
        {
            var classGrain = GrainFactory.GetGrain<IClassGrain>(classId);
            var cls = await classGrain.GetClass();
            classes.Add(cls);
        }

        return classes;
    }

    public async Task AddClass(Guid classId)
    {
        if (!userState.State.ClassIds.Contains(classId))
        {
            userState.State.ClassIds.Add(classId);
            await userState.WriteStateAsync();

        }
    }

    public async Task<bool> RemoveClass(Guid classId)
    {
        if (userState.State.ClassIds.Contains(classId))
        {
            userState.State.ClassIds.Remove(classId);
            await userState.WriteStateAsync();

            // Optional: remove student from class's enrolled students
            var classGrain = GrainFactory.GetGrain<IClassGrain>(classId);
            // Implement a RemoveStudent method in ClassGrain for symmetry if needed
            // await classGrain.RemoveStudent(this.GetPrimaryKey());

            return true;
        }
        return false;
    }
}
