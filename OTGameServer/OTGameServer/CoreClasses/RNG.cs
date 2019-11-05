using System;

namespace TGame
{
    public class RNG
    {
        static System.Random rng = new System.Random();
        
        static public int Next(int min, int max)
        {
            return rng.Next(min, max);
        }

        static public Guid RandomGUID()
        {
            var bytes = new byte[16];
            
            rng.NextBytes(bytes);

            return new Guid(bytes);
        }
    }
}