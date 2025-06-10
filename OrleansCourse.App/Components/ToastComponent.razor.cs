using OrleansCourse.App.Models;
using Microsoft.AspNetCore.Components;

namespace OrleansCourse.App.Components;

public partial class ToastComponent
{
    [Parameter]
    public NotificationModel Notification { get; set; } = default!;

    [Parameter] public EventCallback<NotificationModel> OnClick { get; set; }
}