using AutoMapper;
using OrleansCourse.Abstractions;
using OrleansCourse.App.Models;
using OrleansCourse.App.Services.Interfaces;

namespace OrleansCourse.App.Services;

public class StudentService(IGrainFactory grainFactory, IClassService classService, IMapper mapper)
    : IStudentService
{
    private readonly IClassService _classService = classService;

    public async Task<List<Student>> GetAllStudentsAsync()
    {
        var registryGrain = grainFactory.GetGrain<IStudentRegistryGrain>(0);
        var studentIds = await registryGrain.GetAllStudents();

        var tasks = studentIds.Select( id =>
             grainFactory.GetGrain<IStudentGrain>(id.Id).GetStudent());
        var results = await Task.WhenAll(tasks);
        var mappedStudents = mapper.Map<List<Student>>(results);
        return mappedStudents;
    }

    public async Task<Student?> GetStudentAsync(Guid id)
    {
        var studentGrain = grainFactory.GetGrain<IStudentGrain>(id);
        var student = await studentGrain.GetStudent();
        var mappedStudent = mapper.Map<Student>(student);
        return mappedStudent;
    }

    public async Task<bool> CreateStudentAsync(Student student)
    {
        var registryGrain = grainFactory.GetGrain<IStudentRegistryGrain>(0);
        var mappedStudent = mapper.Map<OrleansCourse.Abstractions.Models.Student>(student);
        await registryGrain.RegisterStudent(mappedStudent);
        return true;
    }

    public async Task<bool> UpdateStudentAsync(Student student)
    {
        var studentGrain = grainFactory.GetGrain<IStudentGrain>(student.Id);
        var mappedStudent = mapper.Map<OrleansCourse.Abstractions.Models.Student>(student);
        await studentGrain.UpdateStudent(mappedStudent);
        return true;
    }

    public async Task<bool> DeleteStudentAsync(Guid id)
    {
        var registryGrain = grainFactory.GetGrain<IStudentRegistryGrain>(0);
        await registryGrain.RemoveStudent(id);
        

        return true;
    }

    public async Task<bool> EnrollStudentInClassAsync(Guid studentId, Guid classId)
    {
        var studentGrain = grainFactory.GetGrain<IStudentGrain>(studentId);
        var classGrain = grainFactory.GetGrain<IClassGrain>(classId);

        var student = await studentGrain.GetStudent();
        var classObj = await classGrain.GetClass();

        if (student == null || classObj == null || classObj.CurrentEnrollment >= classObj.MaxCapacity)
            return false;

        await studentGrain.EnrollInClass(classId);
        await classGrain.EnrollStudent(studentId);

        return true;
    }

    public async Task<bool> UnenrollStudentFromClassAsync(Guid studentId, Guid classId)
    {
        var studentGrain = grainFactory.GetGrain<IStudentGrain>(studentId);
        var classGrain = grainFactory.GetGrain<IClassGrain>(classId);

        await studentGrain.UnenrollFromClass(classId);
        await classGrain.UnenrollStudent(studentId);

        return true;
    }
}
