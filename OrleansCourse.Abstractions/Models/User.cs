namespace OrleansCourse.Abstractions.Models;
[GenerateSerializer, Immutable]
public class User
{
    [Id(0)]  public Guid Id { get; set; }
    [Id(1)]public string Name { get; set; } = string.Empty;
    [Id(2)]public List<Guid> ClassIds { get; set; } = new();
}
