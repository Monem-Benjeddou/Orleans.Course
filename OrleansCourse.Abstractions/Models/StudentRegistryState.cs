namespace OrleansCourse.Abstractions.Models;

[GenerateSerializer]
public class StudentRegistryState
{
    [Id(0)]
    public List<Guid> StudentIds { get; set; } = new();
}