namespace Enlishing
{
    public readonly struct Vector(double x, double y)
    {
        public readonly static Vector zero = new(0, 0);
        public readonly static Vector up = new(0, -1);
        public readonly static Vector down = new(0, 1);
        public readonly static Vector left = new(-1, 0);
        public readonly static Vector right = new(1, 0);

        public readonly double x = x;
        public readonly double y = y;
        public readonly int intX = (int)x;
        public readonly int intY = (int)y;
        public readonly double sqrMagnitude = x * x + y * y;
        public readonly double magnitude = Math.Sqrt(x * x + y * y);

        public Vector Normalize() => this / magnitude;

        public static Vector operator +(Vector a) => a;
        public static Vector operator -(Vector a) => new(-a.x, -a.y);
        public static Vector operator +(Vector a, Vector b) => new(a.x + b.x, a.y + b.y);
        public static Vector operator -(Vector a, Vector b) => a + -b;
        public static Vector operator *(Vector a, double b) => new(a.x * b, a.y * b);
        public static Vector operator *(double a, Vector b) => b * a;
        public static Vector operator /(Vector a, double b) => a * (1 / b);
        public static Vector operator /(double a, Vector b) => b / a;

        public static bool operator ==(Vector a, Vector b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(Vector a, Vector b) => !(a == b);

        public override bool Equals(object obj)
        {
            return obj is Vector other && this == other;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public override string ToString()
        {
            return $"({x},{y})";
        }
    }
}
