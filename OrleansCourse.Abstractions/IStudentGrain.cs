using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Abstractions;

public interface IStudentGrain : IGrainWithGuidKey
{
    Task UpdateStudent(Student updated);
    Task<Student> GetStudent();
    Task SetStudent(Student student);
    Task<List<Guid>> GetEnrolledClassIds();
    Task EnrollInClass(Guid classId);
    Task UnenrollFromClass(Guid classId);
}