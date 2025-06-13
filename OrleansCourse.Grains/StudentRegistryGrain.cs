using OrleansCourse.Abstractions;
using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Grains;

using Orleans;
using Orleans.Runtime;

public class StudentRegistryGrain(
    [PersistentState("studentRegistry", "studentRegistryStore")] IPersistentState<StudentRegistryState> registryState)
    : Grain, IStudentRegistryGrain
{
    public Task<List<Guid>> GetAllStudentIds()
    {
        return Task.FromResult(registryState.State.StudentIds);
    }

    public async Task<List<Student>> GetAllStudents()
    {
        var results = new List<Student>();

        foreach (var studentId in registryState.State.StudentIds)
        {
            var studentGrain = GrainFactory.GetGrain<IStudentGrain>(studentId);
            var student = await studentGrain.GetStudent();
            results.Add(student);
        }

        return results;
    }

    public async Task<bool> RegisterStudent(Student student)
    {
        if (registryState.State.StudentIds.Contains(student.Id))
            return false;

        var studentGrain = GrainFactory.GetGrain<IStudentGrain>(student.Id);
        await studentGrain.SetStudent(student);

        registryState.State.StudentIds.Add(student.Id);
        await registryState.WriteStateAsync();
        return true;
    }

    public async Task<bool> RemoveStudent(Guid studentId)
    {
        if (!registryState.State.StudentIds.Contains(studentId))
            return false;
        registryState.State.StudentIds.Remove(studentId);
        await registryState.WriteStateAsync();
        return true;
    }
}
