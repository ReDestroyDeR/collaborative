using StudentAccounting.Bll.Scholarship;
using StudentAccounting.Bll.Services;
using StudentAccounting.Dal.Entities;
using StudentAccounting.Dal.Repositories;
using StudentAccounting.Dal.Storage;

namespace StudentAccounting.Ui;

/// <summary>
/// Консольный пользовательский интерфейс (слой UI).
/// Собирает зависимости (репозитории и сервисы) и демонстрирует работу
/// паттерна «Репозиторий» и сериализации в JSON.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var dataDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data");
        dataDir = Path.GetFullPath(dataDir);
        Directory.CreateDirectory(dataDir);

        // ---- Компоновка зависимостей: DAL -> BLL -> UI ----
        var studentRepository = new InMemoryRepository<Student>(UniversityContext.CreateSeeded().Students);
        var groupRepository = new InMemoryRepository<Group>(UniversityContext.CreateSeeded().Groups);
        var gradeRepository = new InMemoryRepository<Grade>(UniversityContext.CreateSeeded().Grades);
        var attendanceRepository = new InMemoryRepository<Attendance>(UniversityContext.CreateSeeded().Attendances);

        var studentService = new StudentService(studentRepository, groupRepository);
        var progressService = new ProgressService(studentRepository, gradeRepository, attendanceRepository);

        Console.WriteLine("=== ЛР2. Паттерн «Репозиторий»: DAL (InMemoryRepository) -> BLL (сервисы) -> UI ===");
        Console.WriteLine();

        Console.WriteLine("1. Все студенты (через IRepository<Student>):");
        foreach (var s in studentService.GetAll())
            Console.WriteLine($"   {s.Id}. {s.FullName()} | гр. {s.Group?.Name ?? groupRepository.GetById(s.GroupId)?.Name} | {s.RecordBookNumber} | {s.Status}");
        Console.WriteLine();

        Console.WriteLine("2. CRUD через репозиторий:");
        var created = studentService.Enroll("Новиков", "Игорь", "Анатольевич", "novikov@university.ru", new DateTime(2005, 5, 17), 1);
        Console.WriteLine($"   Create: зачислен {created.FullName()}, № {created.RecordBookNumber}, Id = {created.Id}");
        created.Phone = "+7-901-555-66-77";
        studentRepository.Update(created);
        Console.WriteLine($"   Update: у студента Id={created.Id} обновлён телефон: {studentRepository.GetById(created.Id)!.Phone}");
        Console.WriteLine($"   Read:   GetByGroup(1) -> {string.Join(", ", studentService.GetByGroup(1).Select(s => s.LastName))}");
        Console.WriteLine($"   Delete: удаление Id={created.Id} -> {studentRepository.Delete(created.Id)}, повторно -> {studentRepository.Delete(created.Id)}");
        Console.WriteLine();

        Console.WriteLine("3. Перевод и отчисление:");
        Console.WriteLine($"   Перевод студента Id=3 в группу ЭК-21: {studentService.Transfer(3, 3)}");
        Console.WriteLine($"   Отчисление студента Id=3: {studentService.Expel(3, "по собственному желанию")}");
        Console.WriteLine();

        Console.WriteLine("4. Успеваемость и посещаемость (ProgressService):");
        foreach (var s in studentService.GetAll())
            Console.WriteLine($"   {s.FullName(),-28} ср. балл = {progressService.GetAverageGrade(s.Id):F2}, посещаемость = {progressService.GetAttendanceRate(s.Id):F0}%, отличник = {progressService.IsExcellent(s.Id)}");
        Console.WriteLine();

        Console.WriteLine("5. Сериализация в JSON (JsonRepository<Student>):");
        var jsonPath = Path.Combine(dataDir, "students.json");
        if (File.Exists(jsonPath)) File.Delete(jsonPath);
        var jsonRepository = new JsonRepository<Student>(jsonPath);
        foreach (var s in studentService.GetAll())
            jsonRepository.Add(s);
        jsonRepository.Save();
        Console.WriteLine($"   Данные сохранены в файл: {jsonPath} ({new FileInfo(jsonPath).Length} байт)");

        var reloaded = new JsonRepository<Student>(jsonPath);
        Console.WriteLine($"   Десериализовано из файла: {reloaded.GetAll().Count} студентов");
        foreach (var s in reloaded.Find(s => s.Status == StudentStatus.Active).Take(3))
            Console.WriteLine($"     - {s.FullName()} ({s.RecordBookNumber}), зачислен {s.EnrolledDate:dd.MM.yyyy}");
        Console.WriteLine();

        Console.WriteLine("6. Проверка бизнес-правил (некорректный e-mail):");
        try
        {
            studentService.Enroll("Тестов", "Тест", null, "некорректный-адрес", new DateTime(2005, 1, 1), 1);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"   Исключение перехвачено: {ex.Message}");
        }
        Console.WriteLine();

        Console.WriteLine("7. Расчёт стипендий (ScholarshipService — после рефакторинга):");
        var csvService = new ScholarshipService(studentRepository, groupRepository, gradeRepository,
            new StandardScholarshipPolicy(), new CsvReportExporter());
        var report = csvService.BuildReport(1);
        Console.WriteLine("   Отчёт в формате CSV:");
        Console.Write("   " + csvService.Export(report).Replace("\n", "\n   "));

        var mdService = new ScholarshipService(studentRepository, groupRepository, gradeRepository,
            new StandardScholarshipPolicy(), new MarkdownReportExporter());
        Console.WriteLine("   Отчёт в формате Markdown (группа ИТ-22):");
        Console.Write("   " + mdService.Export(mdService.BuildReport(2)).Replace("\n", "\n   "));
        Console.WriteLine();

        Console.WriteLine("   Попытка построить отчёт по несуществующей группе Id=99:");
        var ok = csvService.TryBuildReport(99, out var failedReport, out var error);
        Console.WriteLine($"   TryBuildReport -> {ok}, отчёт = {(failedReport is null ? "null" : "не null")}, ошибка: {error}");
    }
}
