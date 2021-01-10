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
            public const string InKeyPlusKS = "In Key with KS";
            public const string ChromaticPlusKS = "Chromatic with KS";
            public const string Linear = "Linear";
            public const string Scaler = "Scaler";
            public const string Strummer = "Strummer";
            public const string Drums = "Drums";
            public const string BigDrums = "Big Drums";

            public static string[] Choices = new string[] { InKey, Chromatic, InKeyPlusKS, ChromaticPlusKS, Scaler, Strummer, Drums, BigDrums };
        }

        public class Pressures
        {
            public const string ChannelAftertouch = "Channel Aftertouch";
            public const string PolyPressure = "Poly Pressure";
            public const string BoostModulation = "Boost Modulation";

            public static string[] Choices = new string[] { ChannelAftertouch, PolyPressure, BoostModulation };
        }

        public class TouchStripModes
        {
            public const string Modulation = "Modulation";
            public const string PitchBend = "PitchBend";

            public static string[] Choices = new string[] { Modulation, PitchBend };
        }

        public class PedalModes
        {
            public const string FootSwitch = "Foot Switch (Hold 2 CC 69)";
            public const string RaiseSemitone = "Raise Semitone";
            public const string FootController = "Foot Controller (CC4)";

            public static string[] Choices = new string[] { FootSwitch, RaiseSemitone, FootController };
        }

        public string Output { get; set; }
        public string Layout { get; set; }
        public string Channel { get; set; }
        public string Scale { get; set; }
        public string Key { get; set; }
        public string Octave { get; set; }
        public int OctaveNumber { get { return Int32.Parse(Octave); } }
        public bool Debug { get; set; }
        public string Pressure { get; set; }
        public string TouchStripMode { get; set; }
        public string PedalMode { get; set; }

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
                    Output = (string)regKey.GetValue("Output", "");
                    Layout = (string)regKey.GetValue("Layout", ConfigValues.Layouts.InKey);
                    Channel = (string)regKey.GetValue("Channel", "1");
                    Scale = (string)regKey.GetValue("Scale", "Major");
                    Key = (string)regKey.GetValue("Key", "C");
                    Octave = (string)regKey.GetValue("Octave", "3");
                    Debug = (int)regKey.GetValue("Debug", 0) != 0;
                    Pressure = (string)regKey.GetValue("Pressure", ConfigValues.Pressures.ChannelAftertouch);
                    TouchStripMode = (string)regKey.GetValue("TouchStripMode", TouchStripModes.Modulation);
                    PedalMode = (string)regKey.GetValue("PedalMode", PedalModes.FootSwitch);
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
                regKey.SetValue("Debug", Debug ? 1 : 0);
                regKey.SetValue("Pressure", Pressure);
                regKey.SetValue("TouchStripMode", TouchStripMode);
                regKey.SetValue("PedalMode", PedalMode);
            }
        }
    }
}
