using StudentAccounting.Dal.Entities;

namespace StudentAccounting.Dal.Storage;

/// <summary>
/// Контекст данных: хранит коллекции всех сущностей предметной области
/// в памяти процесса. Простейшая реализация слоя доступа к данным (DAL).
/// </summary>
public class UniversityContext
{
    public List<Faculty> Faculties { get; } = new();
    public List<Department> Departments { get; } = new();
    public List<Group> Groups { get; } = new();
    public List<Student> Students { get; } = new();
    public List<Teacher> Teachers { get; } = new();
    public List<Discipline> Disciplines { get; } = new();
    public List<Curriculum> Curriculums { get; } = new();
    public List<Grade> Grades { get; } = new();
    public List<Lesson> Lessons { get; } = new();
    public List<Attendance> Attendances { get; } = new();

    /// <summary>Создаёт контекст и заполняет его демонстрационными данными.</summary>
    public static UniversityContext CreateSeeded()
    {
        var ctx = new UniversityContext();

        var it = new Faculty { Id = 1, Name = "Факультет информационных технологий", ShortName = "ИТ" };
        var econ = new Faculty { Id = 2, Name = "Экономический факультет", ShortName = "ЭК" };
        ctx.Faculties.Add(it);
        ctx.Faculties.Add(econ);

        var dept1 = new Department { Id = 1, Name = "Кафедра программной инженерии", FacultyId = 1, Faculty = it };
        var dept2 = new Department { Id = 2, Name = "Кафедра прикладной математики", FacultyId = 1, Faculty = it };
        var dept3 = new Department { Id = 3, Name = "Кафедра экономики", FacultyId = 2, Faculty = econ };
        ctx.Departments.Add(dept1);
        ctx.Departments.Add(dept2);
        ctx.Departments.Add(dept3);

        var g1 = new Group { Id = 1, Name = "ИТ-21", CreatedYear = 2022, FacultyId = 1, Faculty = it };
        var g2 = new Group { Id = 2, Name = "ИТ-22", CreatedYear = 2023, FacultyId = 1, Faculty = it };
        var g3 = new Group { Id = 3, Name = "ЭК-21", CreatedYear = 2022, FacultyId = 2, Faculty = econ };
        ctx.Groups.Add(g1);
        ctx.Groups.Add(g2);
        ctx.Groups.Add(g3);

        var s1 = new Student
        {
            Id = 1, LastName = "Иванов", FirstName = "Пётр", MiddleName = "Сергеевич",
            BirthDate = new DateTime(2004, 3, 12), RecordBookNumber = "ИТ-21-001",
            Email = "ivanov@university.ru", Phone = "+7-900-111-22-33",
            EnrolledDate = new DateTime(2022, 9, 1), Status = StudentStatus.Active, GroupId = 1, Group = g1
        };
        var s2 = new Student
        {
            Id = 2, LastName = "Петрова", FirstName = "Анна", MiddleName = "Игоревна",
            BirthDate = new DateTime(2004, 7, 25), RecordBookNumber = "ИТ-21-002",
            Email = "petrova@university.ru", Phone = "+7-900-222-33-44",
            EnrolledDate = new DateTime(2022, 9, 1), Status = StudentStatus.Active, GroupId = 1, Group = g1
        };
        var s3 = new Student
        {
            Id = 3, LastName = "Сидоров", FirstName = "Олег",
            BirthDate = new DateTime(2005, 1, 8), RecordBookNumber = "ИТ-22-001",
            Email = "sidorov@university.ru",
            EnrolledDate = new DateTime(2023, 9, 1), Status = StudentStatus.Active, GroupId = 2, Group = g2
        };
        var s4 = new Student
        {
            Id = 4, LastName = "Кузнецова", FirstName = "Мария", MiddleName = "Андреевна",
            BirthDate = new DateTime(2003, 11, 30), RecordBookNumber = "ЭК-21-001",
            Email = "kuznetsova@university.ru", Phone = "+7-900-333-44-55",
            EnrolledDate = new DateTime(2022, 9, 1), Status = StudentStatus.AcademicLeave, GroupId = 3, Group = g3
        };
        ctx.Students.Add(s1);
        ctx.Students.Add(s2);
        ctx.Students.Add(s3);
        ctx.Students.Add(s4);

        var t1 = new Teacher
        {
            Id = 1, LastName = "Смирнов", FirstName = "Алексей", MiddleName = "Владимирович",
            Position = "доцент", Email = "smirnov@university.ru", DepartmentId = 1, Department = dept1
        };
        var t2 = new Teacher
        {
            Id = 2, LastName = "Фёдорова", FirstName = "Елена", MiddleName = "Петровна",
            Position = "профессор", Email = "fedorova@university.ru", DepartmentId = 2, Department = dept2
        };
        var t3 = new Teacher
        {
            Id = 3, LastName = "Громов", FirstName = "Дмитрий", MiddleName = "Иванович",
            Position = "старший преподаватель", Email = "gromov@university.ru", DepartmentId = 3, Department = dept3
        };
        ctx.Teachers.Add(t1);
        ctx.Teachers.Add(t2);
        ctx.Teachers.Add(t3);

        var d1 = new Discipline
        {
            Id = 1, Name = "Разработка кода информационных систем", TotalHours = 144,
            AssessmentType = AssessmentType.Exam, DepartmentId = 1, Department = dept1
        };
        var d2 = new Discipline
        {
            Id = 2, Name = "Высшая математика", TotalHours = 216,
            AssessmentType = AssessmentType.Exam, DepartmentId = 2, Department = dept2
        };
        var d3 = new Discipline
        {
            Id = 3, Name = "Экономика организации", TotalHours = 108,
            AssessmentType = AssessmentType.Credit, DepartmentId = 3, Department = dept3
        };
        ctx.Disciplines.Add(d1);
        ctx.Disciplines.Add(d2);
        ctx.Disciplines.Add(d3);

        ctx.Curriculums.Add(new Curriculum { Id = 1, GroupId = 1, Group = g1, DisciplineId = 1, Discipline = d1, Semester = 5, Hours = 72 });
        ctx.Curriculums.Add(new Curriculum { Id = 2, GroupId = 1, Group = g1, DisciplineId = 2, Discipline = d2, Semester = 5, Hours = 96 });
        ctx.Curriculums.Add(new Curriculum { Id = 3, GroupId = 2, Group = g2, DisciplineId = 2, Discipline = d2, Semester = 3, Hours = 96 });
        ctx.Curriculums.Add(new Curriculum { Id = 4, GroupId = 3, Group = g3, DisciplineId = 3, Discipline = d3, Semester = 5, Hours = 54 });

        var l1 = new Lesson
        {
            Id = 1, DisciplineId = 1, Discipline = d1, GroupId = 1, Group = g1, TeacherId = 1, Teacher = t1,
            StartTime = new DateTime(2026, 10, 12, 9, 0, 0), DurationHours = 2, Room = "А-305"
        };
        var l2 = new Lesson
        {
            Id = 2, DisciplineId = 2, Discipline = d2, GroupId = 1, Group = g1, TeacherId = 2, Teacher = t2,
            StartTime = new DateTime(2026, 10, 13, 11, 0, 0), DurationHours = 2, Room = "Б-112"
        };
        ctx.Lessons.Add(l1);
        ctx.Lessons.Add(l2);

        ctx.Grades.Add(new Grade { Id = 1, StudentId = 1, Student = s1, DisciplineId = 1, Discipline = d1, Value = 5, Semester = 5, Date = new DateTime(2026, 1, 10), TeacherId = 1, Teacher = t1 });
        ctx.Grades.Add(new Grade { Id = 2, StudentId = 1, Student = s1, DisciplineId = 2, Discipline = d2, Value = 4, Semester = 5, Date = new DateTime(2026, 1, 12), TeacherId = 2, Teacher = t2 });
        ctx.Grades.Add(new Grade { Id = 3, StudentId = 2, Student = s2, DisciplineId = 1, Discipline = d1, Value = 3, Semester = 5, Date = new DateTime(2026, 1, 10), TeacherId = 1, Teacher = t1 });
        ctx.Grades.Add(new Grade { Id = 4, StudentId = 2, Student = s2, DisciplineId = 2, Discipline = d2, Value = 5, Semester = 5, Date = new DateTime(2026, 1, 12), TeacherId = 2, Teacher = t2 });
        ctx.Grades.Add(new Grade { Id = 5, StudentId = 3, Student = s3, DisciplineId = 2, Discipline = d2, Value = 4, Semester = 3, Date = new DateTime(2025, 6, 20), TeacherId = 2, Teacher = t2 });

        ctx.Attendances.Add(new Attendance { Id = 1, StudentId = 1, Student = s1, LessonId = 1, Lesson = l1, IsPresent = true });
        ctx.Attendances.Add(new Attendance { Id = 2, StudentId = 2, Student = s2, LessonId = 1, Lesson = l1, IsPresent = false, Reason = "болезнь" });
        ctx.Attendances.Add(new Attendance { Id = 3, StudentId = 1, Student = s1, LessonId = 2, Lesson = l2, IsPresent = true });
        ctx.Attendances.Add(new Attendance { Id = 4, StudentId = 2, Student = s2, LessonId = 2, Lesson = l2, IsPresent = true });

        return ctx;
    }
}
