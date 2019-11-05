using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace TGame
{
    [Owned]
    public class Point 
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int xy) 
        { 
            X = xy;
            Y = xy;
        }

        [JsonConstructor]
        public Point(int X, int Y) 
        { 
            this.X = X;
            this.Y = Y;
        }

        public Point(PointHex pt)
        {
            X = pt.X + (pt.Z - (pt.Z&1)) / 2;
            Y = pt.Z;
        }

        public Point(JToken data)
        {
            X = (int)data["X"];
            Y = (int)data["Y"];
        }

        public PointHex ToHex()
        {
            return new PointHex(this);
        }

        static public Point operator+(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        static public Point operator*(Point a, int b)
        {
            return new Point(a.X * b, a.Y * b);
        }

        static public bool operator ==(Point a, Point b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
        static public bool operator !=(Point a, Point b)
        {
            return a.X != b.X || a.Y != b.Y;
        }
        public override bool Equals(object o)
        {
            return (o as Point) == this;
        }
        
        public override int GetHashCode()
        {
            return X*10000+Y;
        }

        public override string ToString()
        {
            return X + " " + Y;
        }
    }
}