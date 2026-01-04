using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Orleans;
using OrleansCourse.Abstractions;
using OrleansCourse.Abstractions.Models;
using OrleansCourse.App.Services.Interfaces;

namespace OrleansCourse.App.Services;

public class MLRecommendationService(IClusterClient client, IClassService classService) 
    : IMLRecommendationService
{
    private MLContext? _mlContext;
    private ITransformer? _model;
    private PredictionEngine<EnrollmentData, ClassPrediction>? _predictionEngine;
    private readonly object _lockObject = new();

    public async Task<List<ClassRecommendation>> GetClassRecommendationsAsync(Guid studentId, int topN = 10)
    {
        // Ensure model is trained
        if (_model == null)
        {
            await TrainRecommendationModelAsync();
        }

        if (_model == null || _mlContext == null)
        {
            return new List<ClassRecommendation>();
        }

        // Get all classes
        var allClasses = await classService.GetAllClassesAsync();
        
        // Get student's current enrollments
        var studentGrain = client.GetGrain<IStudentGrain>(studentId);
        var enrolledClassIds = await studentGrain.GetEnrolledClassIds();
        
        // Get all students and their enrollments for collaborative filtering
        var registryGrain = client.GetGrain<IStudentRegistryGrain>(0);
        var allStudentIds = await registryGrain.GetAllStudents();
        
        var recommendations = new List<ClassRecommendation>();
        
        // Create prediction engine if not exists
        lock (_lockObject)
        {
            if (_predictionEngine == null && _model != null)
            {
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<EnrollmentData, ClassPrediction>(_model);
            }
        }

        if (_predictionEngine == null) return recommendations;

        // For each class not enrolled, predict the score
        foreach (var classItem in allClasses.Where(c => !enrolledClassIds.Contains(c.Id)))
        {
            // Use ALS collaborative filtering approach
            // Calculate similarity based on other students' enrollments
            var similarStudents = await FindSimilarStudentsAsync(studentId, allStudentIds, enrolledClassIds);
            
            var recommendationScore = CalculateRecommendationScore(
                studentId, 
                classItem.Id, 
                similarStudents, 
                allStudentIds);
            
            if (recommendationScore > 0)
            {
                recommendations.Add(new ClassRecommendation
                {
                    ClassId = classItem.Id,
                    ClassName = classItem.Name,
                    RecommendationScore = recommendationScore,
                    Reason = $"Recommended based on similar students' preferences"
                });
            }
        }

        return recommendations
            .OrderByDescending(r => r.RecommendationScore)
            .Take(topN)
            .ToList();
    }

    public async Task TrainRecommendationModelAsync()
    {
        _mlContext = new MLContext(seed: 0);

        // Collect enrollment data
        var registryGrain = client.GetGrain<IStudentRegistryGrain>(0);
        var allStudentIds = await registryGrain.GetAllStudents();
        
        var enrollmentData = new List<EnrollmentData>();
        
        foreach (var studentId in allStudentIds)
        {
            var studentGrain = client.GetGrain<IStudentGrain>(studentId.Id);
            var enrolledClassIds = await studentGrain.GetEnrolledClassIds();
            
            foreach (var classId in enrolledClassIds)
            {
                enrollmentData.Add(new EnrollmentData
                {
                    StudentId = (float)studentId.Id.GetHashCode(),
                    ClassId = (float)classId.GetHashCode(),
                    Rating = 1.0f // Binary: enrolled = 1, not enrolled = 0
                });
            }
        }

        if (enrollmentData.Count == 0)
        {
            return;
        }

        var dataView = _mlContext.Data.LoadFromEnumerable(enrollmentData);
        
        // Split data
        var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
        
        // Transform data and configure ALS (Matrix Factorization) trainer
        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(
                outputColumnName: "StudentIdEncoded",
                inputColumnName: "StudentId")
            .Append(_mlContext.Transforms.Conversion.MapValueToKey(
                outputColumnName: "ClassIdEncoded",
                inputColumnName: "ClassId"))
            .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(
                labelColumnName: "Rating",
                matrixColumnIndexColumnName: "StudentIdEncoded",
                matrixRowIndexColumnName: "ClassIdEncoded",
                numberOfIterations: 20,
                approximationRank: 8,
                learningRate: 0.1));

        lock (_lockObject)
        {
            _model = pipeline.Fit(split.TrainSet);
        }
    }

    private async Task<List<Guid>> FindSimilarStudentsAsync(
        Guid studentId, 
        List<OrleansCourse.Abstractions.Models.Student> allStudents,
        List<Guid> enrolledClassIds)
    {
        var similarStudents = new List<Guid>();
        
        foreach (var otherStudent in allStudents.Where(s => s.Id != studentId))
        {
            var otherGrain = client.GetGrain<IStudentGrain>(otherStudent.Id);
            var otherEnrolled = await otherGrain.GetEnrolledClassIds();
            
            // Calculate Jaccard similarity
            var intersection = enrolledClassIds.Intersect(otherEnrolled).Count();
            var union = enrolledClassIds.Union(otherEnrolled).Count();
            var similarity = union > 0 ? (double)intersection / union : 0;
            
            if (similarity > 0.3) // Threshold for similarity
            {
                similarStudents.Add(otherStudent.Id);
            }
        }
        
        return similarStudents;
    }

    private async Task<double> CalculateRecommendationScore(
        Guid studentId,
        Guid classId,
        List<Guid> similarStudents,
        List<OrleansCourse.Abstractions.Models.Student> allStudents)
    {
        if (similarStudents.Count == 0) return 0;

        var score = 0.0;
        var count = 0;

        foreach (var similarStudentId in similarStudents)
        {
            var similarGrain = client.GetGrain<IStudentGrain>(similarStudentId);
            var enrolled = await similarGrain.GetEnrolledClassIds();
            
            if (enrolled.Contains(classId))
            {
                score += 1.0;
                count++;
            }
        }

        return count > 0 ? score / similarStudents.Count : 0;
    }

    // Data models for ML.NET
    private class EnrollmentData
    {
        public float StudentId { get; set; }
        public float ClassId { get; set; }
        public float Rating { get; set; }
    }

    private class ClassPrediction
    {
        public float Score { get; set; }
    }
}

