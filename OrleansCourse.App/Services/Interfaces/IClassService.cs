using OrleansCourse.App.Models;

namespace OrleansCourse.App.Services.Interfaces;

public interface IClassService
{
    Task<List<Class>> GetAllClassesAsync();
    Task<List<Class>> GetAllClassesAsync(Guid userId);
    Task<Class?> GetClassAsync(Guid id);
    
    Task<List<Class>> GetClassesByCategoryAsync(ClassCategory category);
    
    Task<bool> CreateClassAsync(Class classObj);
    
    Task<bool> UpdateClassAsync(Class classObj);
    Task<bool> DeleteClassAsync(Guid userId, Guid classId);
    Task<bool> AddClassToUserAsync(Guid userId, Guid classId);
    Task<List<Student>> GetEnrolledStudentsAsync(Guid classId);
}