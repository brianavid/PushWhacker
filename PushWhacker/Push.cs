using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushWhacker
{
    public class Push
    {
        public class Buttons
        {
            public const int OctaveUp = 55;
            public const int OctaveDown = 54;
            public const int KeyDown = 47;
            public const int KeyUp = 46;
            public const int ScaleLeft = 44;
            public const int ScaleRight = 45;

            public const int BrightnessCC = 79;
            public const int FootSwitch = 69;

            public static  int[] Layouts = { 43, 42, 41, 40, 39, 38, 37, 36 };

            public const int ScaleMajor = 49;
            public const int ScaleMinor = 48;

            public const int ToggleTouchStrip = 90;

            public const int ShowInfo = 30;
        }

        public class Colours
        {
            public const int Black = 0;
            public const int LightGrey = 123;
            public const int DarkGrey = 124;
            public const int White = 122;
            public const int Red = 127;
            public const int Blue = 125;
            public const int Green = 126;
            public const int Yellow = 29;
            public const int DullRed = 67;
            public const int DullBlue = 20;
            public const int DullGreen = 30;
            public const int DullYellow = 85;

            public const int On = 127;
            public const int Dim = 16;
            public const int Off = 0;
        }

        public const int FirstPad = 36;
        public const int LastPad = 99;
    }
}
