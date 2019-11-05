namespace TGame
{
    public struct PointHex
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public PointHex(int xyz) 
        { 
            X = xyz;
            Y = xyz;
            Z = xyz;
        }

        public PointHex(int x, int y, int z) 
        { 
            X = x;
            Y = y;
            Z = z;
        }

        public PointHex(Point pt) 
        {
            X = pt.X - (pt.Y - (pt.Y&1))/2;
            Z = pt.Y;
            Y = -X - Z;
        }

        public Point ToCoord()
        {
            return new Point(this);
        }

        static public PointHex operator+(PointHex a, PointHex b)
        {
            return new PointHex(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
    }
}