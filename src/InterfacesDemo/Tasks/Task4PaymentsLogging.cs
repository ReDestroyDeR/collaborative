using System.Text;

namespace InterfacesDemo.Tasks;

/// <summary>Задание 4. IPayable (оплата) и ILogger (журналирование).</summary>
public static class Task4PaymentsLogging
{
    /// <summary>Объект, способный оплатить указанную сумму.</summary>
    public interface IPayable
    {
        /// <summary>Пытается оплатить сумму; возвращает true при успехе.</summary>
        bool Pay(decimal amount);
        /// <summary>Название способа оплаты.</summary>
        string Name { get; }
    }

    /// <summary>Оплата банковской картой.</summary>
    public class CreditCard : IPayable
    {
        private decimal _balance;
        public string Name { get; }
        public CreditCard(string name, decimal balance) { Name = name; _balance = balance; }
        public bool Pay(decimal amount)
        {
            if (amount <= 0 || amount > _balance) return false;
            _balance -= amount;
            return true;
        }
        public decimal Balance => _balance;
    }

    /// <summary>Оплата наличными.</summary>
    public class Cash : IPayable
    {
        private decimal _amount;
        public string Name { get; }
        public Cash(decimal amount) { Name = "Наличные"; _amount = amount; }
        public bool Pay(decimal amount)
        {
            if (amount <= 0 || amount > _amount) return false;
            _amount -= amount;
            return true;
        }
    }

    /// <summary>Обрабатывает оплату любым способом через интерфейс IPayable.</summary>
    public static string ProcessPayment(IPayable payable, decimal amount)
    {
        var ok = payable.Pay(amount);
        return ok
            ? $"Оплата «{payable.Name}» на сумму {amount} руб. выполнена успешно"
            : $"Оплата «{payable.Name}» на сумму {amount} руб. ОТКЛОНЕНА (недостаточно средств или неверная сумма)";
    }

    /// <summary>Интерфейс журналирования.</summary>
    public interface ILogger
    {
        void Log(string message);
    }

    /// <summary>Вывод сообщений в консоль.</summary>
    public class ConsoleLogger : ILogger
    {
        public void Log(string message) => Console.WriteLine($"[CONSOLE {DateTime.Now:HH:mm:ss}] {message}");
    }

    /// <summary>Запись сообщений в файл журнала.</summary>
    public class FileLogger : ILogger
    {
        private readonly string _path;
        private readonly List<string> _written = new();
        public FileLogger(string path) => _path = path;
        public void Log(string message)
        {
            File.AppendAllText(_path, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            _written.Add(message);
        }
        public IReadOnlyList<string> Written => _written;
        public string Path => _path;
    }

    /// <summary>Выполняет работу, используя переданный журнал через интерфейс ILogger.</summary>
    public static void DoWork(ILogger logger)
    {
        logger.Log("Начало работы");
        logger.Log("Выполняется обработка данных...");
        logger.Log("Работа завершена");
    }

    public static string Run()
    {
        var sb = new StringBuilder();
        sb.AppendLine("--- Задание 4: IPayable / ILogger ---");
        var card = new CreditCard("Visa •••• 4242", 5000);
        var cash = new Cash(300);
        sb.AppendLine(ProcessPayment(card, 1500));
        sb.AppendLine(ProcessPayment(card, 9000));
        sb.AppendLine(ProcessPayment(cash, 250));
        sb.AppendLine(ProcessPayment(cash, 400));
        sb.AppendLine();

        sb.AppendLine("Журналирование через ILogger:");
        sb.AppendLine("  ConsoleLogger: сообщения выводятся в консоль (см. ниже):");
        DoWork(new ConsoleLogger());

        var logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "interfaces_demo.log");
        if (File.Exists(logPath)) File.Delete(logPath);
        DoWork(new FileLogger(logPath));
        sb.AppendLine($"  FileLogger: сообщения записаны в файл {logPath}:");
        foreach (var line in File.ReadAllLines(logPath))
            sb.AppendLine($"    {line}");
        return sb.ToString();
    }
}
