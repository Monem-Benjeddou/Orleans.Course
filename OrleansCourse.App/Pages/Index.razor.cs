using OrleansCourse.App.Models;
using OrleansCourse.App.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace OrleansCourse.App.Pages;

public partial class Index
{
    private List<Student> _students = new List<Student>();

    [Inject] public IStudentService StudentService { get; set; } = null!;

    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        _students = await StudentService.GetAllStudentsAsync();
    }

    private void NavigateToStudentDetails(Guid studentId)
    {
        NavigationManager.NavigateTo($"Students/{studentId}");
    }

    private void NavigateToCreateStudent()
    {
        NavigationManager.NavigateTo("Students/Create");
    }

    private void NavigateToEditStudent(Guid studentId)
    {
        NavigationManager.NavigateTo($"Students/Edit/{studentId}");
    }

    private async Task DeleteStudent(Guid studentId)
    {
        if (await StudentService.DeleteStudentAsync(studentId))
        {
            _students = await StudentService.GetAllStudentsAsync();
        }
    }
}