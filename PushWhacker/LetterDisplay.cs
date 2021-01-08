using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushWhacker
{
    class LetterDisplay
    {
        static string[] A =
        {
            " ** ",
            "*  *",
            "*  *",
            "****",
            "*  *",
            "*  *",
            "*  *",
        };
        static string[] Asharp =
        {
            " **     ",
            "*  *  * ",
            "*  *  * ",
            "**** ***",
            "*  *  * ",
            "*  *  * ",
            "*  *    ",
        };
        static string[] B =
        {
            "*** ",
            "*  *",
            "*  *",
            "*** ",
            "*  *",
            "*  *",
            "*** ",
        };
        static string[] C =
        {
            " ** ",
            "*  *",
            "*   ",
            "*   ",
            "*   ",
            "*  *",
            " ** ",
        };
        static string[] Csharp =
        {
            " **     ",
            "*  *  * ",
            "*     * ",
            "*    ***",
            "*     * ",
            "*  *  * ",
            " **     ",
        };
        static string[] D =
        {
            "*** ",
            "*  *",
            "*  *",
            "*  *",
            "*  *",
            "*  *",
            "*** ",
        };
        static string[] Dsharp =
        {
            "***     ",
            "*  *  * ",
            "*  *  * ",
            "*  * ***",
            "*  *  * ",
            "*  *  * ",
            "***     ",
        };
        static string[] E =
        {
            "****",
            "*   ",
            "*   ",
            "*** ",
            "*   ",
            "*   ",
            "****",
        };
        static string[] F =
        {
            "****",
            "*   ",
            "*   ",
            "*** ",
            "*   ",
            "*   ",
            "*   ",
        };
        static string[] Fsharp =
        {
            "****    ",
            "*     * ",
            "*     * ",
            "***  ***",
            "*     * ",
            "*     * ",
            "*       ",
        };
        static string[] G =
        {
            " ** ",
            "*  *",
            "*   ",
            "* **",
            "*  *",
            "*  *",
            " ** ",
        };
        static string[] Gsharp =
        {
            " **     ",
            "*  *  * ",
            "*     * ",
            "* ** ***",
            "*  *  * ",
            "*  *  * ",
            " **     ",
        };

        static Dictionary<string, string[]> KeyBitmap = new Dictionary<string, string[]>
        {
            { "C", C },
            { "C#", Csharp },
            { "D", D },
            { "D#", Dsharp },
            { "E", E },
            { "F", F },
            { "F#", Fsharp },
            { "G", G },
            { "G#", Gsharp },
            { "A", A },
            { "A#", Asharp },
            { "B", B },
        };

        public static bool IsBit(string key, int i)
        {
            var bitmap = KeyBitmap[key];
            var row = 7 - (i / 8);
            var col = i % 8;
            return row >= bitmap.Length ? false :
                   col >= bitmap[row].Length ? false :
                   bitmap[row][col] != ' ';
        }
    }
}
