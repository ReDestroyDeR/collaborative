using StudentAccounting.Bll.Services;
using StudentAccounting.Dal.Entities;
using StudentAccounting.Dal.Storage;

namespace StudentAccounting.Ui;

/// <summary>
/// Консольный пользовательский интерфейс (слой UI).
/// Демонстрирует работу с бизнес-логикой через слой BLL.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var context = UniversityContext.CreateSeeded();
        var studentService = new StudentService(context);

        Console.WriteLine("=== ИС «Учёт студентов» — демонстрация работы слоёв DAL/BLL/UI ===");
        Console.WriteLine();

        Console.WriteLine("1. Все студенты:");
        foreach (var s in studentService.GetAll())
            Console.WriteLine($"   {s.Id}. {s.FullName()} | гр. {s.Group?.Name} | {s.RecordBookNumber} | {s.Status}");
        Console.WriteLine();

        Console.WriteLine("2. Студенты группы ИТ-21:");
        foreach (var s in studentService.GetByGroup(1))
            Console.WriteLine($"   {s.LastName} {s.FirstName}");
        Console.WriteLine();

        Console.WriteLine("3. Поиск по фамилии «Пет»:");
        foreach (var s in studentService.SearchByLastName("Пет"))
            Console.WriteLine($"   {s.FullName()}");
        Console.WriteLine();

        Console.WriteLine("4. Успеваемость и посещаемость:");
        foreach (var s in studentService.GetAll())
            Console.WriteLine($"   {s.LastName,-12} ср. балл = {studentService.GetAverageGrade(s.Id):F2}, посещаемость = {studentService.GetAttendanceRate(s.Id):F0}%");
        Console.WriteLine();

        Console.WriteLine("5. Средний балл по группам:");
        foreach (var g in context.Groups)
            Console.WriteLine($"   {g.Name,-8} ср. балл = {studentService.GetGroupAverageGrade(g.Id):F2}");
        Console.WriteLine();

        Console.WriteLine("6. Зачисление нового студента в группу ИТ-21:");
        var created = studentService.Enroll("Новиков", "Игорь", "Анатольевич", "novikov@university.ru", new DateTime(2005, 5, 17), 1);
        Console.WriteLine($"   Зачислен: {created.FullName()}, студенческий № {created.RecordBookNumber}");
        Console.WriteLine();

        Console.WriteLine("7. Проверка бизнес-правил (некорректный e-mail):");
        try
        {
            studentService.Enroll("Тестов", "Тест", null, "некорректный-адрес", new DateTime(2005, 1, 1), 1);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"   Исключение перехвачено: {ex.Message}");
        }
    }
}
