using OrleansCourse.Abstractions;
using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Grains;

public class StudentPerformanceGrain([PersistentState("performance", "PerformanceStorage")] IPersistentState<StudentPerformance> performanceState)
    : Grain, IStudentPerformanceGrain
{
    public Task<StudentPerformance> GetPerformance() => Task.FromResult(performanceState.State);

    public async Task SetPerformance(StudentPerformance performance)
    {
        performanceState.State = performance;
        await performanceState.WriteStateAsync();
    }

    public async Task UpdatePerformance(StudentPerformance updated)
    {
        performanceState.State = updated;
        await performanceState.WriteStateAsync();
    }

    public async Task AddGrade(Guid gradeId)
    {
        if (!performanceState.State.GradeIds.Contains(gradeId))
        {
            performanceState.State.GradeIds.Add(gradeId);
            await performanceState.WriteStateAsync();
        }
    }

    public Task<double> GetCurrentAverage() => Task.FromResult(performanceState.State.CurrentAverage);

    public Task<double> GetPredictedFinalGrade() => Task.FromResult(performanceState.State.PredictedFinalGrade);

    public Task<bool> IsAtRisk() => Task.FromResult(performanceState.State.IsAtRisk);
}

