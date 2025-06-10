using OrleansCourse.Abstractions;
using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Grains;

using Orleans;
using Orleans.Runtime;

public class StudentRegistryGrain : Grain, IStudentRegistryGrain
{
    private readonly IPersistentState<StudentRegistryState> _registryState;

    public StudentRegistryGrain(
        [PersistentState("studentRegistry", "studentRegistryStore")]
        IPersistentState<StudentRegistryState> registryState)
    {
        _registryState = registryState;
    }

    public Task<List<Guid>> GetAllStudentIds()
    {
        return Task.FromResult(_registryState.State.StudentIds);
    }

    public async Task<List<Student>> GetAllStudents()
    {
        var results = new List<Student>();

        foreach (var studentId in _registryState.State.StudentIds)
        {
            var studentGrain = GrainFactory.GetGrain<IStudentGrain>(studentId);
            var student = await studentGrain.GetStudent();
            results.Add(student);
        }

        return results;
    }

    public async Task<bool> RegisterStudent(Student student)
    {
        if (_registryState.State.StudentIds.Contains(student.Id))
            return false;

        var studentGrain = GrainFactory.GetGrain<IStudentGrain>(student.Id);
        await studentGrain.SetStudent(student);

        _registryState.State.StudentIds.Add(student.Id);
        await _registryState.WriteStateAsync();
        return true;
    }

    public async Task<bool> RemoveStudent(Guid studentId)
    {
        if (!_registryState.State.StudentIds.Contains(studentId))
            return false;
        _registryState.State.StudentIds.Remove(studentId);
        await _registryState.WriteStateAsync();
        return true;
    }
}
