using OrleansCourse.Abstractions;
using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Grains;

public class StudentGrain : Grain, IStudentGrain
{
    private readonly IPersistentState<Student> _studentState;

    public StudentGrain(
        [PersistentState("student", "StudentStorage")]
        IPersistentState<Student> studentState)
    {
        _studentState = studentState;
    }

    public Task<Student> GetStudent() => Task.FromResult(_studentState.State);

    public async Task SetStudent(Student student)
    {
        _studentState.State = student;
        await _studentState.WriteStateAsync();
    }

    public async Task UpdateStudent(Student updated)
    {
        var current = _studentState.State;

        current.FirstName = updated.FirstName;
        current.LastName = updated.LastName;

        current.Email = updated.Email;
        current.DateOfBirth = updated.DateOfBirth;

        await _studentState.WriteStateAsync();
    }

    public async Task EnrollInClass(Guid classId)
    {
        if (!_studentState.State.EnrolledClassIds.Contains(classId))
        {
            _studentState.State.EnrolledClassIds.Add(classId);
            await _studentState.WriteStateAsync();
        }
    }

    public async Task UnenrollFromClass(Guid classId)
    {
        if (_studentState.State.EnrolledClassIds.Remove(classId))
        {
            await _studentState.WriteStateAsync();
        }
    }

    public Task<List<Guid>> GetEnrolledClassIds() =>
        Task.FromResult(_studentState.State.EnrolledClassIds);
}