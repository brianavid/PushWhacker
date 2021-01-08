using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace PushWhacker
{
    public class ConfigValues
    {
        public class Layouts
        {
            public const string InKey = "In Key";
            public const string Chromatic = "Chromatic";
            public const string Linear = "Linear";
            public const string Scaler = "Scaler";
            public const string Drums = "Drums";
            public const string Strummer = "Strummer";

            public static string[] Choices = new string[] { InKey, Chromatic, Scaler, Drums, Strummer };
        }

        public string Output { get; set; }
        public string Layout { get; set; }
        public string Channel { get; set; }
        public string Scale { get; set; }
        public string Key { get; set; }
        public string Octave { get; set; }
        public int OctaveNumber { get { return Int32.Parse(Octave); } }
        public bool Log { get; set; }
        public bool Debug { get; set; }
        public bool SemitonePedal { get; set; }

        public Dictionary<string, int> Keys { get; }

        public ConfigValues()
        {
            Keys = new Dictionary<string, int>();
            Keys["C"] = 0;
            Keys["C#"] = 1;
            Keys["D"] = 2;
            Keys["D#"] = 3;
            Keys["E"] = 4;
            Keys["F"] = 5;
            Keys["F#"] = 6;
            Keys["G"] = 7;
            Keys["G#"] = 8;
            Keys["A"] = 9;
            Keys["A#"] = 10;
            Keys["B"] = 11;
        }

        public void Load()
        {
            try { 
                using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey("Software", true).CreateSubKey("PushWhacker"))
                {
                    Output = (string)regKey.GetValue("Output");
                    Layout = (string)regKey.GetValue("Layout");
                    Channel = (string)regKey.GetValue("Channel");
                    Scale = (string)regKey.GetValue("Scale");
                    Key = (string)regKey.GetValue("Key");
                    Octave = (string)regKey.GetValue("Octave");
                    Log = (int)regKey.GetValue("Log", 0) != 0;
                    Debug = (int)regKey.GetValue("Debug", 0) != 0;
                    SemitonePedal = (int)regKey.GetValue("SemitonePedal", 0) != 0;
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void Save()
        {
            using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey("Software", true).CreateSubKey("PushWhacker"))
            {
                regKey.SetValue("Output", Output);
                regKey.SetValue("Layout", Layout);
                regKey.SetValue("Channel", Channel);
                regKey.SetValue("Scale", Scale);
                regKey.SetValue("Key", Key);
                regKey.SetValue("Octave", Octave);
                regKey.SetValue("Log", Log ? 1 : 0);
                regKey.SetValue("Debug", Debug ? 1 : 0);
                regKey.SetValue("SemitonePedal", SemitonePedal ? 1 : 0);
            }
        }
    }
}
