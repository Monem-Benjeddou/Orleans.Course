using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Abstractions;

public interface IStudentPerformanceGrain : IGrainWithGuidKey
{
    Task<StudentPerformance> GetPerformance();
    Task SetPerformance(StudentPerformance performance);
    Task UpdatePerformance(StudentPerformance performance);
    Task AddGrade(Guid gradeId);
    Task<double> GetCurrentAverage();
    Task<double> GetPredictedFinalGrade();
    Task<bool> IsAtRisk();
}

