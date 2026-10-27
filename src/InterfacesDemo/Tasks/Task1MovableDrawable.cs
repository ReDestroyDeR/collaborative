using System.Text;

namespace InterfacesDemo.Tasks;

/// <summary>Задание 1. Интерфейсы IMovable и IDrawable.</summary>
public static class Task1MovableDrawable
{
    /// <summary>Объект, который можно перемещать по координатной плоскости.</summary>
    public interface IMovable
    {
        /// <summary>Перемещает объект на dx по оси X и на dy по оси Y.</summary>
        void Move(int dx, int dy);
    }

    /// <summary>Точка на координатной плоскости.</summary>
    public class Point : IMovable
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Point(int x, int y) { X = x; Y = y; }

        public void Move(int dx, int dy)
        {
            X += dx;
            Y += dy;
        }

        public override string ToString() => $"Point({X}, {Y})";
    }

    /// <summary>Объект, который умеет рисовать себя на экране.</summary>
    public interface IDrawable
    {
        /// <summary>Выводит описание фигуры (рисует её).</summary>
        string Draw();
    }

    /// <summary>Круг, который можно нарисовать.</summary>
    public class Circle : IDrawable
    {
        public int CenterX { get; }
        public int CenterY { get; }
        public int Radius { get; }

        public Circle(int centerX, int centerY, int radius)
        {
            CenterX = centerX;
            CenterY = centerY;
            Radius = radius;
        }

        public string Draw() => $"Рисую круг: центр ({CenterX}; {CenterY}), радиус {Radius}";
    }

    /// <summary>Прямоугольник, который можно нарисовать.</summary>
    public class Rectangle : IDrawable
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public Rectangle(int x, int y, int width, int height)
        {
            X = x; Y = y; Width = width; Height = height;
        }

        public string Draw() => $"Рисую прямоугольник: ({X}; {Y}), {Width}x{Height}";
    }

    /// <summary>Обрабатывает список фигур через общий интерфейс IDrawable.</summary>
    public static string DrawAll(IEnumerable<IDrawable> shapes)
    {
        var sb = new StringBuilder();
        foreach (var shape in shapes)
            sb.AppendLine(shape.Draw());
        return sb.ToString();
    }

    public static string Run()
    {
        var sb = new StringBuilder();
        sb.AppendLine("--- Задание 1: IMovable / Point, IDrawable / Circle / Rectangle ---");

        var point = new Point(2, 3);
        sb.AppendLine($"Создана точка: {point}");
        point.Move(5, -2);
        sb.AppendLine($"После point.Move(5, -2): {point}");

        var shapes = new List<IDrawable>
        {
            new Circle(0, 0, 10),
            new Rectangle(1, 2, 30, 20),
            new Circle(5, 5, 3)
        };
        sb.Append(DrawAll(shapes));
        return sb.ToString();
    }
}
