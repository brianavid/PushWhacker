using System;
using System.Windows.Forms;
using NAudio.Midi;

namespace PushWhacker
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ConfigValues configValues = new ConfigValues();
            configValues.Load();

            MidiProcessor midiProcessor = new MidiProcessor(configValues);
            if (!midiProcessor.StartProcessing())
            {
                MessageBox.Show("Can't start Push Midi Processing");
            }
            else
            {
                var applicationContext = new CustomApplicationContext(configValues, midiProcessor);
                Application.Run(applicationContext);

                midiProcessor.StopProcessing();
            }
        }

    }
}
