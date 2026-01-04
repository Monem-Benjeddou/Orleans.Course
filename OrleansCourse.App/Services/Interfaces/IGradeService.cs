using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.App.Services.Interfaces;

public interface IGradeService
{
    Task<Grade> GetGradeAsync(Guid gradeId);
    Task<List<Grade>> GetStudentGradesAsync(Guid studentId, Guid? classId = null);
    Task<bool> CreateGradeAsync(Grade grade);
    Task<bool> UpdateGradeAsync(Grade grade);
    Task<bool> DeleteGradeAsync(Guid gradeId);
    Task<StudentPerformance> GetStudentPerformanceAsync(Guid studentId, Guid classId);
    Task<bool> UpdateStudentPerformanceAsync(Guid studentId, Guid classId);
}

