using System;

namespace Game.ExtensionMethods
{
    public static class MathExtensions
    {
        // Definition of PI constant
        public static float PI = 3.14159265358979323f;

        // Computes the sine of the specified angle in radians
        public static float Sin(double x) => (float)Math.Sin(x);

        // Computes the cosine of the specified angle in radians
        public static float Cos(double x) => (float)Math.Cos(x);

        // Returns the angle whose tangent is the quotient of two specified numbers
        public static float Atan2(double y, double x) => (float)Math.Atan2(y, x);

        // Computes the square root of a specified number
        public static float Sqrt(double x) => (float)Math.Sqrt(x);

        // Returns the absolute value of a specified number
        public static float Abs(double value) => (float)Math.Abs(value);

        // Clamps a value to be within the specified range
        public static float Clamp(float value, float lower, float upper)
        {
            if (value < lower)
            {
                return lower;
            }
            if (value > upper)
            {
                return upper;
            }
            return value;
        }
    }
}