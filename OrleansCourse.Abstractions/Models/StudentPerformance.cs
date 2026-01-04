namespace OrleansCourse.Abstractions.Models;

using Orleans;
using Orleans.CodeGeneration;
using System;
using System.Collections.Generic;

[GenerateSerializer, Immutable]
public class StudentPerformance
{
    [Id(0)] public Guid StudentId { get; set; }
    [Id(1)] public Guid ClassId { get; set; }
    [Id(2)] public double CurrentAverage { get; set; }
    [Id(3)] public double PredictedFinalGrade { get; set; }
    [Id(4)] public bool IsAtRisk { get; set; }
    [Id(5)] public double AtRiskProbability { get; set; }
    [Id(6)] public List<Guid> GradeIds { get; set; } = new List<Guid>();
    [Id(7)] public int AttendanceRate { get; set; } // Percentage
    [Id(8)] public int AssignmentCompletionRate { get; set; } // Percentage
    [Id(9)] public DateTime LastUpdated { get; set; }
}

