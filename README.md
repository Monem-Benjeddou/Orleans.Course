# Orleans Course - Student Management Application

## Overview

This application is a student management system built using Microsoft Orleans, a virtual actor model framework. The system allows for managing students, classes, and enrollments with a distributed architecture that provides scalability and fault tolerance.

## Architecture

The application follows a distributed architecture using the Orleans virtual actor model:

- **Grains**: Core processing units that represent students and classes
- **Grain Interfaces**: Defined in the Abstractions project
- **Services**: Application services that interact with grains
- **Models**: Data models for the application

### Orleans Components

- **Grain Interfaces**: `IStudentGrain`, `IClassGrain`, `IStudentRegistryGrain`, `IClassRegistryGrain`
- **Client**: Orleans cluster client for connecting to the grain silo

## Data Models

### Student

Represents a student in the system:

```csharp
public class Student
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    public List<Guid> EnrolledClassIds { get; set; } = new List<Guid>();
    public string FullName => $"{FirstName} {LastName}";
}
```

### Class

Represents a class that students can enroll in:

```csharp
public class Class
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxCapacity { get; set; }
    public int CurrentEnrollment { get; set; }
    public bool IsActive { get; set; } = true;
    public ClassCategory Category { get; set; }
    public List<Guid> EnrolledStudentIds { get; set; } = new List<Guid>();
}
```

### ClassCategory

Enumeration of available class categories:

```csharp
public enum ClassCategory
{
    Mathematics,
    Science,
    Literature,
    History,
    ComputerScience,
    Languages,
    Arts,
    PhysicalEducation,
    SocialStudies,
    Engineering
}
```

## Services

### IStudentService

Interface for student management operations:

```csharp
public interface IStudentService
{
    Task<List<Student>> GetAllStudentsAsync();
    Task<Student?> GetStudentAsync(Guid id);
    Task<bool> CreateStudentAsync(Student student);
    Task<bool> UpdateStudentAsync(Student student);
    Task<bool> DeleteStudentAsync(Guid id);
    Task<bool> EnrollStudentInClassAsync(Guid studentId, Guid classId);
    Task<bool> UnenrollStudentFromClassAsync(Guid studentId, Guid classId);
}
```

### IClassService

Interface for class management operations:

```csharp
public interface IClassService
{
    Task<List<Class>> GetAllClassesAsync();
    Task<List<Class>> GetAllClassesAsync(Guid userId);
    Task<Class?> GetClassAsync(Guid id);
    Task<List<Class>> GetClassesByCategoryAsync(ClassCategory category);
    Task<bool> CreateClassAsync(Class classObj);
    Task<bool> UpdateClassAsync(Class classObj);
    Task<bool> DeleteClassAsync(Guid userId, Guid classId);
    Task<bool> AddClassToUserAsync(Guid userId, Guid classId);
    Task<List<Student>> GetEnrolledStudentsAsync(Guid classId);
}
```

## Usage Examples

### Managing Students

```csharp
// Create a new student
var student = new Student
{
    Id = Guid.NewGuid(),
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@example.com",
    DateOfBirth = new DateTime(1995, 5, 15)
};

await _studentService.CreateStudentAsync(student);

// Get a student by ID
var retrievedStudent = await _studentService.GetStudentAsync(student.Id);

// Update a student
retrievedStudent.PhoneNumber = "555-123-4567";
await _studentService.UpdateStudentAsync(retrievedStudent);

// Delete a student
await _studentService.DeleteStudentAsync(student.Id);
```

### Managing Classes

```csharp
// Create a new class
var classObj = new Class
{
    Id = Guid.NewGuid(),
    Name = "Introduction to Computer Science",
    Description = "Fundamentals of programming and computer science concepts",
    InstructorName = "Dr. Smith",
    StartDate = DateTime.Now.AddDays(30),
    EndDate = DateTime.Now.AddDays(120),
    MaxCapacity = 30,
    Category = ClassCategory.ComputerScience
};

await _classService.CreateClassAsync(classObj);

// Get all classes
var allClasses = await _classService.GetAllClassesAsync();

// Get classes by category
var csClasses = await _classService.GetClassesByCategoryAsync(ClassCategory.ComputerScience);

// Update a class
classObj.MaxCapacity = 35;
await _classService.UpdateClassAsync(classObj);
```

### Enrollment Management

```csharp
// Enroll a student in a class
await _studentService.EnrollStudentInClassAsync(studentId, classId);

// Unenroll a student from a class
await _studentService.UnenrollStudentFromClassAsync(studentId, classId);

// Get all students enrolled in a class
var enrolledStudents = await _classService.GetEnrolledStudentsAsync(classId);
```

## Orleans Grain Implementation

The application uses Orleans grains to manage state and operations:

### Student Grain

```csharp
public interface IStudentGrain : IGrainWithGuidKey
{
    Task UpdateStudent(Student updated);
    Task<Student> GetStudent();
    Task SetStudent(Student student);
    Task<List<Guid>> GetEnrolledClassIds();
    Task EnrollInClass(Guid classId);
    Task UnenrollFromClass(Guid classId);
}
```

### Class Grain

```csharp
public interface IClassGrain : IGrainWithGuidKey
{
    Task UnenrollStudent(Guid studentId);
    Task<Class> GetClass();
    Task<bool> SetClass(Class cls);
    Task EnrollStudent(Guid studentId);
    Task<List<Guid>> GetEnrolledStudents();
    Task<bool> UpdateClass(Class updatedClass);
}
```

## Benefits of Orleans Architecture

- **Scalability**: The application can scale horizontally by adding more silos
- **Fault Tolerance**: Orleans provides automatic recovery mechanisms
- **Concurrency**: Actor model simplifies concurrent programming
- **Distribution**: State is distributed across the cluster

## Getting Started

1. Clone the repository
2. Build the solution
3. Run the Orleans silo host
4. Run the client application

The application demonstrates how to build a distributed system using Orleans, providing a scalable and fault-tolerant architecture for managing students and classes.