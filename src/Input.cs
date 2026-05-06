namespace VehicleGadgetsPlus
{
    using System.Runtime.InteropServices;

    internal static class Input
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public const int NumPad1 = 0x61;
        public const int NumPad2 = 0x62;
        public const int NumPad3 = 0x63;
        public const int NumPad4 = 0x64;
        public const int NumPad5 = 0x65;
        public const int NumPad6 = 0x66;
        public const int NumPad7 = 0x67;
        public const int NumPad8 = 0x68;
        public const int NumPad9 = 0x69;
        public const int O = 0x4F;

        private static readonly bool[] prevState = new bool[256];
        private static readonly bool[] currState = new bool[256];

        public static void Update()
        {
            for (int i = 0; i < 256; i++)
            {
                prevState[i] = currState[i];
                currState[i] = (GetAsyncKeyState(i) & 0x8000) != 0;
            }
        }

        // Key is currently held down
        public static bool IsKeyDownRightNow(int vKey) => currState[vKey];

        // Key was just pressed this frame (rising edge)
        public static bool IsKeyDown(int vKey) => currState[vKey] && !prevState[vKey];
    }
}
