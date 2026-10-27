using StudentAccounting.Dal.Entities;

namespace StudentAccounting.Bll.Legacy;

/// <summary>
/// ИСХОДНАЯ (НЕОПТИМАЛЬНАЯ) РЕАЛИЗАЦИЯ МОДУЛЯ РАСЧЁТА СТИПЕНДИЙ И ОТЧЁТОВ.
/// Класс оставлен как «точка отсчёта» перед рефакторингом (см. ЛР4).
///
/// Проблемы этой версии:
///  1) жёсткая зависимость от конкретных коллекций List&lt;T&gt; — подставить реализацию
///     хранилища (или mock-объект) для тестирования невозможно;
///  2) дублирование кода: расчёт среднего балла и коэффициента повторяется в каждом методе;
///  3) смешение бизнес-логики, форматирования (текст/CSV) и записи в файл в одном классе;
///  4) «магические числа» и длинные методы с ветвлениями;
///  5) исключения перехватываются и «проглатываются» — об ошибке узнать нельзя;
///  6) модульное тестирование практически невозможно.
/// </summary>
public class ScholarshipReportGenerator
{
    private const decimal BaseScholarship = 2000m;

    private readonly List<Student> _students;
    private readonly List<Grade> _grades;

    public ScholarshipReportGenerator(List<Student> students, List<Grade> grades)
    {
        _students = students;
        _grades = grades;
    }

    // ДУБЛИРОВАНИЕ №1: расчёт среднего балла по списку оценок.
    public double AverageOf(List<Grade> grades)
    {
        if (grades == null) return 0;
        double sum = 0;
        for (int i = 0; i < grades.Count; i++)
        {
            sum = sum + grades[i].Value;
        }
        if (grades.Count == 0) return 0;
        return sum / grades.Count;
    }

    // ДУБЛИРОВАНИЕ №2: тот же расчёт, но по идентификатору студента.
    public double AverageOfStudent(int studentId)
    {
        var list = new List<Grade>();
        for (int i = 0; i < _grades.Count; i++)
        {
            if (_grades[i].StudentId == studentId)
            {
                list.Add(_grades[i]);
            }
        }
        return AverageOf(list);
    }

    // «Магические числа» и повторяющаяся цепочка if/else.
    public string ScholarshipPercent(double averageGrade)
    {
        if (averageGrade == 0)
        {
            return "0%";
        }
        if (averageGrade >= 4.5)
        {
            return "50%";
        }
        if (averageGrade >= 3.5)
        {
            return "0%";
        }
        return "нет";
    }

    // Копия той же логики, но с другим типом результата.
    public decimal ScholarshipAmount(double averageGrade)
    {
        if (averageGrade == 0)
        {
            return 0;
        }
        if (averageGrade >= 4.5)
        {
            return BaseScholarship + BaseScholarship / 2;
        }
        if (averageGrade >= 3.5)
        {
            return BaseScholarship;
        }
        return 0;
    }

    // Длинный метод: логика + форматирование + обращение к консоли.
    public string GenerateTextReport(int groupId)
    {
        var text = "ОТЧЁТ О СТИПЕНДИИ (текстовый формат)\n";
        text += "--------------------------------------\n";
        for (int i = 0; i < _students.Count; i++)
        {
            var s = _students[i];
            if (s.GroupId != groupId) continue;
            var avg = AverageOfStudent(s.Id);
            text += s.LastName + " " + s.FirstName + " | ср.балл=" + avg.ToString("F2") +
                    " | стипендия=" + ScholarshipAmount(avg) + " руб. (" + ScholarshipPercent(avg) + ")\n";
            if (avg >= 4.5)
            {
                Console.WriteLine("Отличник: " + s.LastName);
            }
        }
        return text;
    }

    // Почти копия предыдущего метода — отличается только разделитель.
    public string GenerateCsvReport(int groupId)
    {
        var csv = "Фамилия;Имя;Средний балл;Сумма;Процент\n";
        foreach (var s in _students)
        {
            if (s.GroupId != groupId) continue;
            var avg = AverageOfStudent(s.Id);
            csv += s.LastName + ";" + s.FirstName + ";" + avg.ToString("F2") + ";" +
                   ScholarshipAmount(avg) + ";" + ScholarshipPercent(avg) + "\n";
        }
        return csv;
    }

    // Запись в файл прямо из бизнес-логики + «проглатывание» исключений.
    public void SaveCsvReport(int groupId, string path)
    {
        try
        {
            File.WriteAllText(path, GenerateCsvReport(groupId));
        }
        catch (Exception)
        {
            // файл мог быть занят, путь некорректен и т.п. — но никто об этом не узнает
        }
    }
}
