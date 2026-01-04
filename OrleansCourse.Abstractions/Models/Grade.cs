namespace OrleansCourse.Abstractions.Models;

using Orleans;
using Orleans.CodeGeneration;
using System;

[GenerateSerializer, Immutable]
public class Grade
{
    [Id(0)] public Guid Id { get; set; }
    [Id(1)] public Guid StudentId { get; set; }
    [Id(2)] public Guid ClassId { get; set; }
    [Id(3)] public double Score { get; set; }
    [Id(4)] public double MaxScore { get; set; }
    [Id(5)] public string AssignmentType { get; set; } = string.Empty; // "Homework", "Quiz", "Midterm", "Final", etc.
    [Id(6)] public DateTime DateRecorded { get; set; }
    [Id(7)] public int SemesterWeek { get; set; } // Week number in the semester
    
    public double Percentage => MaxScore > 0 ? (Score / MaxScore) * 100 : 0;
}

