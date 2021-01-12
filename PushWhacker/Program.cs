using System;
using System.Threading;
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

            if (!PushDisplay.Open())
            {
                MessageBox.Show("Can't open Push display");
            }
            else
            {
                MidiProcessor midiProcessor = new MidiProcessor(configValues);
                if (!midiProcessor.StartProcessing())
                {
                    MessageBox.Show("Can't start Push Midi Processing");
                }
                else
                {
                    var running = true;
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        while (running)
                        {
                            PushDisplay.RefreshDisplay();
                            Thread.Sleep(40);
                        }
                    }).Start();

                    var applicationContext = new CustomApplicationContext(configValues, midiProcessor);
                    Application.Run(applicationContext);

                    running = false;
                    Thread.Sleep(100);
                    PushDisplay.WriteText("Goodbye");
                    PushDisplay.RefreshDisplay();
                    Thread.Sleep(1000);

                    midiProcessor.StopProcessing();
                    PushDisplay.Close();
                }
            }
        }

    }
}
