using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Abstractions;

public interface IGradeGrain : IGrainWithGuidKey
{
    Task<Grade> GetGrade();
    Task SetGrade(Grade grade);
    Task UpdateGrade(Grade grade);
    Task DeleteGrade();
}

