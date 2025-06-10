using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Abstractions;

public interface INotificationGrain : IGrainWithGuidKey
{
    Task<NotificationModel?> GetAsync();
    Task SetAsync(NotificationModel notification);
    Task DeleteAsync();
}