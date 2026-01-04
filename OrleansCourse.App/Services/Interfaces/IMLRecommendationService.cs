using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.App.Services.Interfaces;

public interface IMLRecommendationService
{
    /// <summary>
    /// Get class recommendations for a student using ALS collaborative filtering
    /// </summary>
    Task<List<ClassRecommendation>> GetClassRecommendationsAsync(Guid studentId, int topN = 10);
    
    /// <summary>
    /// Train the ALS recommendation model with current enrollment data
    /// </summary>
    Task TrainRecommendationModelAsync();
}

