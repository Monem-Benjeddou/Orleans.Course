using OrleansCourse.App.Models;
using OrleansCourse.App.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace OrleansCourse.App.Pages;

public partial class ClassDetails
{
    [Parameter] public Guid ClassId { get; set; }
    
    private Class? _class;
    private List<Student> _enrolledStudents = new List<Student>();
    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] public IClassService ClassService { get; set; } = null!;
    [Inject] public IStudentService StudentService { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
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
        await LoadClassData();
    }
    
    private async Task LoadClassData()
    {
        _class = await ClassService.GetClassAsync(ClassId);
        
        if (_class != null)
        {
            _enrolledStudents.Clear();
            foreach (var studentId in _class.EnrolledStudentIds)
            {
                var student = await StudentService.GetStudentAsync(studentId);
                if (student != null)
                {
                    _enrolledStudents.Add(student);
                }
            }
        }
    }
    
    private void NavigateToClasses()
    {
        NavigationManager.NavigateTo("/Classes");
    }
    
    private void NavigateToEditClass(Guid classId)
    {
        NavigationManager.NavigateTo($"/Classes/Edit/{classId}");
    }
    
    private void NavigateToAddStudent(Guid classId)
    {
        NavigationManager.NavigateTo($"/Classes/{classId}/AddStudent");
    }
    
    private void NavigateToStudentDetails(Guid studentId)
    {
        NavigationManager.NavigateTo($"/Students/{studentId}");
    }
    
    private async Task DeleteClass()
    {
        if (_class != null)
        {
            await ClassService.DeleteClassAsync(_userId, _class.Id);
            NavigateToClasses();
        }
    }
    
    private async Task RemoveStudentFromClass(Guid studentId)
    {
        if (_class != null)
        {
            await StudentService.UnenrollStudentFromClassAsync(studentId, _class.Id);
            await LoadClassData();
        }
    }
}