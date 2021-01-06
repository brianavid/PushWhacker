using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushWhacker
{
    public class ConfigValues
    {
        public string Output { get; set; }
        public string Layout { get; set; }
        public string Channel { get; set; }
        public string Scale { get; set; }
        public string Key { get; set; }
        public string Octave { get; set; }
        public int OctaveNumber { get { return Int32.Parse(Octave); } }
        public bool Log { get; set; }
        public bool Debug { get; set; }

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
                    var output = regKey.GetValue("Output") as string;
                    if (!String.IsNullOrEmpty(output)) Output = output;

                    var layout = regKey.GetValue("Layout") as string;
                    if (!String.IsNullOrEmpty(layout)) Layout = layout;

                    var channel = regKey.GetValue("Channel") as string;
                    if (!String.IsNullOrEmpty(channel)) Channel = channel;

                    var scale = regKey.GetValue("Scale") as string;
                    if (!String.IsNullOrEmpty(scale)) Scale = scale;

                    var key = regKey.GetValue("Key") as string;
                    if (!String.IsNullOrEmpty(key)) Key = key;

                    var octave = regKey.GetValue("Octave") as string;
                    Octave = !String.IsNullOrEmpty(octave) ? octave : "3";

                    var log = regKey.GetValue("Log") as string;
                    Log = !String.IsNullOrEmpty(log);

                    var debug = regKey.GetValue("Debug") as string;
                    Debug = !String.IsNullOrEmpty(debug);
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
                regKey.SetValue("Log", Log ? "Yes" : "");
                regKey.SetValue("Debug", Debug ? "Yes" : "");
            }
        }
    }
}
