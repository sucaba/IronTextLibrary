using System;

namespace IronText.Algorithm
{
    public static class Functions
    {
        public static double Log2 = Math.Log(2.0);

        public static double Entropy(double probability)
        {
            return - probability * Math.Log(probability) / Log2;
        }
    }
}
