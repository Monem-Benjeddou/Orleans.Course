using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using OrleansCourse.App.Models;
using OrleansCourse.App.Services.Interfaces;

namespace OrleansCourse.App.Pages;

partial class MyClasses : ComponentBase
{
    private List<Class> _classes = new();
    private Guid _userId = Guid.Empty;

    [Inject] public IClassService ClassService { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;
    [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        AuthenticationState state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = state.User;
        var userId = user.FindFirst(c => c.Type.Contains("nameidentifier"))?.Value;
        if (!userId.IsNullOrEmpty())
        {
            _userId = Guid.Parse(userId!);
            _classes = await ClassService.GetAllClassesAsync(_userId);

        }
    }

    private void NavigateToClassDetails(Guid classId)
    {
        NavigationManager.NavigateTo($"Classes/{classId}");
    }
}

