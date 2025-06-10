using OrleansCourse.App.Models;
using OrleansCourse.App.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace OrleansCourse.App.Pages;

public partial class StudentDetails
{
    [Parameter] public Guid StudentId { get; set; }
    
    private Student? _student;
    private List<Class> _enrolledClasses = new List<Class>();
    
    [Inject] public IStudentService StudentService { get; set; } = null!;
    [Inject] public IClassService ClassService { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadStudentData();
    }
    
    private async Task LoadStudentData()
    {
        _student = await StudentService.GetStudentAsync(StudentId);
        
        if (_student != null)
        {
            _enrolledClasses.Clear();
            foreach (var classId in _student.EnrolledClassIds)
            {
                var classObj = await ClassService.GetClassAsync(classId);
                if (classObj != null)
                {
                    _enrolledClasses.Add(classObj);
                }
            }
        }
    }
    
    private void NavigateToStudents()
    {
        NavigationManager.NavigateTo("/");
    }
    
    private void NavigateToEditStudent(Guid studentId)
    {
        NavigationManager.NavigateTo($"/Students/Edit/{studentId}");
    }
    
    private void NavigateToEnrollClass(Guid studentId)
    {
        NavigationManager.NavigateTo($"/Students/{studentId}/Enroll");
    }
    
    private async Task DeleteStudent()
    {
        if (_student != null)
        {
            await StudentService.DeleteStudentAsync(_student.Id);
            NavigateToStudents();
        }
    }
    
    private async Task UnenrollFromClass(Guid classId)
    {
        if (_student != null)
        {
            await StudentService.UnenrollStudentFromClassAsync(_student.Id, classId);
            await LoadStudentData();
        }
    }
}