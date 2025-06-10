using OrleansCourse.Abstractions;
using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Grains;

public class NotificationGrain : Grain<NotificationModel>, INotificationGrain
{
    public Task<NotificationModel?> GetAsync() => Task.FromResult(State);

    public async Task SetAsync(NotificationModel notification)
    {
        State = notification;
        await WriteStateAsync();
    }

    public async Task DeleteAsync()
    {
        await ClearStateAsync();
    }
}