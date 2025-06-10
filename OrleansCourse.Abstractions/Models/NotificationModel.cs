namespace OrleansCourse.Abstractions.Models;

[GenerateSerializer, Immutable]
public class NotificationModel
{
    [Id(0)]
    public string Title { get; set; } = string.Empty;

    [Id(1)]
    public string Message { get; set; } = string.Empty;
}