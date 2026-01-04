using Orleans;
using OrleansCourse.Abstractions;
using OrleansCourse.Abstractions.Models;
using OrleansCourse.App.Services.Interfaces;

namespace OrleansCourse.App.Services;

public class GradeService(IClusterClient client) : IGradeService
{
    public async Task<Grade> GetGradeAsync(Guid gradeId)
    {
        var gradeGrain = client.GetGrain<IGradeGrain>(gradeId);
        return await gradeGrain.GetGrade();
    }

    public async Task<List<Grade>> GetStudentGradesAsync(Guid studentId, Guid? classId = null)
    {
        var performanceGrain = client.GetGrain<IStudentPerformanceGrain>(
            GetPerformanceGrainId(studentId, classId ?? Guid.Empty));
        var performance = await performanceGrain.GetPerformance();
        
        if (performance == null || performance.GradeIds.Count == 0)
            return new List<Grade>();

        var tasks = performance.GradeIds.Select(id => GetGradeAsync(id));
        var grades = await Task.WhenAll(tasks);
        return grades.Where(g => classId == null || g.ClassId == classId).ToList();
    }

    public async Task<bool> CreateGradeAsync(Grade grade)
    {
        var gradeGrain = client.GetGrain<IGradeGrain>(grade.Id);
        await gradeGrain.SetGrade(grade);
        
        // Update student performance
        var performanceGrain = client.GetGrain<IStudentPerformanceGrain>(
            GetPerformanceGrainId(grade.StudentId, grade.ClassId));
        await performanceGrain.AddGrade(grade.Id);
        await UpdateStudentPerformanceAsync(grade.StudentId, grade.ClassId);
        
        return true;
    }

    public async Task<bool> UpdateGradeAsync(Grade grade)
    {
        var gradeGrain = client.GetGrain<IGradeGrain>(grade.Id);
        await gradeGrain.UpdateGrade(grade);
        await UpdateStudentPerformanceAsync(grade.StudentId, grade.ClassId);
        return true;
    }

    public async Task<bool> DeleteGradeAsync(Guid gradeId)
    {
        var grade = await GetGradeAsync(gradeId);
        var gradeGrain = client.GetGrain<IGradeGrain>(gradeId);
        await gradeGrain.DeleteGrade();
        await UpdateStudentPerformanceAsync(grade.StudentId, grade.ClassId);
        return true;
    }

    public async Task<StudentPerformance> GetStudentPerformanceAsync(Guid studentId, Guid classId)
    {
        var performanceGrain = client.GetGrain<IStudentPerformanceGrain>(
            GetPerformanceGrainId(studentId, classId));
        return await performanceGrain.GetPerformance();
    }

    public async Task<bool> UpdateStudentPerformanceAsync(Guid studentId, Guid classId)
    {
        var grades = await GetStudentGradesAsync(studentId, classId);
        if (grades.Count == 0) return false;

        var average = grades.Average(g => g.Percentage);
        var performanceGrain = client.GetGrain<IStudentPerformanceGrain>(
            GetPerformanceGrainId(studentId, classId));
        
        var performance = await performanceGrain.GetPerformance();
        if (performance == null)
        {
            performance = new StudentPerformance
            {
                StudentId = studentId,
                ClassId = classId,
                GradeIds = grades.Select(g => g.Id).ToList(),
                LastUpdated = DateTime.UtcNow
            };
        }
        
        performance = performance with
        {
            CurrentAverage = average,
            GradeIds = grades.Select(g => g.Id).ToList(),
            LastUpdated = DateTime.UtcNow
        };
        
        await performanceGrain.SetPerformance(performance);
        return true;
    }

    private static Guid GetPerformanceGrainId(Guid studentId, Guid classId)
    {
        // Combine student and class IDs to create a unique performance grain ID
        var bytes = new byte[16];
        studentId.ToByteArray().CopyTo(bytes, 0);
        classId.ToByteArray().CopyTo(bytes, 8);
        return new Guid(bytes);
    }
}

