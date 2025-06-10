using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Abstractions;

public interface IClassGrain : IGrainWithGuidKey
{
    Task UnenrollStudent(Guid studentId);
    Task<Class> GetClass();
    Task<bool> SetClass(Class cls);
    Task EnrollStudent(Guid studentId);
    Task<List<Guid>> GetEnrolledStudents();
    Task<bool> UpdateClass(Class updatedClass);
}