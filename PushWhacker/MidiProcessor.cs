﻿using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PushWhacker
{
    public class MidiProcessor
    {
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
            try
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
            }
            catch (Exception)
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
            switch (configValues.Layout)
            {
                case ConfigValues.Layouts.InKey:
                    SetScaleNotesAndLightsInKey(3);
                    break;

                case ConfigValues.Layouts.Scaler:
                    SetScaleNotesAndLightsInKey(7);
                    break;

                case ConfigValues.Layouts.Chromatic:
                    SetScaleNotesAndLightsChromatic(5);
                    break;
                case ConfigValues.Layouts.Linear:
                    SetScaleNotesAndLightsChromatic(8);
                    break;

                case ConfigValues.Layouts.Strummer:
                    SetScaleNotesAndLightsStrummer();
                    break;

                case ConfigValues.Layouts.Drums:
                    SetScaleNotesAndLightsDrum();
                    break;
            }

            SetButtonLED(Push.Buttons.OctaveDown, Push.Colours.On);
            SetButtonLED(Push.Buttons.OctaveUp, Push.Colours.On);
            SetButtonLED(Push.Buttons.PageLeft, Push.Colours.On);
            SetButtonLED(Push.Buttons.PageRight, Push.Colours.On);

            SetButtonLED(Push.Buttons.ScaleMajor, configValues.Scale == "Major" ? Push.Colours.On : Push.Colours.Dim);
            SetButtonLED(Push.Buttons.ScaleMinor, configValues.Scale == "Minor" ? Push.Colours.On : Push.Colours.Dim);

            SetButtonLED(Push.Buttons.LayoutInKey, configValues.Layout == ConfigValues.Layouts.InKey ? Push.Colours.Red : Push.Colours.White);
            SetButtonLED(Push.Buttons.LayoutChromatic, configValues.Layout == ConfigValues.Layouts.Chromatic ? Push.Colours.Red : Push.Colours.White);
            SetButtonLED(Push.Buttons.LayoutScaler, configValues.Layout == ConfigValues.Layouts.Scaler ? Push.Colours.Red : Push.Colours.White);
            SetButtonLED(Push.Buttons.LayoutStrummer, configValues.Layout == ConfigValues.Layouts.Strummer ? Push.Colours.Red : Push.Colours.White);
            SetButtonLED(Push.Buttons.LayoutDrums, configValues.Layout == ConfigValues.Layouts.Drums ? Push.Colours.Red : Push.Colours.White);

        }

        static void SetScaleNotesAndLightsInKey(int cycleWidth)
        {
            var intervals = Scales[configValues.Scale];
            int nearestToC4 = -1;
            int startingNote = configValues.Keys[configValues.Key] + (12 * configValues.OctaveNumber);
            int targetNote = startingNote;
            int rowStartNote = startingNote;
            int rowStartPos = 0;

            scaleNoteMapping = new int[64];
            for (int i = 0; i < 64; i++)
            {
                int col = i % 8;
                int pos = (rowStartPos + col) % intervals.Length;
                int sourceNote = Push.FirstPad + i;
                bool isOctaveNote = (targetNote - startingNote) % 12 == 0;
                bool isC4 = targetNote == nearestToC4 || nearestToC4 < 0 && targetNote >= 60;

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
                    targetNote = rowStartNote;
                    for (int j = 0; j < cycleWidth; j++)
                    {
                        pos = (rowStartPos + j) % intervals.Length;
                        targetNote += intervals[pos];
                    }
                    rowStartNote = targetNote;
                    rowStartPos = (rowStartPos + cycleWidth) % intervals.Length;
                }

                SetPadLED(sourceNote, isC4 ? Push.Colours.Green : isOctaveNote ? Push.Colours.Blue : Push.Colours.White);
            }
        }

        static void SetScaleNotesAndLightsChromatic(int cycleWidth)
        {
            var intervals = Scales[configValues.Scale];
            int nearestToC4 = -1;
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

            scaleNoteMapping = new int[64];
            for (int i = 0; i < 64; i++)
            {
                int col = i % 8;
                int sourceNote = Push.FirstPad + i;
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
                if (cycleWidth < 8 && col == 7)
                {
                    targetNote = rowStartNote + cycleWidth;
                    rowStartNote = targetNote;
                }

                SetPadLED(sourceNote, isC4 ? Push.Colours.Green : isOctaveNote ? Push.Colours.Blue : isScaleNote ? Push.Colours.White : Push.Colours.DarkGrey);
            }
        }

        static void DefineSpecificButton(int row, int col, int note, int colour)
        {
            int i = row * 8 + col;
            int sourceNote = Push.FirstPad + i;
            scaleNoteMapping[i] = note;
            SetPadLED(sourceNote, colour);
        }

        static void SetScaleNotesAndLightsStrummer()
        {
            ClearLights();
            scaleNoteMapping = new int[64];
            DefineSpecificButton(0, 4, 62, Push.Colours.DullGreen);
            DefineSpecificButton(0, 5, 64, Push.Colours.DullGreen);
            DefineSpecificButton(0, 6, 65, Push.Colours.DullGreen);
            DefineSpecificButton(1, 5, 67, Push.Colours.DullGreen);
            DefineSpecificButton(1, 6, 69, Push.Colours.DullGreen);
            DefineSpecificButton(1, 7, 71, Push.Colours.DullGreen);
            DefineSpecificButton(2, 5, 67, Push.Colours.DullGreen);
            DefineSpecificButton(2, 6, 69, Push.Colours.DullGreen);
            DefineSpecificButton(2, 7, 71, Push.Colours.DullGreen);
            DefineSpecificButton(3, 4, 72, Push.Colours.DullBlue);
            DefineSpecificButton(3, 5, 74, Push.Colours.DullBlue);
            DefineSpecificButton(3, 6, 76, Push.Colours.DullBlue);
            DefineSpecificButton(3, 7, 77, Push.Colours.DullBlue);
            DefineSpecificButton(4, 4, 78, Push.Colours.DullBlue);
            DefineSpecificButton(4, 5, 79, Push.Colours.DullBlue);
            DefineSpecificButton(4, 6, 81, Push.Colours.DullBlue);
            DefineSpecificButton(4, 7, 83, Push.Colours.DullBlue);
            DefineSpecificButton(5, 4, 60, Push.Colours.DullYellow);
            DefineSpecificButton(5, 5, 61, Push.Colours.DullYellow);
            DefineSpecificButton(5, 6, 63, Push.Colours.DullYellow);
            DefineSpecificButton(5, 7, 66, Push.Colours.DullYellow);
            DefineSpecificButton(6, 4, 68, Push.Colours.DullYellow);
            DefineSpecificButton(6, 5, 70, Push.Colours.DullYellow);
            DefineSpecificButton(6, 6, 73, Push.Colours.DullYellow);
            DefineSpecificButton(6, 7, 75, Push.Colours.DullYellow);
            DefineSpecificButton(0, 0, 36, Push.Colours.White);
            DefineSpecificButton(0, 1, 37, Push.Colours.White);
            DefineSpecificButton(0, 2, 38, Push.Colours.White);
            DefineSpecificButton(0, 3, 39, Push.Colours.White);
            DefineSpecificButton(1, 0, 40, Push.Colours.White);
            DefineSpecificButton(1, 1, 41, Push.Colours.White);
            DefineSpecificButton(1, 2, 42, Push.Colours.White);
            DefineSpecificButton(1, 3, 43, Push.Colours.White);
            DefineSpecificButton(2, 0, 44, Push.Colours.White);
            DefineSpecificButton(2, 1, 45, Push.Colours.White);
            DefineSpecificButton(2, 2, 46, Push.Colours.White);
            DefineSpecificButton(2, 3, 47, Push.Colours.White);
            DefineSpecificButton(3, 0, 48, Push.Colours.White);
            DefineSpecificButton(3, 1, 49, Push.Colours.White);
            DefineSpecificButton(3, 2, 50, Push.Colours.White);
            DefineSpecificButton(3, 3, 51, Push.Colours.White);
            DefineSpecificButton(4, 0, 52, Push.Colours.White);
            DefineSpecificButton(4, 1, 53, Push.Colours.White);
            DefineSpecificButton(4, 2, 54, Push.Colours.White);
            DefineSpecificButton(4, 3, 55, Push.Colours.White);
            DefineSpecificButton(5, 0, 56, Push.Colours.White);
            DefineSpecificButton(5, 1, 57, Push.Colours.White);
            DefineSpecificButton(5, 2, 58, Push.Colours.White);
            DefineSpecificButton(5, 3, 59, Push.Colours.White);
        }

        static void SetScaleNotesAndLightsDrum()
        {
            var note = 36;
            var groupColours = new int[] { Push.Colours.White, Push.Colours.DullBlue, Push.Colours.DullGreen, Push.Colours.DullYellow };

            scaleNoteMapping = new int[64];
            for (var group = 0; group < 4; group++)
            {
                var colour = groupColours[group];
                for (var row = 0; row < 4; row++)
                {
                    for (var col = 0; col < 4; col++)
                    {
                        DefineSpecificButton((group % 2) * 4 + row, (group / 2) * 4 + col,  note++, colour);
                    }
                }
            }
        }



        static void SetPadLED(int sourceNote, int colour)
        {
            var ledNote = new NoteOnEvent(0, 1, sourceNote, colour, 0);
            midiLights.Send(ledNote.GetAsShortMessage());
        }

        static void SetButtonLED(int button, int colour)
        {
            var ledButton = new ControlChangeEvent(0, 1, (MidiController)button, colour);
            midiLights.Send(ledButton.GetAsShortMessage());
        }


        static void ClearLights()
        {
            for (int i = 0; i < 64; i++)
            {
                int sourceNote = Push.FirstPad + i;
                SetPadLED(sourceNote, Push.Colours.Black);
            }
            for (int i = 0; i < 128; i++)
            {
                SetButtonLED(i, Push.Colours.Off);
            }
        }


        static void midiIn_ErrorReceived(object sender, MidiInMessageEventArgs e)
        {
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
                    case Push.Buttons.FootSwitch:
                        footSwitchPressed = (ccEvent.ControllerValue < 64);
                        return;

                    case Push.Buttons.OctaveDown:
                        if (ccEvent.ControllerValue > 64)
                        {
                            configValues.Octave = (Math.Max(configValues.OctaveNumber, 1) - 1).ToString();
                            configValues.Save();
                            SetScaleNotesAndLights();
                        }
                        return;

                    case Push.Buttons.OctaveUp:
                        if (ccEvent.ControllerValue > 64)
                        {
                            configValues.Octave = (Math.Min(configValues.OctaveNumber, 7) + 1).ToString();
                            configValues.Save();
                            SetScaleNotesAndLights();
                        }
                        return;

                    case Push.Buttons.PageLeft:
                        if (ccEvent.ControllerValue > 64)
                        {
                            int keyIndex = configValues.Keys[configValues.Key];
                            if (keyIndex > 0)
                            {
                                configValues.Key = configValues.Keys.Keys.ToArray()[keyIndex - 1];
                                configValues.Save();
                                DisplayKeyOnPads();
                            }
                        }
                        else
                        {
                            SetScaleNotesAndLights();
                        }
                        return;

                    case Push.Buttons.PageRight:
                        if (ccEvent.ControllerValue > 64)
                        {
                            int keyIndex = configValues.Keys[configValues.Key];
                            if (keyIndex < 11)
                            {
                                configValues.Key = configValues.Keys.Keys.ToArray()[keyIndex + 1];
                                configValues.Save();
                                DisplayKeyOnPads();
                            }
                        }
                        else
                        {
                            SetScaleNotesAndLights();
                        }
                        return;

                    case Push.Buttons.LayoutInKey:
                    case Push.Buttons.LayoutChromatic:
                    case Push.Buttons.LayoutScaler:
                    case Push.Buttons.LayoutStrummer:
                    case Push.Buttons.LayoutDrums:
                        if (ccEvent.ControllerValue > 64)
                        {
                            switch ((byte)ccEvent.Controller)
                            {
                                case Push.Buttons.LayoutInKey:
                                    configValues.Layout = ConfigValues.Layouts.InKey;
                                    break;
                                case Push.Buttons.LayoutChromatic:
                                    configValues.Layout = ConfigValues.Layouts.Chromatic;
                                    break;
                                case Push.Buttons.LayoutScaler:
                                    configValues.Layout = ConfigValues.Layouts.Scaler;
                                    break;
                                case Push.Buttons.LayoutStrummer:
                                    configValues.Layout = ConfigValues.Layouts.Strummer;
                                    break;
                                case Push.Buttons.LayoutDrums:
                                    configValues.Layout = ConfigValues.Layouts.Drums;
                                    break;
                            }
                            configValues.Save();
                            SetScaleNotesAndLights();
                        }
                        return;

                    case Push.Buttons.ScaleMajor:
                    case Push.Buttons.ScaleMinor:
                        if (ccEvent.ControllerValue > 64)
                        {
                            switch ((byte)ccEvent.Controller)
                            {
                                case Push.Buttons.ScaleMajor:
                                    configValues.Scale = "Major";
                                    break;
                                case Push.Buttons.ScaleMinor:
                                    configValues.Scale = "Minor";
                                    break;
                            }
                            configValues.Save();
                            DisplayKeyOnPads();
                        }
                        else
                        { 
                            SetScaleNotesAndLights();
                        }
                        return;

                    case Push.Buttons.BrightnessCC:
                        SetLedBrightness(ccEvent.ControllerValue);
                        return;
                }
            }

            if (e.MidiEvent.CommandCode == MidiCommandCode.NoteOn || e.MidiEvent.CommandCode == MidiCommandCode.NoteOff)
            {
                var noteOnEvent = e.MidiEvent as NoteEvent;
                var padNoteNumber = noteOnEvent.NoteNumber;

                if (padNoteNumber < Push.FirstPad || padNoteNumber > Push.LastPad)
                {
                    return;
                }

                if (e.MidiEvent.CommandCode == MidiCommandCode.NoteOn || !notesOn.ContainsKey(padNoteNumber))
                {
                    noteOnEvent.NoteNumber = scaleNoteMapping[noteOnEvent.NoteNumber - Push.FirstPad];
                    if (noteOnEvent.NoteNumber == 0)
                    {
                        return;
                    }

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

            if (configValues.Debug)
            {
                System.Diagnostics.Trace.WriteLine(String.Format("{0}  {1,-10} {2}",
                    FormatTimeStamp(e.Timestamp), FormatMidiBytes(e.RawMessage), e.MidiEvent));
            }

            midiOut.Send(e.MidiEvent.GetAsShortMessage());
        }

        static void DisplayKeyOnPads()
        {
            for (int i = 0; i < 64; i++)
            {
                int sourceNote = Push.FirstPad + i;
                SetPadLED(sourceNote, LetterDisplay.IsBit(configValues.Key, i) ? Push.Colours.White : Push.Colours.Black);
            }
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

        public static void DisplayColours(int bank)
        {
            for (int i = 0; i < 64; i++)
            {
                SetPadLED(Push.FirstPad + i, bank * 64 + i);
            }
        }
    }
}
