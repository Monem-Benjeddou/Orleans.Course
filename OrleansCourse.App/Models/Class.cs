namespace OrleansCourse.App.Models;

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

    public List<Guid> EnrolledStudentIds { get; set; } = [];
}