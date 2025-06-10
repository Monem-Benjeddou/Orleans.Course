using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Abstractions;

public interface IStudentRegistryGrain : IGrainWithIntegerKey
{
    Task<List<Guid>> GetAllStudentIds();
    Task<List<Student>> GetAllStudents();
    Task<bool> RegisterStudent(Student student);
    Task<bool> RemoveStudent(Guid studentId);
}