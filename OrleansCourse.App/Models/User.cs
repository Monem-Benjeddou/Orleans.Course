namespace OrleansCourse.App.Models;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Guid> ClassIds { get; set; } = new();
}