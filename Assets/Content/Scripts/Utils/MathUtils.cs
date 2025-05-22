namespace Heroicsolo.SpiralSurvivor.Utils
{
    public static class MathUtils
    {
        public static int NextPowerOfTwo(int value)
        {
            if (value < 1) return 1;

            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value++;

            return value;
        }
    }
}
