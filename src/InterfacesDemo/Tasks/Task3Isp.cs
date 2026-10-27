using System.Text;

namespace InterfacesDemo.Tasks;

/// <summary>Задание 3. Принцип разделения интерфейсов (ISP).</summary>
public static class Task3Isp
{
    // ---------- ШАГ 1. «Толстый» интерфейс — нарушает ISP ----------
    /// <summary>Толстый интерфейс устройства: заставляет реализовывать всё подряд.</summary>
    public interface IDevice
    {
        void Print(string document);
        void Scan(string target);
        void Fax(string number, string document);
    }

    // ---------- ШАГ 2. Разделение на узкие интерфейсы ----------
    public interface IPrinter
    {
        string Print(string document);
    }

    public interface IScanner
    {
        string Scan(string target);
    }

    public interface IFax
    {
        string Fax(string number, string document);
    }

    /// <summary>Принтер умеет только печатать — реализует только нужный интерфейс.</summary>
    public class Printer : IPrinter
    {
        public string Print(string document) => $"[Принтер] Печать документа: {document}";
    }

    /// <summary>Сканер умеет только сканировать.</summary>
    public class Scanner : IScanner
    {
        public string Scan(string target) => $"[Сканер] Сканирование в файл: {target}";
    }

    /// <summary>МФУ объединяет все три узких интерфейса.</summary>
    public class MultifunctionDevice : IPrinter, IScanner, IFax
    {
        public string Print(string document) => $"[МФУ] Печать документа: {document}";
        public string Scan(string target) => $"[МФУ] Сканирование в файл: {target}";
        public string Fax(string number, string document) => $"[МФУ] Отправка факса на {number}: {document}";
    }

    public static string Run()
    {
        var sb = new StringBuilder();
        sb.AppendLine("--- Задание 3: принцип разделения интерфейсов (ISP) ---");
        sb.AppendLine("До применения ISP: IDevice { Print, Scan, Fax } —Printer был бы обязан");
        sb.AppendLine("реализовывать Scan() и Fax(), что ему не нужно (нарушение ISP).");
        sb.AppendLine("После: интерфейсы разделены на IPrinter / IScanner / IFax.");
        sb.AppendLine();

        IPrinter printer = new Printer();
        IScanner scanner = new Scanner();
        sb.AppendLine(printer.Print("Отчёт.docx"));
        sb.AppendLine(scanner.Scan("scan_001.png"));
        sb.AppendLine();

        var mfu = new MultifunctionDevice();
        sb.AppendLine(mfu.Print("Договор.pdf"));
        sb.AppendLine(mfu.Scan("scan_002.png"));
        sb.AppendLine(mfu.Fax("+7-495-000-00-00", "Акт.pdf"));
        return sb.ToString();
    }
}
