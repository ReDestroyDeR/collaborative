using System.Text;

namespace InterfacesDemo.Tasks;

/// <summary>Задание 2. Интерфейс IShape и производный от него I3DShape.</summary>
public static class Task2Shapes
{
    /// <summary>Геометрическая фигура на плоскости.</summary>
    public interface IShape
    {
        /// <summary>Площадь фигуры.</summary>
        double GetArea();

        /// <summary>Периметр фигуры.</summary>
        double GetPerimeter();
    }

    /// <summary>Круг.</summary>
    public class Circle : IShape
    {
        public double Radius { get; }
        public Circle(double radius) => Radius = radius;
        public double GetArea() => Math.PI * Radius * Radius;
        public double GetPerimeter() => 2 * Math.PI * Radius;
        public override string ToString() => $"Круг (r = {Radius})";
    }

    /// <summary>Прямоугольник.</summary>
    public class Rectangle : IShape
    {
        public double Width { get; }
        public double Height { get; }
        public Rectangle(double width, double height) { Width = width; Height = height; }
        public double GetArea() => Width * Height;
        public double GetPerimeter() => 2 * (Width + Height);
        public override string ToString() => $"Прямоугольник ({Width} x {Height})";
    }

    /// <summary>Объёмная фигура наследует контракт плоской фигуры.</summary>
    public interface I3DShape : IShape
    {
        /// <summary>Объём фигуры.</summary>
        double GetVolume();
    }

    /// <summary>Куб.</summary>
    public class Cube : I3DShape
    {
        public double Side { get; }
        public Cube(double side) => Side = side;
        public double GetArea() => 6 * Side * Side;              // площадь поверхности
        public double GetPerimeter() => 12 * Side;               // сумма длин всех рёбер
        public double GetVolume() => Side * Side * Side;
        public override string ToString() => $"Куб (a = {Side})";
    }

    /// <summary>Выводит информацию о любой фигуре через интерфейс IShape.</summary>
    public static string PrintShapeInfo(IShape shape) =>
        $"{shape}: площадь = {shape.GetArea():F2}, периметр = {shape.GetPerimeter():F2}";

    /// <summary>Выводит информацию об объёмной фигуре через интерфейс I3DShape.</summary>
    public static string PrintShapeInfo(I3DShape shape) =>
        $"{shape}: площадь = {shape.GetArea():F2}, периметр = {shape.GetPerimeter():F2}, объём = {shape.GetVolume():F2}";

    public static string Run()
    {
        var sb = new StringBuilder();
        sb.AppendLine("--- Задание 2: IShape / Circle / Rectangle, I3DShape / Cube ---");
        var shapes = new List<IShape> { new Circle(5), new Rectangle(4, 6), new Cube(3) };
        foreach (var shape in shapes)
            sb.AppendLine(PrintShapeInfo(shape));

        var cube = new Cube(2);
        sb.AppendLine(PrintShapeInfo(cube));
        sb.AppendLine($"I3DShape наследует IShape: {typeof(I3DShape).GetInterfaces().Contains(typeof(IShape))}");
        return sb.ToString();
    }
}
