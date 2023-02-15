using Microsoft.Win32;
using System;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.ApplicationServices;

namespace PushWhacker
{
    static class Program
    {
        static MidiProcessor midiProcessor;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ConfigValues configValues = new ConfigValues();
            configValues.Load();

            if (!PushDisplay.Open() && !configValues.UserModeOnly)
            {
                MessageBox.Show("Can't open Push display");
            }
            else
            {
                midiProcessor = new MidiProcessor(configValues);

                if (!midiProcessor.StartProcessing())
                {
                    MessageBox.Show("Can't start Push Midi Processing\n\nThis is usually because another program or browser web page is using Midi and has locked all ports");
                }
                else
                {
                    PowerManager.IsMonitorOnChanged += new EventHandler(MonitorOnChanged);
                    SystemEvents.SessionEnding += new SessionEndingEventHandler(PushWacker_SesssionEndingEventHandler);
                    var applicationContext = new CustomApplicationContext(configValues, midiProcessor);
                    Application.Run(applicationContext);

                    midiProcessor.StopProcessing();
                }

                PushDisplay.Close();
            }
        }

        static void PushWacker_SesssionEndingEventHandler(object sender, EventArgs e)
        {
            midiProcessor.StopProcessing();
            PushDisplay.Close();
        }

        static void MonitorOnChanged(object sender, EventArgs e)
        {
            if (PowerManager.IsMonitorOn)
            {
                midiProcessor.StartProcessing();
            }
            else
            {
                PushDisplay.WriteText("");
                midiProcessor.StopProcessing();
            }
        }

    }
}
