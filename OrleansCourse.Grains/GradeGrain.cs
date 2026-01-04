using OrleansCourse.Abstractions;
using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Grains;

public class GradeGrain([PersistentState("grade", "GradeStorage")] IPersistentState<Grade> gradeState)
    : Grain, IGradeGrain
{
    public Task<Grade> GetGrade() => Task.FromResult(gradeState.State);

    public async Task SetGrade(Grade grade)
    {
        gradeState.State = grade;
        await gradeState.WriteStateAsync();
    }

    public async Task UpdateGrade(Grade updated)
    {
        gradeState.State = updated;
        await gradeState.WriteStateAsync();
    }

    public async Task DeleteGrade()
    {
        await gradeState.ClearStateAsync();
    }
}

