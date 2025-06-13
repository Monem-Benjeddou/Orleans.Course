using OrleansCourse.Abstractions;
using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Grains;

public class StudentGrain([PersistentState("student", "StudentStorage")] IPersistentState<Student> studentState)
    : Grain, IStudentGrain
{
    public Task<Student> GetStudent() => Task.FromResult(studentState.State);

    public async Task SetStudent(Student student)
    {
        studentState.State = student;
        await studentState.WriteStateAsync();
    }

    public async Task UpdateStudent(Student updated)
    {
        var current = studentState.State;

        current.FirstName = updated.FirstName;
        current.LastName = updated.LastName;

        current.Email = updated.Email;
        current.DateOfBirth = updated.DateOfBirth;

        await studentState.WriteStateAsync();
    }

    public async Task EnrollInClass(Guid classId)
    {
        if (!studentState.State.EnrolledClassIds.Contains(classId))
        {
            studentState.State.EnrolledClassIds.Add(classId);
            await studentState.WriteStateAsync();
        }
    }

    public async Task UnenrollFromClass(Guid classId)
    {
        if (studentState.State.EnrolledClassIds.Remove(classId))
        {
            await studentState.WriteStateAsync();
        }
    }

    public Task<List<Guid>> GetEnrolledClassIds() =>
        Task.FromResult(studentState.State.EnrolledClassIds);
}