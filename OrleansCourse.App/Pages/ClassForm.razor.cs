using OrleansCourse.App.Models;
using OrleansCourse.App.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace OrleansCourse.App.Pages;

public partial class ClassForm
{
    [Parameter] public Guid? ClassId { get; set; }
    
    private Class _class = new Class();
    private bool _isEditMode => ClassId.HasValue;
    
    [Inject] public IClassService ClassService { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    
    protected override async Task OnInitializedAsync()
    {
        if (_isEditMode && ClassId.HasValue)
        {
            var existingClass = await ClassService.GetClassAsync(ClassId.Value);
            if (existingClass != null)
            {
                _class = existingClass;
            }
        }
        else
        {
            // Set default values for new class
            _class = new Class
            {
                Id = Guid.NewGuid(),
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(3),
                MaxCapacity = 30,
                IsActive = true
            };
        }
    }
    
    private async Task HandleValidSubmit()
    {
        bool success;
        
        if (_isEditMode)
        {
            success = await ClassService.UpdateClassAsync(_class);
        }
        else
        {
            success = await ClassService.CreateClassAsync(_class);
        }
        
        if (success)
        {
            NavigateToClasses();
        }
    }
    
    private void NavigateToClasses()
    {
        NavigationManager.NavigateTo("/Classes");
    }
}