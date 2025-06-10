namespace OrleansCourse.App.Models;

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