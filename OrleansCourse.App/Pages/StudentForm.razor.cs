using OrleansCourse.App.Models;
using OrleansCourse.App.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace OrleansCourse.App.Pages;

public partial class StudentForm
{
    [Parameter] public Guid? StudentId { get; set; }
    
    private Student _student = new Student();
    private bool _isEditMode => StudentId.HasValue;
    
    [Inject] public IStudentService StudentService { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    
    protected override async Task OnInitializedAsync()
    {
        if (_isEditMode && StudentId.HasValue)
        {
            var existingStudent = await StudentService.GetStudentAsync(StudentId.Value);
            if (existingStudent != null)
            {
                _student = existingStudent;
            }
        }
        else
        {
            // Set default values for new student
            _student = new Student
            {
                Id = Guid.NewGuid(),
                DateOfBirth = DateTime.Today.AddYears(-18) // Default to 18 years old
            };
        }
    }
    
    private async Task HandleValidSubmit()
    {
        bool success;
        
        if (_isEditMode)
        {
            success = await StudentService.UpdateStudentAsync(_student);
        }
        else
        {
            success = await StudentService.CreateStudentAsync(_student);
        }
        
        if (success)
        {
            NavigateToStudents();
        }
    }
    
    private void NavigateToStudents()
    {
        NavigationManager.NavigateTo("/");
    }
}