using OrleansCourse.App.Models;
using OrleansCourse.App.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace OrleansCourse.App.Pages;

public partial class Classes
{
    private List<Class> _classes = new List<Class>();
    
    private List<string> _categories = new List<string>();
    
    [Inject] public IClassService ClassService { get; set; } = null!;
    
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    private Guid _userId = Guid.Empty;

    
    protected override async Task OnInitializedAsync()
    {
        AuthenticationState state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = state.User;
        var userId = user.FindFirst(c => c.Type.Contains("nameidentifier"))?.Value;
        if (!userId.IsNullOrEmpty())
        {
            _userId = Guid.Parse(userId!);
        }
        _classes = await ClassService.GetAllClassesAsync();
        
        _categories = Enum.GetNames(typeof(ClassCategory)).ToList();
    }
    
    private async Task CategorySelected(ChangeEventArgs e)
    {
        string selectedCategory = e.Value?.ToString() ?? string.Empty;
        if (selectedCategory.Equals("All") || selectedCategory == string.Empty)
        {
            _classes = await ClassService.GetAllClassesAsync();
        }
        else
        {
            _classes = await ClassService.GetClassesByCategoryAsync(Enum.Parse<ClassCategory>(selectedCategory));
        }
    }
    
    private void NavigateToClassDetails(Guid classId)
    {
        NavigationManager.NavigateTo($"Classes/{classId}");
    }
    
    private void NavigateToCreateClass()
    {
        NavigationManager.NavigateTo("Classes/Create");
    }
    
    private void NavigateToEditClass(Guid classId)
    {
        NavigationManager.NavigateTo($"Classes/Edit/{classId}");
    }
    
    private async Task DeleteClass(Guid classId)
    {
         
        if (await ClassService.DeleteClassAsync(_userId, classId))
        {
            _classes = await ClassService.GetAllClassesAsync();
        }
    }
    private async Task AssignClass(Guid classId)
    {
        if (await ClassService.AddClassToUserAsync(_userId, classId))
        {
            _classes = await ClassService.GetAllClassesAsync();
        }
    }
}