using OrleansCourse.Abstractions;
using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Grains;

public class UserGrain : Grain, IUserGrain
{
    private readonly IPersistentState<User> _userState;

    public UserGrain([PersistentState("user", "UserStorage")] IPersistentState<User> userState)
    {
        _userState = userState;
    }

    public Task<User> GetUser() => Task.FromResult(_userState.State);

    public async Task SetUser(User user)
    {
        _userState.State = user;
        await _userState.WriteStateAsync();
    }

    public Task<List<Guid>> GetClassIds() => Task.FromResult(_userState.State.ClassIds);

    public int GetTotalClassesCount() => _userState.State.ClassIds.Count;

    public async Task<List<Class>> GetClassesDetails()
    {
        var classes = new List<Class>();

        foreach (var classId in _userState.State.ClassIds)
        {
            var classGrain = GrainFactory.GetGrain<IClassGrain>(classId);
            var cls = await classGrain.GetClass();
            classes.Add(cls);
        }

        return classes;
    }

    public async Task AddClass(Guid classId)
    {
        if (!_userState.State.ClassIds.Contains(classId))
        {
            _userState.State.ClassIds.Add(classId);
            await _userState.WriteStateAsync();

        }
    }

    public async Task<bool> RemoveClass(Guid classId)
    {
        if (_userState.State.ClassIds.Contains(classId))
        {
            _userState.State.ClassIds.Remove(classId);
            await _userState.WriteStateAsync();

            // Optional: remove student from class's enrolled students
            var classGrain = GrainFactory.GetGrain<IClassGrain>(classId);
            // Implement a RemoveStudent method in ClassGrain for symmetry if needed
            // await classGrain.RemoveStudent(this.GetPrimaryKey());

            return true;
        }
        return false;
    }
}
