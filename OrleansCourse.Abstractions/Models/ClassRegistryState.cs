namespace OrleansCourse.Abstractions.Models;

[GenerateSerializer]
public class ClassRegistryState
{
    [Id(0)]
    public List<Guid> ClassIds { get; set; } = new();
}