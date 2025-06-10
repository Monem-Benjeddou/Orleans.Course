using Microsoft.Extensions.Logging;
using OrleansCourse.Abstractions;
using OrleansCourse.Abstractions.Models;

namespace OrleansCourse.Silo.Startup;

public class StartupSeeder(IGrainFactory grainFactory, ILogger<StartupSeeder> logger) : IStartupTask
{
    private readonly List<Guid> _studentIds = new();
    private readonly List<Guid> _classIds = new();

    public async Task Execute(CancellationToken cancellationToken)
    {
        logger.LogInformation("Seeding Orleans grains...");

        await SeedStudents();
        await SeedClasses();
        await EnrollStudentsInClasses();

        logger.LogInformation("Seeding complete.");
    }

    private async Task SeedStudents()
    {
        var registryGrain = grainFactory.GetGrain<IStudentRegistryGrain>(0);

        for (int i = 1; i <= 5; i++)
        {
            var id = Guid.NewGuid();
            _studentIds.Add(id);

            var student = new Student
            {
                Id = id,
                FirstName = $"Student{i}",
                LastName = "Test",
                Email = $"student{i}@school.com",
                PhoneNumber = "123-456-7890",
                Address = $"123 Street {i}",
                DateOfBirth = DateTime.UtcNow.AddYears(-20).AddDays(i),
            };

            await registryGrain.RegisterStudent(student);
        }
    }

    private async Task SeedClasses()
    {
        var registryGrain = grainFactory.GetGrain<IClassRegistryGrain>(0);

        for (int i = 1; i <= 4; i++)
        {
            var id = Guid.NewGuid();
            _classIds.Add(id);

            var grain = grainFactory.GetGrain<IClassGrain>(id);

            var classItem = new Class
            {
                Id = id,
                Name = $"Class {i}",
                Description = $"This is the description for Class {i}",
                InstructorName = $"Instructor {i}",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                MaxCapacity = 30,
                CurrentEnrollment = 0,
                IsActive = true,
                Category = ClassCategory.ComputerScience
            };

            await grain.SetClass(classItem);

            await registryGrain.AddClassId(id);
        }
    }

    private async Task EnrollStudentsInClasses()
    {
        foreach (var studentId in _studentIds)
        {
            foreach (var classId in _classIds)
            {
                var classGrain = grainFactory.GetGrain<IClassGrain>(classId);
                await classGrain.EnrollStudent(studentId); 
                var studentGrain = grainFactory.GetGrain<IStudentGrain>(studentId);
                await studentGrain.EnrollInClass(classId); 
            }
        }
    }
}
