using NAudio.Midi;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PushWhacker
{
    public class MidiProcessor
    {
        static Logger logger = LogManager.GetLogger("PushWhacker");
        static ConfigValues configValues;
        const string SourceMidi = "Ableton Push 2";

        static MidiIn midiIn = null;
        static MidiOut midiOut = null;
        static MidiOut midiLights = null;
        static bool footSwitchPressed = false;
        static int[] scaleNoteMapping;
        static Dictionary<int, int> ccValues;
        static Dictionary<int, int> notesOn = new Dictionary<int, int>();

        public static Dictionary<string, int[]> Scales { get; private set; }

        public MidiProcessor(ConfigValues values)
        {
            configValues = values;
            Scales = new Dictionary<string, int[]>();
            Scales["Major"] = new int[] { 2, 2, 1, 2, 2, 2, 1 };
            Scales["Minor"] = new int[] { 2, 1, 2, 2, 1, 2, 2 };
            Scales["Ionian"] = new int[] { 2, 2, 1, 2, 2, 2, 1 };
            Scales["Dorian"] = new int[] { 2, 1, 2, 2, 2, 1, 2 };
            Scales["Phrygian "] = new int[] { 1, 2, 2, 2, 1, 2, 2 };
            Scales["Lydian "] = new int[] { 2, 2, 2, 1, 2, 2, 1 };
            Scales["Mixolydian "] = new int[] { 2, 2, 1, 2, 2, 1, 2 };
            Scales["Aeolian"] = new int[] { 2, 1, 2, 2, 1, 2, 2 };
            Scales["Locrian"] = new int[] { 1, 2, 2, 1, 2, 2, 2 };
            Scales["Major Pentatonic"] = new int[] { 2, 2, 3, 2, 3 };
            Scales["Minor Pentatonic"] = new int[] { 3, 2, 2, 3, 2 };
            Scales["Whole Tone"] = new int[] { 2, 2, 2, 2, 2, 2 };
            Scales["Octatonic WH"] = new int[] { 2, 1, 2, 1, 2, 1, 2, 1 };
            Scales["Octatonic HW"] = new int[] { 1, 2, 1, 2, 1, 2, 1, 2 };
            Scales["Hungarian (Gypsy) Minor"] = new int[] { 2, 1, 3, 1, 1, 3, 1 };
            Scales["Dbl Harm (Gypsy) Major "] = new int[] { 1, 3, 1, 2, 1, 3, 1 };

            ccValues = new Dictionary<int, int>();
            ccValues[14] = 0;
            ccValues[15] = 0;
            ccValues[71] = 0;
            ccValues[72] = 0;
            ccValues[73] = 0;
            ccValues[74] = 0;
            ccValues[75] = 0;
            ccValues[76] = 0;
            ccValues[77] = 0;
            ccValues[78] = 0;
            ccValues[79] = 64;
        }

        public bool StartProcessing()
        {
            for (int device = 0; device < MidiIn.NumberOfDevices; device++)
            {
                if (MidiIn.DeviceInfo(device).ProductName == SourceMidi)
                {
                    midiIn = new MidiIn(device);
                }
            }

            for (int device = 0; device < MidiOut.NumberOfDevices; device++)
            {
                if (MidiOut.DeviceInfo(device).ProductName == SourceMidi)
                {
                    midiLights = new MidiOut(device);
                }
            }

            if (!string.IsNullOrEmpty(configValues.Output))
            {
                for (int device = 0; device < MidiOut.NumberOfDevices; device++)
                {
                    if (MidiOut.DeviceInfo(device).ProductName == configValues.Output)
                    {
                        midiOut = new MidiOut(device);
                    }
                }
            }

            if (midiIn == null || midiLights == null)
            {
                return false;
            }

            SetScaleNotesAndLights();
            SetLedBrightness(ccValues[79]);

            if (midiOut != null)
            {
                StartInput();
            }

            return true;
        }

        public void StopProcessing()
        {
            ClearLights();
            FinishInput();
        }

        private static void StartInput()
        {
            midiIn.MessageReceived += midiIn_MessageReceived;
            midiIn.ErrorReceived += midiIn_ErrorReceived;
            midiIn.Start();
        }

        private static void FinishInput()
        {
            if (midiIn != null)
            {
                midiIn.Stop();
                midiIn.MessageReceived -= midiIn_MessageReceived;
                midiIn.ErrorReceived -= midiIn_ErrorReceived;
                midiIn.Dispose();
                midiIn = null;
            }
            if (midiLights != null)
            {
                midiLights.Dispose();
                midiLights = null;
            }
            if (midiOut != null)
            {
                midiOut.Dispose();
                midiOut = null;
            }
        }

        static void SetScaleNotesAndLights()
        {
            var intervals = Scales[configValues.Scale];
            int nearestToC4 = -1;
            scaleNoteMapping = new int[64];
            switch (configValues.Layout)
            {
                case "In Key":
                case "Scaler":
                    {
                        int startingNote = configValues.Keys[configValues.Key] + (12 * configValues.OctaveNumber);
                        int targetNote = startingNote;
                        int rowStartNote = startingNote;
                        int rowStartPos = 0;
                        for (int i = 0; i < 64; i++)
                        {
                            int row = i / 8;
                            int col = i % 8;
                            int pos = (rowStartPos + col) % intervals.Length;
                            int sourceNote = 36 + i;
                            bool isOctaveNote = (targetNote - startingNote) % 12 == 0;
                            bool isC4 = targetNote == nearestToC4 || nearestToC4 <0 && targetNote >= 60;

                            if (isC4) nearestToC4 = targetNote;

                            scaleNoteMapping[i] = targetNote;

#if false
                            if (configValues.Debug)
                            {
                                System.Diagnostics.Trace.WriteLine(String.Format("[{0},{1}] -> {2} {3}{4} {5}", row, col, targetNote,
                                                                                    configValues.Keys.Keys.ToArray()[targetNote % 12], targetNote / 12,
                                                                                    isOctaveNote ? "OCT" : ""));
                            }

#endif
                            targetNote += intervals[pos];
                            if (col == 7)
                            {
                                int width = configValues.Layout == "Scaler" ? 7 : 3;
                                targetNote = rowStartNote;
                                for (int j = 0; j < width; j++)
                                {
                                    pos = (rowStartPos + j) % intervals.Length;
                                    targetNote += intervals[pos];
                                }
                                rowStartNote = targetNote;
                                rowStartPos = (rowStartPos + width) % intervals.Length;
                            }

                            var ledNote = new NoteOnEvent(0, 1, sourceNote, isC4 ? 126 : isOctaveNote ? 125 : 122, 0);
                            midiLights.Send(ledNote.GetAsShortMessage());
                        }
                        break;
                    }

                case "Chromatic":
                case "Linear":
                    {
                        var isInScale = new bool[12];
                        int chromaticNote = 0;
                        for (var i = 0; i < intervals.Length; i++)
                        {
                            isInScale[chromaticNote++] = true;
                            for (var j = 1; j < intervals[i]; j++)
                            {
                                isInScale[chromaticNote++] = false;
                            }
                        }

                        int startingNote = configValues.Keys[configValues.Key] + (12 * configValues.OctaveNumber);
                        int targetNote = startingNote;
                        int rowStartNote = startingNote;
                        for (int i = 0; i < 64; i++)
                        {
                            int row = i / 8;
                            int col = i % 8;
                            int sourceNote = 36 + i;
                            bool isOctaveNote = (targetNote - startingNote) % 12 == 0;
                            bool isScaleNote = isInScale[(targetNote - startingNote) % 12];
                            bool isC4 = targetNote == nearestToC4 || nearestToC4 < 0 && targetNote >= 60;

                            if (isC4) nearestToC4 = targetNote;

                            scaleNoteMapping[i] = targetNote;

#if false
                            if (configValues.Debug)
                            {
                                System.Diagnostics.Trace.WriteLine(String.Format("[{0},{1}] -> {2} {3}{4} {5}", row, col, targetNote,
                                                                                    configValues.Keys.Keys.ToArray()[targetNote % 12], targetNote / 12,
                                                                                    isOctaveNote ? "OCT" : isScaleNote ? "Scale" : ""));
                            }

#endif
                            targetNote += 1;
                            if (configValues.Layout == "Chromatic" && col == 7)
                            {
                                targetNote = rowStartNote + 5;
                                rowStartNote = targetNote;
                            }

                            var ledNote = new NoteOnEvent(0, 1, sourceNote, isC4 ? 126 : isOctaveNote ? 125 : isScaleNote ? 122 : 123, 0);
                            midiLights.Send(ledNote.GetAsShortMessage());
                        }
                        break;
                    }
            }

            var ledOct = new ControlChangeEvent(0, 1, (MidiController)54, 127);
            midiLights.Send(ledOct.GetAsShortMessage());

            ledOct = new ControlChangeEvent(0, 1, (MidiController)55, 127);
            midiLights.Send(ledOct.GetAsShortMessage());
        }

        void ClearLights()
        {
            for (int i = 0; i < 64; i++)
            {
                int sourceNote = 36 + i;
                var ledNote = new NoteOnEvent(0, 1, sourceNote, 0, 0);
                midiLights.Send(ledNote.GetAsShortMessage());
            }
        }


        static void midiIn_ErrorReceived(object sender, MidiInMessageEventArgs e)
        {
            if (configValues.Log)
            {
                logger.Error(String.Format("{0}  {1,-10} {2}",
                    FormatTimeStamp(e.Timestamp), FormatMidiBytes(e.RawMessage), e.MidiEvent));
            }

            if (configValues.Debug)
            {
                System.Diagnostics.Trace.WriteLine(String.Format("ERROR {0}  {1,-10} {2}",
                    FormatTimeStamp(e.Timestamp), FormatMidiBytes(e.RawMessage), e.MidiEvent));
            }
        }


        static void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            if (e.MidiEvent == null || e.MidiEvent.CommandCode == MidiCommandCode.AutoSensing)
            {
                return;
            }

            if (e.MidiEvent.CommandCode == MidiCommandCode.ControlChange)
            {
                var ccEvent = e.MidiEvent as ControlChangeEvent;

                if (ccValues.ContainsKey((byte)ccEvent.Controller))
                {
                    int delta = ccEvent.ControllerValue >= 64 ? ccEvent.ControllerValue - 128 : ccEvent.ControllerValue;
                    int ccValue = ccValues[(byte)ccEvent.Controller] + delta;

                    ccValue = Math.Max(Math.Min(ccValue, 127), 0);

                    ccEvent.ControllerValue = ccValue;
                    ccValues[(byte)ccEvent.Controller] = ccValue;
                }

                switch ((byte)ccEvent.Controller)
                {
                    case 69:
                        footSwitchPressed = (ccEvent.ControllerValue < 64);
                        return;

                    case 54:
                        if (ccEvent.ControllerValue > 64)
                        {
                            configValues.Octave = (Math.Max(configValues.OctaveNumber, 1) - 1).ToString();
                            SetScaleNotesAndLights();
                        }
                        return;

                    case 55:
                        if (ccEvent.ControllerValue > 64)
                        {
                            configValues.Octave = (Math.Min(configValues.OctaveNumber, 7) + 1).ToString();
                            SetScaleNotesAndLights();
                        }
                        return;

                    case 79:
                        SetLedBrightness(ccEvent.ControllerValue);
                        return;
                }
            }

            if (e.MidiEvent.CommandCode == MidiCommandCode.NoteOn || e.MidiEvent.CommandCode == MidiCommandCode.NoteOff)
            {
                var noteOnEvent = e.MidiEvent as NoteEvent;
                var padNoteNumber = noteOnEvent.NoteNumber;

                if (padNoteNumber < 36 || padNoteNumber >= 100)
                {
                    return;
                }

                if (e.MidiEvent.CommandCode == MidiCommandCode.NoteOn)
                {
                    noteOnEvent.NoteNumber = scaleNoteMapping[noteOnEvent.NoteNumber - 36];

                    if (configValues.SemitonePedal && footSwitchPressed)
                    {
                        noteOnEvent.NoteNumber += 1;
                    }

                    notesOn[padNoteNumber] = noteOnEvent.NoteNumber;
                }
                else
                {
                    noteOnEvent.NoteNumber = notesOn[padNoteNumber];
                    notesOn.Remove(padNoteNumber);
                }
            }

            if (configValues.Log)
            {
                logger.Info(String.Format("{0}  {1,-10} {2}",
                    FormatTimeStamp(e.Timestamp), FormatMidiBytes(e.RawMessage), e.MidiEvent));
            }

            if (configValues.Debug)
            {
                System.Diagnostics.Trace.WriteLine(String.Format("{0}  {1,-10} {2}",
                    FormatTimeStamp(e.Timestamp), FormatMidiBytes(e.RawMessage), e.MidiEvent));
            }

            midiOut.Send(e.MidiEvent.GetAsShortMessage());
        }

        private static void SetLedBrightness(int brightness)
        {
            //  Use the rh controller to set the LED brightness
            var buffer = new byte[]
                             {
                                     0xF0, // MIDI excl start
                                     0x00, // Manu ID
                                     0x21, // 
                                     0x1D, // 
                                     0x01, // DevID
                                     0x01, // Prod Model ID
                                     0x06, // Command ID - set LED brightness
                                     (byte)brightness,
                                     0xF7, // MIDI excl end
                             };
            midiLights.SendBuffer(buffer);
        }

        static string FormatTimeStamp(int timeStampMs)
        {
            return String.Format("{0,2}:{1:D2}.{2:D3}", timeStampMs / 60000, (timeStampMs / 1000) % 60, timeStampMs % 1000);
        }

        static string FormatMidiBytes(int rawMessage, string basis = "")
        {
            string f = "";
            for (int b = 0; b < 3; b++)
            {
                f += String.Format("{0:X2} ", rawMessage % 256);
                rawMessage /= 256;
            }
            return f;
        }
    }
}
