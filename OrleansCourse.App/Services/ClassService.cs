using AutoMapper;
using Orleans;
using OrleansCourse.Abstractions;
using OrleansCourse.App.Models;
using OrleansCourse.App.Services.Interfaces;

namespace OrleansCourse.App.Services;

public class ClassService(IClusterClient client, IMapper mapper) : IClassService
{
    public async Task<List<Class>> GetAllUserClassesAsync(Guid userId)
    {
        var userGrain = client.GetGrain<IUserGrain>(userId);
        var classIds = await userGrain.GetClassIds();

        var classes = new List<Class>();
        foreach (var classId in classIds)
        {
            var classGrain = client.GetGrain<IClassGrain>(classId);
            var classData = await classGrain.GetClass();
            if (classData != null)
            {
                classes.Add(mapper.Map<Class>(classData));
            }
        }

        return classes;
    }
    public async Task<List<Class>> GetAllClassesAsync()
    {
        var registry = client.GetGrain<IClassRegistryGrain>(0);
        var allClassIds = await registry.GetAllClassIds();

        var classes = new List<Class>();
        foreach (var classId in allClassIds)
        {
            var classGrain = client.GetGrain<IClassGrain>(classId);
            var classData = await classGrain.GetClass();
            if (classData != null)
            {
                classes.Add(mapper.Map<Class>(classData));
            }
        }

        return classes;
    }
    public async Task<List<Class>> GetAllClassesAsync(Guid userId)
    {
        var registry = client.GetGrain<IUserGrain>(userId);
        var allClassIds = await registry.GetClassIds();

        var classes = new List<Class>();
        foreach (var classId in allClassIds)
        {
            var classGrain = client.GetGrain<IClassGrain>(classId);
            var classData = await classGrain.GetClass();
            if (classData != null)
            {
                classes.Add(mapper.Map<Class>(classData));
            }
        }

        return classes;
    }
    public async Task<bool> CreateClassAsync(Class classObj)
    {
        var grain = client.GetGrain<IClassGrain>(classObj.Id);
        var mappedClass = mapper.Map<OrleansCourse.Abstractions.Models.Class>(classObj);

        var success = await grain.SetClass(mappedClass);
        if (success)
        {
            var registry = client.GetGrain<IClassRegistryGrain>(0);
            await registry.AddClassId(classObj.Id);
        }
        return success;
    }


    public async Task<bool> DeleteClassAsync(Guid userId, Guid classId)
    {
        var userGrain = client.GetGrain<IUserGrain>(userId);
        var removedFromUser = await userGrain.RemoveClass(classId);

        var classGrain = client.GetGrain<IClassGrain>(classId);
        // You might want to also clear data on class grain if needed.

        var registry = client.GetGrain<IClassRegistryGrain>(0);
        await registry.RemoveClassId(classId);

        return removedFromUser;
    }
    
    public async Task<Class?> GetClassAsync(Guid id)
    {
        var grain = client.GetGrain<IClassGrain>(id);
        var classModel = await grain.GetClass();
        return classModel is null ? null : mapper.Map<Class>(classModel);
    }

    public async Task<List<Class>> GetClassesByCategoryAsync(ClassCategory category)
    {
        // Orleans doesn't support querying by default
        // You may implement a registry grain for indexing later
        return new List<Class>();
    }

    public async Task<bool> UpdateClassAsync(Class classObj)
    {
        var grain = client.GetGrain<IClassGrain>(classObj.Id);
        var mappedClass = mapper.Map<OrleansCourse.Abstractions.Models.Class>(classObj);
        return await grain.UpdateClass(mappedClass);
    }

    public async Task<List<Student>> GetEnrolledStudentsAsync(Guid classId)
    {
        var classGrain = client.GetGrain<IClassGrain>(classId);
        var studentIds = await classGrain.GetEnrolledStudents();
        var students = new List<Student>();

        foreach (var studentId in studentIds)
        {
            var studentGrain = client.GetGrain<IStudentGrain>(studentId);
            var student = await studentGrain.GetStudent();
            if (student != null)
            {
                students.Add(mapper.Map<Student>(student));
            }
        }
        return students;
    }

    public async Task<bool> AddClassToUserAsync(Guid userId, Guid classId)
    {
        var userGrain = client.GetGrain<IUserGrain>(userId);

        await userGrain.AddClass(classId);

        return true;
    }
}
