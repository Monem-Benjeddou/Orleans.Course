using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Orleans;
using OrleansCourse.Abstractions;
using OrleansCourse.App.Services.Interfaces;

namespace OrleansCourse.App.Services;

public class AtRiskIdentificationService(IClusterClient client, IGradeService gradeService) 
    : IAtRiskIdentificationService
{
    private MLContext? _mlContext;
    private ITransformer? _model;
    private readonly object _lockObject = new();

    public async Task<(bool IsAtRisk, double Probability)> IdentifyAtRiskStudentAsync(Guid studentId, Guid classId)
    {
        // Ensure model is trained
        if (_model == null)
        {
            await TrainAtRiskModelAsync();
        }

        if (_model == null || _mlContext == null)
        {
            // Fallback: simple heuristic
            var performance = await gradeService.GetStudentPerformanceAsync(studentId, classId);
            var isAtRisk = performance.CurrentAverage < 60 || performance.AttendanceRate < 70;
            return (isAtRisk, isAtRisk ? 0.7 : 0.3);
        }

        // Get current performance data
        var performance = await gradeService.GetStudentPerformanceAsync(studentId, classId);
        var grades = await gradeService.GetStudentGradesAsync(studentId, classId);
        
        if (grades.Count == 0)
        {
            return (false, 0.0);
        }

        // Prepare features for prediction
        var features = new AtRiskData
        {
            CurrentAverage = (float)performance.CurrentAverage,
            AssignmentCount = grades.Count,
            AttendanceRate = performance.AttendanceRate,
            AssignmentCompletionRate = performance.AssignmentCompletionRate,
            RecentGradeTrend = CalculateGradeTrend(grades),
            DaysSinceLastAssignment = CalculateDaysSinceLastAssignment(grades),
            BelowAverageAssignments = (float)(grades.Count(g => g.Percentage < 60) / (double)Math.Max(1, grades.Count))
        };

        // Create prediction engine
        PredictionEngine<AtRiskData, AtRiskPrediction>? predictionEngine;
        lock (_lockObject)
        {
            predictionEngine = _mlContext.Model.CreatePredictionEngine<AtRiskData, AtRiskPrediction>(_model);
        }

        if (predictionEngine == null)
        {
            var isAtRisk = performance.CurrentAverage < 60;
            return (isAtRisk, isAtRisk ? 0.7 : 0.3);
        }

        var prediction = predictionEngine.Predict(features);
        var probability = prediction.Probability;
        var isAtRiskResult = probability > 0.5; // Threshold for at-risk

        return (isAtRiskResult, probability);
    }

    public async Task<List<(Guid StudentId, double RiskProbability)>> GetAtRiskStudentsAsync(Guid classId)
    {
        var classGrain = client.GetGrain<IClassGrain>(classId);
        var enrolledStudentIds = await classGrain.GetEnrolledStudents();
        
        var atRiskStudents = new List<(Guid StudentId, double RiskProbability)>();
        
        foreach (var studentId in enrolledStudentIds)
        {
            var (isAtRisk, probability) = await IdentifyAtRiskStudentAsync(studentId, classId);
            if (isAtRisk)
            {
                atRiskStudents.Add((studentId, probability));
            }
        }
        
        return atRiskStudents.OrderByDescending(s => s.RiskProbability).ToList();
    }

    public async Task TrainAtRiskModelAsync()
    {
        _mlContext = new MLContext(seed: 0);

        // Collect training data from all students
        var registryGrain = client.GetGrain<IStudentRegistryGrain>(0);
        var allStudentIds = await registryGrain.GetAllStudents();
        
        var trainingData = new List<AtRiskData>();
        
        foreach (var studentId in allStudentIds)
        {
            var studentGrain = client.GetGrain<IStudentGrain>(studentId.Id);
            var enrolledClassIds = await studentGrain.GetEnrolledClassIds();
            
            foreach (var classId in enrolledClassIds)
            {
                var grades = await gradeService.GetStudentGradesAsync(studentId.Id, classId);
                if (grades.Count < 2) continue;
                
                var performance = await gradeService.GetStudentPerformanceAsync(studentId.Id, classId);
                
                // Determine if student was at-risk (label)
                var finalGrade = grades.Where(g => g.AssignmentType == "Final").FirstOrDefault();
                var isAtRisk = finalGrade != null && finalGrade.Percentage < 60 || 
                              performance.CurrentAverage < 60 ||
                              performance.AttendanceRate < 70;
                
                var features = new AtRiskData
                {
                    CurrentAverage = (float)performance.CurrentAverage,
                    AssignmentCount = grades.Count,
                    AttendanceRate = performance.AttendanceRate,
                    AssignmentCompletionRate = performance.AssignmentCompletionRate,
                    RecentGradeTrend = CalculateGradeTrend(grades),
                    DaysSinceLastAssignment = CalculateDaysSinceLastAssignment(grades),
                    BelowAverageAssignments = (float)(grades.Count(g => g.Percentage < 60) / (double)Math.Max(1, grades.Count)),
                    IsAtRisk = isAtRisk // Label
                };
                
                trainingData.Add(features);
            }
        }

        if (trainingData.Count < 10) // Need minimum data
        {
            return;
        }

        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);
        
        // Split data
        var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
        
        // Configure Logistic Regression trainer
        var pipeline = _mlContext.Transforms.Concatenate(
                "Features",
                nameof(AtRiskData.CurrentAverage),
                nameof(AtRiskData.AssignmentCount),
                nameof(AtRiskData.AttendanceRate),
                nameof(AtRiskData.AssignmentCompletionRate),
                nameof(AtRiskData.RecentGradeTrend),
                nameof(AtRiskData.DaysSinceLastAssignment),
                nameof(AtRiskData.BelowAverageAssignments))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(AtRiskData.IsAtRisk)))
            .Append(_mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression(
                new LbfgsLogisticRegressionBinaryTrainer.Options
                {
                    LabelColumnName = "Label",
                    FeatureColumnName = "Features",
                    MaximumNumberOfIterations = 100
                }));

        lock (_lockObject)
        {
            _model = pipeline.Fit(split.TrainSet);
        }
    }

    private static float CalculateGradeTrend(List<OrleansCourse.Abstractions.Models.Grade> grades)
    {
        if (grades.Count < 2) return 0;
        
        var sortedGrades = grades.OrderBy(g => g.DateRecorded).ToList();
        var recentGrades = sortedGrades.TakeLast(Math.Min(3, sortedGrades.Count)).ToList();
        var olderGrades = sortedGrades.Take(Math.Max(0, sortedGrades.Count - recentGrades.Count)).ToList();
        
        if (olderGrades.Count == 0) return 0;
        
        var recentAvg = recentGrades.Average(g => g.Percentage);
        var olderAvg = olderGrades.Average(g => g.Percentage);
        
        return (float)(recentAvg - olderAvg); // Positive = improving, Negative = declining
    }

    private static int CalculateDaysSinceLastAssignment(List<OrleansCourse.Abstractions.Models.Grade> grades)
    {
        if (grades.Count == 0) return 999;
        
        var lastAssignment = grades.Max(g => g.DateRecorded);
        return (DateTime.UtcNow - lastAssignment).Days;
    }

    // Data models for ML.NET
    private class AtRiskData
    {
        public float CurrentAverage { get; set; }
        public int AssignmentCount { get; set; }
        public int AttendanceRate { get; set; }
        public int AssignmentCompletionRate { get; set; }
        public float RecentGradeTrend { get; set; }
        public int DaysSinceLastAssignment { get; set; }
        public float BelowAverageAssignments { get; set; }
        public bool IsAtRisk { get; set; } // Label
    }

    private class AtRiskPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool IsAtRisk { get; set; }
        
        [ColumnName("Probability")]
        public float Probability { get; set; }
        
        [ColumnName("Score")]
        public float Score { get; set; }
    }
}

