namespace OrleansCourse.App.Services.Interfaces;

public interface IAtRiskIdentificationService
{
    /// <summary>
    /// Identify if a student is at risk using logistic regression
    /// </summary>
    Task<(bool IsAtRisk, double Probability)> IdentifyAtRiskStudentAsync(Guid studentId, Guid classId);
    
    /// <summary>
    /// Get at-risk students for a specific class
    /// </summary>
    Task<List<(Guid StudentId, double RiskProbability)>> GetAtRiskStudentsAsync(Guid classId);
    
    /// <summary>
    /// Train the logistic regression at-risk identification model
    /// </summary>
    Task TrainAtRiskModelAsync();
}

