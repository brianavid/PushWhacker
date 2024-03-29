﻿using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PushWhacker
{
    public class MidiProcessor
    {
        static ConfigValues configValues;
        const string SourceMidi = "MIDIIN2 (Ableton Push 2)";
        const string LightsMidi = "MIDIOUT2 (Ableton Push 2)";

        const int KsOct0Note = 1000;
        const int KsOct1Note = 1001;
        const int KsOct2Note = 1002;
        const int ScalarNote = 2000;

        static MidiIn midiIn = null;
        static MidiOut midiOut = null;
        static MidiOut midiLights = null;
        static bool switchedScale = false;
        static int KsFirstNote = 0;
        static int lastModulationValue = 0;
        static int[] scaleNoteMapping;
        static int[] scaleNoteMapping2;
        static Dictionary<int, int> touchCCs;
        static Dictionary<int, int> ccValues;
        static Dictionary<int, int> notesOn = new Dictionary<int, int>();
        static int[] ScalerKsNotes = new int[] { -1, 47, 45, 43, 41, 40, 38, 36 };
        static int[] ScalerPadNotes = new int[] { 48, 50, 52, 53, 55, 57, 59, 60 };
        static int[] WhiteNotes = new int[] { 12, 14, 16, 17, 19, 21, 23, 24 };
        static DateTime revertToDefaultTime = DateTime.MaxValue;
        static DateTime ccDisplayTime = DateTime.MaxValue;
        static string ccDisplayMessage = null;

        static bool inUserMode = false;

        static ConfigValues.PedalCalibrationId currentPedalCalibration;

        static Dictionary<int, int> noteColours = new Dictionary<int, int>();

        public static Dictionary<string, int[]> Scales { get; private set; }
        public static string[] ScaleNames;

        private static string storeButtonPressed = null;
        private static DateTime storeButtonTime;
        public static string storeInstruction = null;
        private static bool setupButtonPressed;
        private static int midiChannelCCValue = 0;
        private static int lastAfterTouchPressure;

        public MidiProcessor(ConfigValues values)
        {
            configValues = values;
            Scales = new Dictionary<string, int[]>();
            Scales["Major"] = new int[] { 2, 2, 1, 2, 2, 2, 1 };
            Scales["Minor"] = new int[] { 2, 1, 2, 2, 1, 2, 2 };
            Scales["Harmonic Minor"] = new int[] { 2, 1, 2, 2, 1, 3, 1 };
            Scales["Melodic Minor (Asc)"] = new int[] { 2, 1, 2, 2, 2, 2, 1 };
            Scales["Melodic Minor (Desc)"] = new int[] { 2, 1, 2, 2, 1, 2, 2 };
            Scales["Ionian"] = new int[] { 2, 2, 1, 2, 2, 2, 1 };
            Scales["Dorian"] = new int[] { 2, 1, 2, 2, 2, 1, 2 };
            Scales["Phrygian"] = new int[] { 1, 2, 2, 2, 1, 2, 2 };
            Scales["Lydian"] = new int[] { 2, 2, 2, 1, 2, 2, 1 };
            Scales["Mixolydian"] = new int[] { 2, 2, 1, 2, 2, 1, 2 };
            Scales["Aeolian"] = new int[] { 2, 1, 2, 2, 1, 2, 2 };
            Scales["Locrian"] = new int[] { 1, 2, 2, 1, 2, 2, 2 };
            Scales["Dorian ♭2"] = new int[] { 1, 2, 2, 2, 2, 1, 2 };
            Scales["Lydian Augmented"] = new int[] { 2, 2, 2, 2, 1, 2, 1 };
            Scales["Lydian Dominant"] = new int[] { 2, 2, 2, 1, 2, 1, 2 };
            Scales["Mixolydian ♭6"] = new int[] { 2, 2, 1, 2, 1, 2, 2 };
            Scales["Locrian ♮2"] = new int[] { 2, 1, 2, 1, 2, 2, 2 };
            Scales["Altered"] = new int[] { 1, 2, 1, 2, 2, 2, 2 };
            Scales["Phrygian Dominant"] = new int[] { 1, 3, 1, 2, 1, 2, 2 };
            Scales["Ukrainian Dorian"] = new int[] { 2, 1, 3, 1, 2, 1, 2 };
            Scales["Hungarian (Gypsy) Minor"] = new int[] { 2, 1, 3, 1, 1, 3, 1 };
            Scales["Dbl Harm (Gypsy) Major"] = new int[] { 1, 3, 1, 2, 1, 3, 1 };
            Scales["Major Pentatonic"] = new int[] { 2, 2, 3, 2, 3 };
            Scales["Minor Pentatonic"] = new int[] { 3, 2, 2, 3, 2 };
            Scales["Whole Tone"] = new int[] { 2, 2, 2, 2, 2, 2 };
            Scales["Octatonic WH"] = new int[] { 2, 1, 2, 1, 2, 1, 2, 1 };
            Scales["Octatonic HW"] = new int[] { 1, 2, 1, 2, 1, 2, 1, 2 };
            
            ScaleNames = Scales.Keys.ToArray();


            touchCCs = new Dictionary<int, int>();
            touchCCs[0] = 71;
            touchCCs[1] = 72;
            touchCCs[2] = 73;
            touchCCs[3] = 74;
            touchCCs[4] = 75;
            touchCCs[5] = 76;
            touchCCs[6] = 77;
            touchCCs[7] = 78;
            touchCCs[9] = 15;
            touchCCs[10] = 14;

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
            ccValues[Push.Buttons.BrightnessCC] = configValues.Brightness;
            ccValues[Push.Buttons.ChannelCC] = 0; // CC 79 with Setup button pressed
        }

        public bool StartProcessing()
        {
            if (midiIn != null)
            {
                return true;
            }

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
                    if (MidiOut.DeviceInfo(device).ProductName == LightsMidi)
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
                    FinishInput();
                    return false;
                }
            }
            catch (Exception)
            {
                FinishInput();
                return false;
            }

            inUserMode = !configValues.UserModeOnly;
            SendSysex(new byte[] { 0x0A, (byte)(inUserMode ? 1 : 0) });

            if (inUserMode)
            {
                SetScaleNotesAndLights();
                SetLedBrightness(ccValues[Push.Buttons.BrightnessCC]);
            }

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

            midiIn.CreateSysexBuffers(512, 10);
            midiIn.SysexMessageReceived += midiIn_SysexMessageReceived;
            midiIn.Start();
        }

        private static void FinishInput()
        {
            if (midiIn != null)
            {
                midiIn.Stop();
                midiIn.MessageReceived -= midiIn_MessageReceived;
                midiIn.SysexMessageReceived -= midiIn_SysexMessageReceived;
                midiIn.ErrorReceived -= midiIn_ErrorReceived;
                midiIn.Reset();
                midiIn.Close();
                midiIn = null;
            }
            if (midiLights != null)
            {
                midiLights.Close();
                midiLights = null;
            }
            if (midiOut != null)
            {
                midiOut.Close();
                midiOut = null;
            }
        }

        static void SetScaleNotesAndLights(bool displayMode = true)
        {
            if (displayMode)
            {
                DisplayMode();
            }

            scaleNoteMapping = null;
            scaleNoteMapping2 = null;

            switch (configValues.Layout)
            {
                case ConfigValues.Layouts.InKey:
                    SetScaleNotesAndLightsInKey(3);
                    break;

                case ConfigValues.Layouts.InKeyPlusKS:
                    SetScaleNotesAndLightsInKey(3, 3);
                    OverlayKeySwitchPads();
                    break;

                case ConfigValues.Layouts.Scaler:
                    SetScaleNotesAndLightsScalar();
                    break;

                case ConfigValues.Layouts.Chromatic:
                    SetScaleNotesAndLightsChromatic(5);
                    break;

                case ConfigValues.Layouts.ChromaticPlusKS:
                    SetScaleNotesAndLightsChromatic(5, 3);
                    OverlayKeySwitchPads();
                    break;

                case ConfigValues.Layouts.Strummer:
                    SetScaleNotesAndLightsStrummer();
                    break;

                case ConfigValues.Layouts.Drums:
                    SetScaleNotesAndLightsDrum();
                    break;

                case ConfigValues.Layouts.BigDrums:
                    SetScaleNotesAndLightsBigDrums();
                    break;

                case ConfigValues.Layouts.WhiteKeys:
                    SetScaleNotesAndLightsWhiteKeys();
                    break;
            }

            for (int cc = 1; cc < 127; cc++)
            {
                if (!Push.Buttons.ReservedCCs.Contains(cc))
                {
                    SetButtonLED(cc, Push.Buttons.Coloured.Contains(cc) ? Push.Colours.Blue : Push.Colours.White);
                }
            }

            SetButtonLED(Push.Buttons.OctaveDown, Push.Colours.On);
            SetButtonLED(Push.Buttons.OctaveUp, Push.Colours.On);
            SetButtonLED(Push.Buttons.KeyDown, Push.Colours.On);
            SetButtonLED(Push.Buttons.KeyUp, Push.Colours.On);
            SetButtonLED(Push.Buttons.ScaleLeft, Push.Colours.On);
            SetButtonLED(Push.Buttons.ScaleRight, Push.Colours.On);
            SetButtonLED(Push.Buttons.LayoutLeft, Push.Colours.On);
            SetButtonLED(Push.Buttons.LayoutRight, Push.Colours.On);

            SetButtonLED(Push.Buttons.ResetScale, Push.Colours.On);
            SetButtonLED(Push.Buttons.ResetLayout, Push.Colours.On);

            SetButtonLED(Push.Buttons.Store_A, Push.Colours.On);
            SetButtonLED(Push.Buttons.Store_B, Push.Colours.On);

            SetButtonLED(Push.Buttons.ToggleTouchStrip, Push.Colours.On);
            SetButtonLED(Push.Buttons.ShowInfo, Push.Colours.On);
            if (configValues.UserModeOnly)
            {
                SetButtonLED(Push.Buttons.UserMode, Push.Colours.On);
            }

            SetPressureMode(configValues.Pressure == ConfigValues.Pressures.PolyPressure);
            SetTouchStripMode(configValues.TouchStripMode == ConfigValues.TouchStripModes.PitchBend);
            SetPedalMode(configValues.PedalMode == ConfigValues.PedalModes.FootController);
        }

        private static void DisplayMode()
        {
            switch (configValues.Layout)
            {
                case ConfigValues.Layouts.InKey:
                case ConfigValues.Layouts.InKeyPlusKS:
                    if (configValues.PedalMode == ConfigValues.PedalModes.SwitchScale && 
                        configValues.SwitchedScale != configValues.Scale &&
                        (switchedScale || configValues.SwitchedScale != ScaleNames[0]))
                    {
                        var scale1 = switchedScale ? configValues.SwitchedScale : configValues.Scale;
                        var scale2 = switchedScale ? configValues.Scale : configValues.SwitchedScale;
                        PushDisplay.WriteText($"{configValues.Key}{configValues.Octave} {scale1} [{scale2}]", 32);
                    }
                    else
                    {
                        PushDisplay.WriteText($"{configValues.Key}{configValues.Octave} {configValues.Scale}");
                    }
                    break;
                case ConfigValues.Layouts.Chromatic:
                case ConfigValues.Layouts.ChromaticPlusKS:
                    PushDisplay.WriteText($"{configValues.Key}{configValues.Octave} {configValues.Scale}");
                    break;
                default:
                    PushDisplay.WriteText($"{configValues.Layout}");
                    break;
            }
        }

        static void SetScaleNotesAndLightsInKey(int cycleWidth, int offset = 0)
        {
            var intervals = Scales[configValues.Scale];
            var intervals2 = Scales[configValues.SwitchedScale];
            var intervalsFromRoot = Enumerable.Range(0, intervals.Length + 1).Select(i => intervals.Take(i).Sum()).ToArray();
            var intervals2FromRoot = Enumerable.Range(0, intervals2.Length + 1).Select(i => intervals2.Take(i).Sum()).ToArray();
            var keyNote = configValues.Keys[configValues.Key];

            int FindRowStartPos()
            {
                int pos;
                for (pos = 0; keyNote + intervalsFromRoot[intervalsFromRoot.Length - pos - 1] >= 12; pos++) { }
                return intervalsFromRoot.Length - pos;
            }

            int nearestToC4 = -1;
            int rowStartPos = configValues.FixLayout ? FindRowStartPos() : intervals.Length;
            int startingNote = keyNote + (12 * configValues.OctaveNumber) + intervalsFromRoot[rowStartPos];
            int startingNote2 = keyNote + (12 * configValues.OctaveNumber) + intervals2FromRoot[rowStartPos];
            int targetNote = startingNote;
            int rowStartNote = startingNote;
            int targetNote2 = startingNote2;
            int rowStartNote2 = startingNote2;

            scaleNoteMapping = new int[64];
            if (configValues.SwitchedScale != configValues.Scale && intervals.Length == intervals2.Length)
            {
                scaleNoteMapping2 = new int[64];
            }
            else
            {
                intervals2 = intervals;
            }

            for (int i = 0; i < 64-offset; i++)
            {
                int col = i % 8;
                int pos = (rowStartPos + col) % intervals.Length;
                int sourceNote = Push.FirstPad + i + offset;
                bool isOctaveNote = (targetNote - keyNote) % 12 == 0;
                bool isC4 = targetNote == nearestToC4 || nearestToC4 < 0 && targetNote >= 60;

                if (isC4) nearestToC4 = targetNote;

                scaleNoteMapping[i+offset] = targetNote;
                if (scaleNoteMapping2 != null)
                {
                    scaleNoteMapping2[i + offset] = targetNote2;
                }

#if false
                            if (configValues.Debug)
                            {
                                System.Diagnostics.Trace.WriteLine(String.Format("[{0},{1}] -> {2} {3}{4} {5}", row, col, targetNote,
                                                                                    configValues.Keys.Keys.ToArray()[targetNote % 12], targetNote / 12,
                                                                                    isOctaveNote ? "OCT" : ""));
                            }

#endif
                targetNote += intervals[pos];
                targetNote2 += intervals2[pos];
                if (col == 7)
                {
                    targetNote = rowStartNote;
                    targetNote2 = rowStartNote2;
                    for (int j = 0; j < cycleWidth; j++)
                    {
                        pos = (rowStartPos + j) % intervals.Length;
                        targetNote += intervals[pos];
                        targetNote2 += intervals2[pos];
                    }
                    rowStartNote = targetNote;
                    rowStartNote2 = targetNote2;
                    rowStartPos = (rowStartPos + cycleWidth) % intervals.Length;
                }

                SetPadLED(sourceNote, isC4 ? Push.Colours.DullGreen : isOctaveNote ? Push.Colours.Blue : Push.Colours.White);
            }
        }

        static void SetScaleNotesAndLightsChromatic(int cycleWidth, int offset = 0)
        {
            var keyNote = configValues.Keys[configValues.Key];
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

            int startingNote = (configValues.FixLayout ? 0 : keyNote) + (12 * configValues.OctaveNumber+12);
            int targetNote = startingNote;
            int rowStartNote = startingNote;

            scaleNoteMapping = new int[64];
            for (int i = 0; i < 64-offset; i++)
            {
                int col = i % 8;
                int sourceNote = Push.FirstPad + i + offset;
                bool isOctaveNote = targetNote % 12 == keyNote;
                bool isScaleNote = isInScale[(targetNote - keyNote) % 12];
                bool isC4 = targetNote == 60;

                if (isC4) nearestToC4 = targetNote;

                scaleNoteMapping[i+offset] = targetNote;

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

                SetPadLED(sourceNote, isC4 ? Push.Colours.DullGreen : isOctaveNote ? Push.Colours.Blue : isScaleNote ? Push.Colours.White : Push.Colours.DarkGrey);
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
            for (var i = 0; i < 64; i++)
            {
                scaleNoteMapping[i] = -1;
            }
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

        static void SetScaleNotesAndLightsScalar()
        {
            scaleNoteMapping = new int[64];
            for (var i = 0; i < 64; i++)
            {
                scaleNoteMapping[i] = -1;
            }
            for (var col = 0; col < 8; col++)
            {
                DefineSpecificButton(0, col, -1, Push.Colours.Black);
            }
            for (var row = 1; row < 8; row++)
            {
                for (var col = 0; col < 8; col++)
                {
                    DefineSpecificButton(row, col, ScalarNote, row % 2 == 0 ? Push.Colours.DullBlue : Push.Colours.White);
                }
            }
        }

        static void SetScaleNotesAndLightsWhiteKeys()
        {
            scaleNoteMapping = new int[64];
            for (var i = 0; i < 64; i++)
            {
                scaleNoteMapping[i] = -1;
            }
            for (var row = 0; row < 8; row++)
            {
                for (var col = 0; col < 8; col++)
                {
                    DefineSpecificButton(row, col, WhiteNotes[col] + row*12, row % 2 == 0 ? Push.Colours.DullBlue : Push.Colours.White);
                }
            }
        }

        static void SetScaleNotesAndLightsBigDrums()
        {
            var note = 36;
            scaleNoteMapping = new int[64];
            for (var row = 0; row < 8; row+=2)
            {
                for (var col = 0; col < 8; col+=2)
                {
                    var colour = col / 2 % 2 == row / 2 % 2 ? Push.Colours.White : Push.Colours.DullBlue;
                    var n = note++;
                    DefineSpecificButton(row, col, n, colour);
                    DefineSpecificButton(row+1, col, n, colour);
                    DefineSpecificButton(row, col+1, n, colour);
                    DefineSpecificButton(row+1, col+1, n, colour);
                }
            }
        }

        static void OverlayKeySwitchPads()
        {
            var note = KsFirstNote;
            for (var row = 0; row < 7; row++)
            {
                for (var col = 0; col < 3; col++)
                {
                    DefineSpecificButton(row, col, note++, Push.Colours.DullRed);
                }
            }
            DefineSpecificButton(7, 0, KsOct0Note, KsFirstNote == 0 ? Push.Colours.DullGreen : Push.Colours.DullYellow);
            DefineSpecificButton(7, 1, KsOct1Note, KsFirstNote == 12 ? Push.Colours.DullGreen : Push.Colours.DullYellow);
            DefineSpecificButton(7, 2, KsOct2Note, KsFirstNote == 24 ? Push.Colours.DullGreen : Push.Colours.DullYellow);
        }

        static void SetPadLED(int sourceNote, int colour)
        {
            var ledNote = new NoteOnEvent(0, 1, sourceNote, colour, 0);
            midiLights.Send(ledNote.GetAsShortMessage());
            noteColours[sourceNote] = colour;
        }

        static void ModifyPadLED(int sourceNote, int colour)
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
#if DEBUG
            System.Diagnostics.Trace.WriteLine(String.Format("ERROR {0}  {1,-10} {2}",
                FormatTimeStamp(e.Timestamp), FormatMidiBytes(e.RawMessage), e.MidiEvent));
#endif
        }


        static void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            var midiEvent = e.MidiEvent;

            if (midiEvent == null || midiEvent.CommandCode == MidiCommandCode.AutoSensing)
            {
                if (DateTime.Now >= revertToDefaultTime)
                {
                    DisplayMode();
                    revertToDefaultTime = DateTime.MaxValue;
                }
                if (storeButtonPressed != null)
                {
                    if (DateTime.Now > storeButtonTime.AddMilliseconds(2000))
                    {
                        if (storeButtonPressed == "")
                        {
                            foreach (var store in Push.StoreageButtonLabels.Keys)
                            {
                                configValues.SaveStore(store);
                                PushDisplay.WriteText("All stores reset");
                                storeButtonPressed = null;
                            }
                        }
                        else
                        {
                            configValues.SaveStore(storeButtonPressed);
                            PushDisplay.WriteText("Stored: " + Push.StoreageButtonLabels[storeButtonPressed]);
                            storeButtonPressed = null;
                        }
                    }
                    else if (storeInstruction != null && DateTime.Now > storeButtonTime.AddMilliseconds(500))
                    {
                        PushDisplay.WriteText(storeInstruction);
                        storeInstruction = null;
                    }
                }
#if DISPLAY_CC_VALUES
                else if (ccDisplayMessage != null && DateTime.Now > ccDisplayTime.AddMilliseconds(100))
                {
                    PushDisplay.WriteText(ccDisplayMessage);
                    ccDisplayTime = DateTime.Now;
                }
#endif          
                return;
            }

            if (midiEvent.CommandCode == MidiCommandCode.ControlChange)
            {
                var ccEvent = midiEvent as ControlChangeEvent;

                if (setupButtonPressed && (byte)ccEvent.Controller == Push.Buttons.BrightnessCC)
                {
                    ccEvent.Controller = (MidiController)Push.Buttons.ChannelCC;
                }

                if (ccValues.ContainsKey((byte)ccEvent.Controller))
                {
                    int delta = ccEvent.ControllerValue >= 64 ? ccEvent.ControllerValue - 128 : ccEvent.ControllerValue;
                    int ccValue = ccValues[(byte)ccEvent.Controller] + delta;

                    ccValue = Math.Max(Math.Min(ccValue, 127), 0);

                    ccEvent.ControllerValue = ccValue;
                    ccValues[(byte)ccEvent.Controller] = ccValue;
                    if (touchCCs.Values.ToList().Contains((byte)ccEvent.Controller))
                    {
                        ccDisplayMessage = $"CC {ccEvent.Controller} : {(ccValue * 100 + 100) / 128}%";
                    }
                }

                if (ccEvent.Controller == MidiController.Modulation)
                {
                    lastModulationValue = ccEvent.ControllerValue;
                    midiLights.Send(midiEvent.GetAsShortMessage());
                    if (configValues.TouchStripMode == ConfigValues.TouchStripModes.Expression)
                    {
                        ccEvent.Controller = MidiController.Expression;
                    }
                    if (configValues.TouchStripMode == ConfigValues.TouchStripModes.ModAndExp)
                    {
                        var exprcc = ccEvent.Clone() as ControlChangeEvent;
                        exprcc.Controller = MidiController.Expression;
                        midiOut.Send(exprcc.GetAsShortMessage());
                    }
                }

                switch ((byte)ccEvent.Controller)
                {
                    case Push.Buttons.FootSwitch:
                        if (configValues.PedalMode == ConfigValues.PedalModes.SwitchScale)
                        {
                            switch (configValues.Layout)
                            {
                                case ConfigValues.Layouts.InKey:
                                case ConfigValues.Layouts.InKeyPlusKS:
                                    switchedScale = (ccEvent.ControllerValue >= 64);    // Assuming correct calibration for polarity
                                    DisplayMode();
                                    break;
                            }
                            return;
                        }
                        break;

                    case Push.Buttons.OctaveDown:
                        if (ccEvent.ControllerValue > 64)
                        {
                            configValues.Octave = (Math.Max(configValues.OctaveNumber, 0) - 1).ToString();
                            configValues.Save();
                            SetScaleNotesAndLights();
                        }
                        return;

                    case Push.Buttons.OctaveUp:
                        if (ccEvent.ControllerValue > 64)
                        {
                            configValues.Octave = (Math.Min(configValues.OctaveNumber, 6) + 1).ToString();
                            configValues.Save();
                            SetScaleNotesAndLights();
                        }
                        return;

                    case Push.Buttons.KeyDown:
                        if (ccEvent.ControllerValue > 64)
                        {
                            int keyChangeAmmount = configValues.KeyChangeFifths ? 5 : 11;
                            int keyIndex = configValues.Keys[configValues.Key];
                            configValues.Key = configValues.Keys.Keys.ToArray()[(keyIndex + keyChangeAmmount) % 12];
                            configValues.Save();
                            SetScaleNotesAndLights();
                        }
                        return;

                    case Push.Buttons.KeyUp:
                        if (ccEvent.ControllerValue > 64)
                        {
                            int keyChangeAmmount = configValues.KeyChangeFifths ? 7 : 1;
                            int keyIndex = configValues.Keys[configValues.Key];
                            configValues.Key = configValues.Keys.Keys.ToArray()[(keyIndex + keyChangeAmmount) % 12];
                            configValues.Save();
                            SetScaleNotesAndLights();
                        }
                        return;

                    case Push.Buttons.LayoutLeft:
                        if (ccEvent.ControllerValue > 64)
                        {
                            int layoutIndex = Array.IndexOf(ConfigValues.Layouts.Choices, configValues.Layout);
                            if (layoutIndex > 0)
                            {
                                configValues.Layout = ConfigValues.Layouts.Choices[layoutIndex - 1];
                                configValues.Save();
                                SetScaleNotesAndLights(false);
                            }
                            PushDisplay.WriteText(configValues.Layout);
                        }
                        else
                        {
                            RequestDefaultDisplay();
                        }
                        return;

                    case Push.Buttons.LayoutRight:
                        if (ccEvent.ControllerValue > 64)
                        {
                            int layoutIndex = Array.IndexOf(ConfigValues.Layouts.Choices, configValues.Layout);
                            if (layoutIndex < ConfigValues.Layouts.Choices.Count() - 1)
                            {
                                configValues.Layout = ConfigValues.Layouts.Choices[layoutIndex + 1];
                                configValues.Save();
                                SetScaleNotesAndLights(false);
                            }
                            PushDisplay.WriteText(configValues.Layout);
                        }
                        else
                        {
                            RequestDefaultDisplay();
                        }
                        return;

                    case Push.Buttons.ScaleLeft:
                        if (ccEvent.ControllerValue > 64)
                        {
                            if (switchedScale)
                            {
                                int scaleIndex = Array.IndexOf(ScaleNames, configValues.SwitchedScale);
                                if (scaleIndex > 0)
                                {
                                    configValues.SwitchedScale = ScaleNames[scaleIndex - 1];
                                    configValues.Save();
                                    SetScaleNotesAndLights();
                                }
                            }
                            else
                            {
                                bool sameScale = configValues.SwitchedScale == configValues.Scale;
                                int scaleIndex = Array.IndexOf(ScaleNames, configValues.Scale);
                                if (scaleIndex > 0)
                                {
                                    configValues.Scale = ScaleNames[scaleIndex - 1];
                                    if (sameScale)
                                    {
                                        configValues.SwitchedScale = configValues.Scale;
                                    }
                                    configValues.Save();
                                    SetScaleNotesAndLights();
                                }
                            }
                        }
                        return;

                    case Push.Buttons.ScaleRight:
                        if (ccEvent.ControllerValue > 64)
                        {
                            if (switchedScale)
                            {
                                int scaleIndex = Array.IndexOf(ScaleNames, configValues.SwitchedScale);
                                if (scaleIndex < ScaleNames.Count() - 1)
                                {
                                    configValues.SwitchedScale = ScaleNames[scaleIndex + 1];
                                    configValues.Save();
                                    SetScaleNotesAndLights();
                                }
                            }
                            else
                            {
                                bool sameScale = configValues.SwitchedScale == configValues.Scale;
                                int scaleIndex = Array.IndexOf(ScaleNames, configValues.Scale);
                                if (scaleIndex < ScaleNames.Count() - 1)
                                {
                                    configValues.Scale = ScaleNames[scaleIndex + 1];
                                    if (sameScale)
                                    {
                                        configValues.SwitchedScale = configValues.Scale;
                                    }
                                    configValues.Save();
                                    SetScaleNotesAndLights();
                                }
                            }
                        }
                        return;

                    case Push.Buttons.ResetScale:
                        if (ccEvent.ControllerValue > 64)
                        {
                            configValues.Scale = ScaleNames[0];
                            configValues.SwitchedScale = ScaleNames[0];
                            configValues.Key = "C";
                            configValues.Octave = "2";
                            configValues.Save();
                            SetScaleNotesAndLights();
                            storeButtonPressed = "";
                            storeButtonTime = DateTime.Now;
                            storeInstruction = "Hold to clear stores ...";
                        }
                        else
                        {
                            storeButtonPressed = null;
                            DisplayMode();
                        }
                        return;

                    case Push.Buttons.ResetLayout:
                        if (ccEvent.ControllerValue > 64)
                        {
                            configValues.Layout = ConfigValues.Layouts.Choices[0]; ;
                            configValues.Save();
                            SetScaleNotesAndLights(false);
                            PushDisplay.WriteText(configValues.Layout);
                        }
                        else
                        {
                            RequestDefaultDisplay();
                        }
                        return;

                    case Push.Buttons.CyclePressure:
                        if (ccEvent.ControllerValue > 64)
                        {
                            var currentPressureMode = configValues.Pressure;
                            var currentPressureIndex = Array.FindIndex(ConfigValues.Pressures.Choices, v => v == currentPressureMode);
                            currentPressureIndex = (currentPressureIndex + 1) % ConfigValues.Pressures.Choices.Length;
                            configValues.Pressure = ConfigValues.Pressures.Choices[currentPressureIndex];
                            configValues.Save();
                            SetPressureMode(configValues.Pressure == ConfigValues.Pressures.PolyPressure);
                            PushDisplay.WriteText($"Pressure: {configValues.Pressure}");
                        }
                        else
                        {
                            RequestDefaultDisplay();
                        }
                        return;

                    case Push.Buttons.ToggleTouchStrip:
                        if (ccEvent.ControllerValue > 64)
                        {
                            var currentTouchStripMode = configValues.TouchStripMode;
                            var currentTouchStripIndex = Array.FindIndex(ConfigValues.TouchStripModes.Choices, v => v == currentTouchStripMode);
                            currentTouchStripIndex = (currentTouchStripIndex + 1) % ConfigValues.TouchStripModes.Choices.Length;
                            configValues.TouchStripMode = ConfigValues.TouchStripModes.Choices[currentTouchStripIndex];
                            configValues.Save();
                            SetTouchStripMode(configValues.TouchStripMode == ConfigValues.TouchStripModes.PitchBend);
                            PushDisplay.WriteText($"Touch Strip: {configValues.TouchStripMode}");
                        }
                        else
                        {
                            RequestDefaultDisplay();
                        }
                        return;

                    case Push.Buttons.ShowInfo:
                        if (ccEvent.ControllerValue > 64)
                        {
                            setupButtonPressed = true;
                            RequestDeviceIdInfo();
                        }
                        else
                        {
                            setupButtonPressed = false;
                            DisplayMode();
                        }
                        return;

                    case Push.Buttons.UserMode:
                        if (configValues.UserModeOnly && ccEvent.ControllerValue > 64)
                        {
                            if (inUserMode)
                            {
                                PushDisplay.WriteText("");
                                ClearLights();
                            }
                            inUserMode = !inUserMode;
                            SendSysex(new byte[] { 0x0A, (byte)(inUserMode ? 1 : 0) });
                            if (inUserMode)
                            {
                                SetScaleNotesAndLights();
                            }
                        }
                        return;

#if EXPERIMENTAL_MMC
                    case Push.Buttons.RecordButton:
                        if (ccEvent.ControllerValue > 64)
                        {
                            playing = true;
                            recording = true;
                            midiOut.SendBuffer(new byte[] { 0xF0, 0x7F, 0x01, 0x06, 0x06, 0xF7 });
                            SetButtonLED(Push.Buttons.RecordButton, Push.Colours.Red);
                        }
                        return;

                    case Push.Buttons.PlayButton:
                        if (ccEvent.ControllerValue > 64)
                        {
                            playing = !playing;
                            recording = false;
                            onTime = DateTime.Now;
                            midiOut.SendBuffer(new byte[] { 0xF0, 0x7F, 0x01, 0x06, (byte)(playing ? 0x02 : 0x01), 0xF7 });
                            SetButtonLED(Push.Buttons.PlayButton, playing ? Push.Colours.Green : Push.Colours.White);
                            SetButtonLED(Push.Buttons.RecordButton, Push.Colours.White);
                        }
                        else
                        {
                            if (!playing)
                            {
                                if ((DateTime.Now - onTime).TotalMilliseconds > 1000)
                                {
                                    midiOut.SendBuffer(new byte[] { 0xF0, 0x7F, 0x01, 0x06, 0x05, 0xF7 });
                                }
                            }
                        }
                        return;
#endif
                    case Push.Buttons.Store_A:
                        HandleStoreButton(ccEvent, "A");
                        return;

                    case Push.Buttons.Store_B:
                        HandleStoreButton(ccEvent, "B");
                        return;

                    case Push.Buttons.BrightnessCC:
                        SetLedBrightness(ccEvent.ControllerValue);
                        configValues.Save();
                        return;

                    case Push.Buttons.ChannelCC:
                        midiChannelCCValue = ccEvent.ControllerValue;
                        midiEvent.Channel = midiChannelCCValue / 8 + 1;
                        PushDisplay.WriteText($"MIDI Channel = {midiEvent.Channel}");
                        return;
                }

                if (Push.Buttons.ReservedCCs.Contains((byte)ccEvent.Controller))
                {
                    //  Other than those defined as passed thru, all others are reserved and ignored
                    return;
                }

                if (ccEvent.ControllerValue == 0 && configValues.CcNoRelease && 
                    (int)ccEvent.Controller >= 102 && (int)ccEvent.Controller <= 109)
                {
                    //  Do not pass through button releases
                    return;
                }
            }

            midiEvent.Channel = midiChannelCCValue / 8 + 1;

            if (midiEvent.CommandCode == MidiCommandCode.NoteOn || midiEvent.CommandCode == MidiCommandCode.NoteOff)
            {
                var noteEvent = midiEvent as NoteEvent;
                var padNoteNumber = noteEvent.NoteNumber;

                if (padNoteNumber < Push.FirstPad || padNoteNumber > Push.LastPad)
                {
                    if (touchCCs.Keys.Contains(padNoteNumber))
                    {
                        var touchCC = touchCCs[padNoteNumber];
                        var touchVal = (ccValues[touchCC] * 100 + 100) / 128;
                        if (midiEvent.CommandCode == MidiCommandCode.NoteOn && noteEvent.Velocity >= 64)
                        {
                            ccDisplayMessage = $"CC {touchCC} : {touchVal}%";
                            ccDisplayTime = DateTime.Now;
                        }
                        else
                        {
                            ccDisplayMessage = null;
                            DisplayMode();
                        }
                    }
                    return;
                }

                var noteEncoded = 
                    (switchedScale && scaleNoteMapping2 != null) ?
                        scaleNoteMapping2[padNoteNumber - Push.FirstPad] :
                        scaleNoteMapping[padNoteNumber - Push.FirstPad];

                if (noteEncoded == ScalarNote)
                {
                    var row = (padNoteNumber - Push.FirstPad) / 8;
                    var col = (padNoteNumber - Push.FirstPad) % 8;
                    if (midiEvent.CommandCode == MidiCommandCode.NoteOn)
                    {
                        var n1 = new NoteEvent(0, noteEvent.Channel, MidiCommandCode.NoteOn, ScalerKsNotes[row], noteEvent.Velocity);
                        var n2 = new NoteEvent(0, noteEvent.Channel, MidiCommandCode.NoteOff, ScalerKsNotes[row], 0);
                        var n3 = new NoteEvent(0, noteEvent.Channel, MidiCommandCode.NoteOn, ScalerPadNotes[col], noteEvent.Velocity);
                        midiOut.Send(n1.GetAsShortMessage());
                        System.Threading.Thread.Sleep(1);
                        midiOut.Send(n2.GetAsShortMessage());
                        System.Threading.Thread.Sleep(1);
                        midiOut.Send(n3.GetAsShortMessage());
                        ModifyPadLED(padNoteNumber, Push.Colours.Red);
                    }
                    else
                    {
                        var n = new NoteEvent(0, noteEvent.Channel, MidiCommandCode.NoteOff, ScalerPadNotes[col], 0);
                        midiOut.Send(n.GetAsShortMessage());
                        ModifyPadLED(padNoteNumber, noteColours[padNoteNumber]);
                    }
                    return;
                }

                if (midiEvent.CommandCode == MidiCommandCode.NoteOn)
                {
                    if (noteEncoded >= 128)
                    {
                        switch (noteEncoded)
                        {
                            case KsOct0Note:
                                KsFirstNote = 0;
                                break;
                            case KsOct1Note:
                                KsFirstNote = 12;
                                break;
                            case KsOct2Note:
                                KsFirstNote = 24;
                                break;
                            default:
                                return;
                        }
                        SetScaleNotesAndLights();
                        return;
                    }
                    
                    if (noteEncoded < 0)
                    {
                        return;
                    }

                    noteEvent.NoteNumber = noteEncoded;
                    notesOn[padNoteNumber] = noteEvent.NoteNumber;
                    for (var nn = Push.FirstPad; nn <= Push.LastPad; nn++)
                    {
                        if (scaleNoteMapping[nn - Push.FirstPad] == scaleNoteMapping[padNoteNumber - Push.FirstPad])
                        {
                            ModifyPadLED(nn, Push.Colours.Red);
                        }
                    }
                }
                else if (notesOn.ContainsKey(padNoteNumber))
                {
                    for (var nn = Push.FirstPad; nn <= Push.LastPad; nn++)
                    {
                        if (scaleNoteMapping[nn - Push.FirstPad] == scaleNoteMapping[padNoteNumber - Push.FirstPad])
                        {
                            ModifyPadLED(nn, noteColours[nn]);
                        }
                    }
                    noteEvent.NoteNumber = notesOn[padNoteNumber];
                    notesOn.Remove(padNoteNumber);
                    //if (notesOn.ContainsValue(noteEvent.NoteNumber)) return;
                }
                else
                {
                    return;
                }
            }

            if (midiEvent.CommandCode == MidiCommandCode.ChannelAfterTouch &&
                configValues.Pressure == ConfigValues.Pressures.BoostModulation)
            {
                var afterTouchEvent = midiEvent as ChannelAfterTouchEvent;
                int newModValue = lastModulationValue + ((127 - lastModulationValue) * afterTouchEvent.AfterTouchPressure) / 128;
                midiEvent = new ControlChangeEvent(0, midiEvent.Channel, MidiController.Modulation, newModValue);
                midiLights.Send(midiEvent.GetAsShortMessage());
                if (configValues.TouchStripMode == ConfigValues.TouchStripModes.Expression)
                {
                    (midiEvent as ControlChangeEvent).Controller = MidiController.Expression;
                }
                if (configValues.TouchStripMode == ConfigValues.TouchStripModes.ModAndExp)
                {
                    var exprcc = midiEvent.Clone() as ControlChangeEvent;
                    exprcc.Controller = MidiController.Expression;
                    midiOut.Send(exprcc.GetAsShortMessage());
                }
            }

            if (midiEvent.CommandCode == MidiCommandCode.ChannelAfterTouch &&
               configValues.Pressure == ConfigValues.Pressures.PitchBendUp)
            {
                var afterTouchEvent = midiEvent as ChannelAfterTouchEvent;
                if (afterTouchEvent.AfterTouchPressure > 0 &&
                    lastAfterTouchPressure > afterTouchEvent.AfterTouchPressure+5)
                {
                    lastAfterTouchPressure = afterTouchEvent.AfterTouchPressure;
                    return;
                }
                lastAfterTouchPressure = afterTouchEvent.AfterTouchPressure;
                midiEvent = new PitchWheelChangeEvent(0, midiEvent.Channel,
                                                         (afterTouchEvent.AfterTouchPressure << 6) + (afterTouchEvent.AfterTouchPressure >> 1));
            }

            if (midiEvent.CommandCode == MidiCommandCode.KeyAfterTouch)
            {
                var polyPressureEvent = midiEvent as NoteEvent;
                var padNoteNumber = polyPressureEvent.NoteNumber;
                if (notesOn.ContainsKey(padNoteNumber))
                {
                    polyPressureEvent.NoteNumber = notesOn[padNoteNumber];
                }
                else
                {
                    return;
                }
            }

#if DEBUG
            System.Diagnostics.Trace.WriteLine(String.Format("{0}  {1,-10} {2}",
                FormatTimeStamp(e.Timestamp), FormatMidiBytes(e.RawMessage), midiEvent));
#endif

            midiOut.Send(midiEvent.GetAsShortMessage());

            void HandleStoreButton(ControlChangeEvent ccEvent, string store)
            {
                if (ccEvent.ControllerValue > 64)
                {
                    storeButtonPressed = store;
                    storeButtonTime = DateTime.Now;
                    storeInstruction = "Hold to store ...";
                }
                else
                {
                    configValues.LoadStore(store);
                    SetScaleNotesAndLights();
                    storeButtonPressed = null;
                }
            }
        }

        private static void RequestDefaultDisplay()
        {
            revertToDefaultTime = DateTime.Now.AddSeconds(2);
        }

        static void midiIn_SysexMessageReceived(object sender, MidiInSysexMessageEventArgs e)
        {
            var sysexMessage = e.SysexBytes;

            StringBuilder sb = new StringBuilder();
            foreach( var b in sysexMessage)
            {
                sb.AppendFormat("{0:x02} ", b);
            }
            System.Diagnostics.Trace.WriteLine($"Sysex {sb}");

            if (sysexMessage.Length == 23 && sysexMessage[1] == 0x7E && sysexMessage[2] == 0x01 && sysexMessage[3] == 0x06 && sysexMessage[4] == 0x02)
            {
                var swRevision = sysexMessage[14] + (sysexMessage[14] << 7);
                var serialNo = sysexMessage[16] + (sysexMessage[17] << 7) + (sysexMessage[18] << 14) + (sysexMessage[19] << 21) + (sysexMessage[20] << 28);

                PushDisplay.WriteText(String.Format("Push2 ver {0}.{1}.{2}  Sn:{3}  Rev:{4}", sysexMessage[12], sysexMessage[13], swRevision, serialNo, sysexMessage[21]), 36);
            }
            else if (sysexMessage.Length > 8 && sysexMessage[1] == 0x00 && sysexMessage[2] == 0x21 && sysexMessage[3] == 0x1d && sysexMessage[4] == 0x01 && sysexMessage[5] == 0x01)
            {
                switch (sysexMessage[6])
                {
                    case 0x0A:
                        inUserMode = sysexMessage[7] != 0;
                        break;
                    case 0x13:
                        int val;
                        switch (currentPedalCalibration)
                        {
                            case ConfigValues.PedalCalibrationId.SwitchOff:
                            case ConfigValues.PedalCalibrationId.SwitchOn:
                                val = sysexMessage[13] + (sysexMessage[14] << 7);
                                configValues.PedalCalibrations[currentPedalCalibration] = val;
                                break;
                            case ConfigValues.PedalCalibrationId.ControlToe:
                            case ConfigValues.PedalCalibrationId.ControlHeel:
                                val = sysexMessage[11] + (sysexMessage[12] << 7);
                                configValues.PedalCalibrations[currentPedalCalibration] = val;
                                break;
                        }

                        configValues.Save();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                PushDisplay.WriteText(sb.ToString(), 16);
            }
        }

        private static void RequestDeviceIdInfo()
        {
            midiLights.SendBuffer(new byte[] { 0xF0, 0x7E, 0x01, 0x06, 0x01, 0xF7 });
        }

        private static void SetLedBrightness(int brightness)
        {
            //  Use the rh controller to set the LED brightness
            SendSysex(new byte[] { 0x06, (byte)brightness });
            SendSysex(new byte[] { 0x08, (byte)((brightness % 64) * 2), (byte)(brightness >= 64 ? 1 : 0) });
            configValues.Brightness = brightness;
        }

        private static void SetPressureMode(bool isPoly)
        {
            SendSysex(new byte[] { 0x1E, isPoly ? (byte)1 : (byte)0 });
        }

        private static void SetTouchStripMode(bool isPitchBend)
        {
            SendSysex(new byte[] { 0x17, isPitchBend ? (byte)0x68 : (byte)0x05 });
        }

        public static void CalibrateFootPedal(ConfigValues.PedalCalibrationId pedalCalibration)
        {
            currentPedalCalibration = pedalCalibration;
            SendSysex(new byte[] { 0x13, 0x09 });
        }

        private static void SetPedalMode(bool isContinuous)
        {
            if (isContinuous)
            {
                var heel = configValues.PedalCalibrations[ConfigValues.PedalCalibrationId.ControlHeel];
                var toe  = configValues.PedalCalibrations[ConfigValues.PedalCalibrationId.ControlToe];
                if (heel == toe)
                {
                    toe = 4095;
                    heel = 0;
                }
                SendSysex(new byte[] { 0x30, 0x03, 0x7F, 0x00, 0x00 });
                SendSysex(new byte[] { 0x30, 0x02, 0x04, 0x00, 0x00 });
                SendSysex(new byte[] { 0x31, 0x02, (byte)(heel & 0x7F), (byte)(heel >> 7), (byte)(toe & 0x7F), (byte)(toe >> 7) });
                SendSysex(new byte[] { 0x32, 0x02, 0x00, 0x00, 0x00, 0x08, 0x00, 0x10, 0x00, 0x18, 0x00, });
                SendSysex(new byte[] { 0x32, 0x02, 0x04, 0x20, 0x00, 0x28, 0x00, 0x30, 0x00, 0x38, 0x00, });
                SendSysex(new byte[] { 0x32, 0x02, 0x08, 0x40, 0x00, 0x48, 0x00, 0x50, 0x00, 0x58, 0x00, });
                SendSysex(new byte[] { 0x32, 0x02, 0x0C, 0x60, 0x00, 0x68, 0x00, 0x70, 0x00, 0x78, 0x00, });
                SendSysex(new byte[] { 0x32, 0x02, 0x10, 0x00, 0x01, 0x08, 0x01, 0x10, 0x01, 0x18, 0x01, });
                SendSysex(new byte[] { 0x32, 0x02, 0x14, 0x20, 0x01, 0x28, 0x01, 0x30, 0x01, 0x38, 0x01, });
                SendSysex(new byte[] { 0x32, 0x02, 0x18, 0x40, 0x01, 0x48, 0x01, 0x50, 0x01, 0x58, 0x01, });
                SendSysex(new byte[] { 0x32, 0x02, 0x1C, 0x60, 0x01, 0x68, 0x01, 0x70, 0x01, 0x78, 0x01, });
            }
            else
            {
                var off  = configValues.PedalCalibrations[ConfigValues.PedalCalibrationId.SwitchOff];
                var on   = configValues.PedalCalibrations[ConfigValues.PedalCalibrationId.SwitchOn];
                if (off == on)
                {
                    on = 4095;
                    off = 0;
                }
                SendSysex(new byte[] { 0x30, 0x02, 0x7F, 0x00, 0x00 });
                SendSysex(new byte[] { 0x30, 0x03, 0x45, 0x00, 0x00 });
                SendSysex(new byte[] { 0x31, 0x03, (byte)(off & 0x7F), (byte)(off >> 7), (byte)(on & 0x7F), (byte)(on >> 7) }); 
                SendSysex(new byte[] { 0x32, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, });
                SendSysex(new byte[] { 0x32, 0x03, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, });
                SendSysex(new byte[] { 0x32, 0x03, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, });
                SendSysex(new byte[] { 0x32, 0x03, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, });
                SendSysex(new byte[] { 0x32, 0x03, 0x10, 0xFF, 0x01, 0xFF, 0x01, 0xFF, 0x01, 0xFF, 0x01, });
                SendSysex(new byte[] { 0x32, 0x03, 0x14, 0xFF, 0x01, 0xFF, 0x01, 0xFF, 0x01, 0xFF, 0x01, });
                SendSysex(new byte[] { 0x32, 0x03, 0x18, 0xFF, 0x01, 0xFF, 0x01, 0xFF, 0x01, 0xFF, 0x01, });
                SendSysex(new byte[] { 0x32, 0x03, 0x1C, 0xFF, 0x01, 0xFF, 0x01, 0xFF, 0x01, 0xFF, 0x01, });
            }
        }

        private static void SendSysex(byte[] message)
        {
            midiLights.SendBuffer(new byte[] { 0xF0, 0x00, 0x21, 0x1D, 0x01, 0x01 });
            midiLights.SendBuffer(message);
            midiLights.SendBuffer(new byte[] { 0xF7 });
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
