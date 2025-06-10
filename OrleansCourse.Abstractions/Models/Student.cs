namespace OrleansCourse.Abstractions.Models;

using Orleans;
using Orleans.CodeGeneration;
using System;
using System.Collections.Generic;

[GenerateSerializer, Immutable]
public class Student
{
    [Id(0)] public Guid Id { get; set; }
    [Id(1)] public string FirstName { get; set; } = string.Empty;

    [Id(2)] public string LastName { get; set; } = string.Empty;

    [Id(3)] public string Email { get; set; } = string.Empty;

    [Id(4)] public string PhoneNumber { get; set; } = string.Empty;

    [Id(5)] public DateTime DateOfBirth { get; set; }

    [Id(6)] public string Address { get; set; } = string.Empty;

    [Id(7)] public List<Guid> EnrolledClassIds { get; set; } = new List<Guid>();

    public string FullName => $"{FirstName} {LastName}";
}