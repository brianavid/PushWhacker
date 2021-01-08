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
            public const int PageLeft = 62;
            public const int PageRight = 63;
            public const int BrightnessCC = 79;
            public const int FootSwitch = 69;

            public const int LayoutInKey = 43;
            public const int LayoutChromatic = 42;
            public const int LayoutScaler = 41;
            public const int LayoutStrummer = 40;
            public const int LayoutDrums = 39;
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

            public const int On = 127;
        }

        public const int FirstPad = 36;
        public const int LastPad = 99;
    }
}
