using OrleansCourse.App.Models;

namespace OrleansCourse.App.Services.Interfaces;

public interface IStudentService
{
    Task<List<Student>> GetAllStudentsAsync();
    
    Task<Student?> GetStudentAsync(Guid id);
    
    Task<bool> CreateStudentAsync(Student student);
    
    Task<bool> UpdateStudentAsync(Student student);
    
    Task<bool> DeleteStudentAsync(Guid id);
    
    Task<bool> EnrollStudentInClassAsync(Guid studentId, Guid classId);
    
    Task<bool> UnenrollStudentFromClassAsync(Guid studentId, Guid classId);
}