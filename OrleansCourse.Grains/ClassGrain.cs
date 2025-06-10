using OrleansCourse.Abstractions;
using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Grains;

public class ClassGrain([PersistentState("class", "ClassStorage")] IPersistentState<Class> classState)
    : Grain, IClassGrain
{
    public Task<Class> GetClass()
    {
        var x = classState.State;
        return Task.FromResult(classState.State);
    }

    public async Task<bool> SetClass(Class cls)
    {
        cls.EnrolledStudentIds ??= [];
        classState.State = cls;
        await classState.WriteStateAsync();
        return true;
    }

    public Task<List<Guid>> GetEnrolledStudents() => Task.FromResult(classState.State.EnrolledStudentIds);

    public async Task EnrollStudent(Guid studentId)
    {
        if (!classState.State.EnrolledStudentIds.Contains(studentId))
        {
            classState.State.EnrolledStudentIds.Add(studentId);
            classState.State.CurrentEnrollment++;
            await classState.WriteStateAsync();
            Console.WriteLine($"Student {studentId} enrolled in class {classState.State.Id}");

        }
    }
    public async Task UnenrollStudent(Guid studentId)
    {
        if (classState.State.EnrolledStudentIds.Remove(studentId))
        {
            classState.State.CurrentEnrollment = classState.State.EnrolledStudentIds.Count;
            await classState.WriteStateAsync();
        }
    }
    public async Task<bool> UpdateClass(Class updatedClass)
    {
        var current = classState.State;

        current.Name = updatedClass.Name;
        current.Description = updatedClass.Description;
        current.InstructorName = updatedClass.InstructorName;
        current.StartDate = updatedClass.StartDate;
        current.EndDate = updatedClass.EndDate;
        current.MaxCapacity = updatedClass.MaxCapacity;
        current.IsActive = updatedClass.IsActive;
        current.Category = updatedClass.Category;


        await classState.WriteStateAsync();
        return true;
    }
}