namespace OrleansCourse.Abstractions.Models;

using Orleans;
using Orleans.CodeGeneration;

[GenerateSerializer, Immutable]
public class ClassRecommendation
{
    [Id(0)] public Guid ClassId { get; set; }
    [Id(1)] public string ClassName { get; set; } = string.Empty;
    [Id(2)] public double RecommendationScore { get; set; } // 0-1, higher is better
    [Id(3)] public string Reason { get; set; } = string.Empty; // Why this class is recommended
}

