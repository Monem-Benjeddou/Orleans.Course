using System.Text.Json.Serialization;

namespace OrleansCourse.Abstractions.Models;
[GenerateSerializer, Immutable]
public class Class
{
    [Id(0)]
    public Guid Id { get; set; }
    [Id(1)]
    public string Name { get; set; } = string.Empty;
    [Id(2)]
    public string Description { get; set; } = string.Empty;
    [Id(3)]
    public string InstructorName { get; set; } = string.Empty;
    [Id(4)]
    public DateTime StartDate { get; set; }
    [Id(5)]
    public DateTime EndDate { get; set; }
    [Id(6)]
    public int MaxCapacity { get; set; }
    [Id(7)]
    public int CurrentEnrollment { get; set; }
    [Id(8)]
    public bool IsActive { get; set; } = true;
    [Id(9)]
    public ClassCategory Category { get; set; }
    [Id(10)]
    public List<Guid> EnrolledStudentIds { get; set; } = [];
}