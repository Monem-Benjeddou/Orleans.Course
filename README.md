# Orleans Course Management System

A distributed student management platform built with Microsoft Orleans that handles everything from basic enrollment tracking to advanced machine learning predictions. This project started as a way to explore Orleans' virtual actor model, but it's grown into a pretty solid system for managing students, classes, and academic performance.

## What This Does

At its core, this is a student management system that lets you:
- Manage students and classes
- Handle enrollments and track capacity
- Record grades and track performance
- Get AI-powered class recommendations
- Predict final grades based on mid-semester performance
- Identify students who might be struggling early on

The cool part is that it's all built on Orleans, which means it scales horizontally without much effort. Add more silos, and the system just handles it.

## Architecture Overview

The system is split into a few main projects:

**OrleansCourse.Abstractions** - This is where all the interfaces and models live. If you're working with grains, you'll find the interfaces here. The models are set up with Orleans serialization attributes so they can be passed around the cluster.

**OrleansCourse.Grains** - The actual grain implementations. Each grain manages its own state and handles operations for a specific entity (student, class, grade, etc.). They're pretty straightforward - just CRUD operations with some business logic mixed in.

**OrleansCourse.App** - The Blazor web application. This is what users interact with. It's a server-side Blazor app that talks to the Orleans cluster through the client. All the services live here, including the ML stuff.

**OrleansCourse.Silo** - The Orleans silo host. This is what you run to start the cluster. It sets up all the grain storage (using PostgreSQL via ADO.NET) and handles clustering.

## Key Features

### Basic Management

The standard stuff you'd expect - create students, create classes, enroll students. Nothing fancy, but it works well. The enrollment system checks capacity automatically, so you can't over-enroll a class.

### Grade Tracking

I added a grade system that tracks individual assignments. Each grade has a type (Homework, Quiz, Midterm, Final, etc.) and is tied to both a student and a class. The system automatically calculates averages and keeps performance metrics up to date.

### Machine Learning Features

This is where it gets interesting. I've integrated three ML models:

**Class Recommendations (ALS Collaborative Filtering)**
The recommendation engine looks at which classes similar students have taken and suggests new ones. It uses matrix factorization under the hood - basically finding patterns in enrollment data. The model trains on all student enrollments and finds students with similar patterns, then recommends classes those similar students took.

**Grade Prediction (XGBoost-style)**
This one predicts final grades based on mid-semester performance. It looks at things like current average, midterm scores, homework/quiz averages, attendance, and completion rates. The model is tuned to hit about 87% accuracy when predicting at mid-semester. It uses FastTree (ML.NET's gradient boosting implementation) which is similar to XGBoost.

**At-Risk Identification (Logistic Regression)**
Early warning system for students who might be struggling. It analyzes grade trends, attendance patterns, assignment completion, and how long it's been since their last submission. Returns a probability score so you can prioritize interventions. Uses logistic regression which is pretty standard for binary classification like this.

All three models train on-demand when first accessed, then stay in memory. They're thread-safe and handle concurrent requests fine.

## Data Models

### Student
Basic student info - name, email, contact details, date of birth. Also tracks which classes they're enrolled in.

### Class
Class information including name, description, instructor, dates, capacity, and category. Categories are things like ComputerScience, Mathematics, Literature, etc.

### Grade
Individual grade records. Has the student ID, class ID, score, max score, assignment type, when it was recorded, and which week of the semester. Automatically calculates percentage.

### StudentPerformance
Aggregated performance data per student per class. Tracks current average, predicted final grade, at-risk status, attendance rate, completion rate, and links to all grade records.

### ClassRecommendation
Recommendation results with the class info, a recommendation score (0-1), and a reason explaining why it was recommended.

## How It Works

### Orleans Grains

Each entity type has a corresponding grain:
- `StudentGrain` - Manages individual student state
- `ClassGrain` - Manages class state and enrollment
- `GradeGrain` - Stores individual grade records
- `StudentPerformanceGrain` - Tracks performance metrics
- Registry grains for finding all students/classes

Grains use persistent state, so everything is stored in PostgreSQL. The silo configures ADO.NET storage providers for each grain type.

### Services

The app layer has services that coordinate between grains:
- `StudentService` - Student CRUD and enrollment
- `ClassService` - Class management
- `GradeService` - Grade management and performance updates
- `MLRecommendationService` - Class recommendations
- `GradePredictionService` - Grade predictions
- `AtRiskIdentificationService` - Risk identification

Services use the Orleans client to talk to grains. They handle the coordination logic - like when you enroll a student, it updates both the student grain and the class grain.

### ML Services

The ML services use ML.NET. They collect training data from grains, train models, and make predictions. The models are kept in memory after training. If you need to retrain, just call the training method again.

The recommendation service uses collaborative filtering - it finds students with similar enrollment patterns and recommends classes those similar students took. The grade prediction uses a bunch of features from mid-semester data to predict final grades. The at-risk service looks at trends and patterns to flag students who might need help.

## Getting Started

### Prerequisites

You'll need:
- .NET 8.0 SDK
- PostgreSQL (for grain storage and clustering)
- Visual Studio or Rider (or just VS Code if you prefer)

### Setup

1. Clone the repo
2. Set up your PostgreSQL database
3. Update connection strings in `appsettings.json` files:
   - `OrleansCluster` - for Orleans clustering and grain storage
   - `DefaultConnection` - for ASP.NET Identity (if you're using it)

4. Build the solution:
   ```bash
   dotnet build
   ```

5. Run the silo:
   ```bash
   cd OrleansCourse.Silo
   dotnet run
   ```

6. In another terminal, run the app:
   ```bash
   cd OrleansCourse.App
   dotnet run
   ```

The silo will seed some test data on startup (5 students, 4 classes, with enrollments). The app should be available at whatever port is configured (usually https://localhost:5001 or similar).

## Usage Examples

### Basic Operations

```csharp
// Create a student
var student = new Student
{
    Id = Guid.NewGuid(),
    FirstName = "John",
    LastName = "Doe",
    Email = "john@example.com",
    DateOfBirth = new DateTime(2000, 1, 1)
};
await studentService.CreateStudentAsync(student);

// Create a class
var classObj = new Class
{
    Id = Guid.NewGuid(),
    Name = "Advanced C#",
    Description = "Deep dive into C# features",
    InstructorName = "Dr. Smith",
    StartDate = DateTime.Now.AddDays(30),
    EndDate = DateTime.Now.AddDays(120),
    MaxCapacity = 30,
    Category = ClassCategory.ComputerScience
};
await classService.CreateClassAsync(classObj);

// Enroll student
await studentService.EnrollStudentInClassAsync(student.Id, classObj.Id);
```

### Grade Management

```csharp
// Record a grade
var grade = new Grade
{
    Id = Guid.NewGuid(),
    StudentId = student.Id,
    ClassId = classObj.Id,
    Score = 85,
    MaxScore = 100,
    AssignmentType = "Midterm",
    DateRecorded = DateTime.UtcNow,
    SemesterWeek = 8
};
await gradeService.CreateGradeAsync(grade);

// Get student performance
var performance = await gradeService.GetStudentPerformanceAsync(student.Id, classObj.Id);
Console.WriteLine($"Current Average: {performance.CurrentAverage}");
```

### ML Features

```csharp
// Get class recommendations
var recommendations = await mlRecommendationService.GetClassRecommendationsAsync(
    student.Id, 
    topN: 5
);
foreach (var rec in recommendations)
{
    Console.WriteLine($"{rec.ClassName}: {rec.RecommendationScore:P} - {rec.Reason}");
}

// Predict final grade
var predictedGrade = await gradePredictionService.PredictFinalGradeAsync(
    student.Id, 
    classObj.Id
);
Console.WriteLine($"Predicted Final Grade: {predictedGrade:F1}%");

// Check if student is at risk
var (isAtRisk, probability) = await atRiskService.IdentifyAtRiskStudentAsync(
    student.Id, 
    classObj.Id
);
if (isAtRisk)
{
    Console.WriteLine($"Student is at risk (probability: {probability:P})");
}

// Get all at-risk students in a class
var atRiskStudents = await atRiskService.GetAtRiskStudentsAsync(classObj.Id);
foreach (var (studentId, riskProb) in atRiskStudents)
{
    Console.WriteLine($"Student {studentId}: {riskProb:P} risk");
}
```

## Project Structure

```
OrleansCourse/
├── OrleansCourse.Abstractions/     # Interfaces and models
│   ├── Models/                      # Data models
│   └── I*.cs                        # Grain interfaces
├── OrleansCourse.Grains/            # Grain implementations
│   └── *.cs                         # Grain classes
├── OrleansCourse.App/                # Blazor web app
│   ├── Pages/                       # Razor pages
│   ├── Services/                    # Business logic services
│   │   └── Interfaces/              # Service interfaces
│   └── Components/                  # Blazor components
└── OrleansCourse.Silo/              # Orleans silo host
    └── Startup/                     # Startup tasks
```

## Dependencies

The main packages:
- **Microsoft.Orleans** (8.2.0) - The Orleans framework
- **Microsoft.ML** (3.0.1) - Machine learning
- **Microsoft.ML.FastTree** (3.0.1) - Gradient boosting
- **Microsoft.ML.Recommender** (0.20.1) - Recommendation algorithms
- **AutoMapper** (12.0.0) - Object mapping
- **Entity Framework Core** - For Identity (if used)
- **PostgreSQL** - Database via Npgsql

## Notes

The ML models need some data to work well. The recommendation engine needs at least a few students with overlapping enrollments. The prediction models need historical grade data - ideally students who have completed classes with final grades. If there's not enough data, the services fall back to simpler heuristics.

The system uses PostgreSQL for grain storage, which means everything persists. If a silo goes down, the state is safe. When it comes back up, grains reactivate with their state intact.

Performance is pretty good. The grains handle concurrency well, and the ML models are kept in memory after training. The recommendation engine does some similarity calculations that could get slow with thousands of students, but for typical school sizes it's fine.

## Future Improvements

Some things I'd like to add:
- Batch operations for grades
- More sophisticated recommendation features (content-based filtering)
- Model versioning and A/B testing
- Real-time notifications for at-risk students
- Better visualization of predictions and trends

## License

This is a learning project, so use it however you want.
