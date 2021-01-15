using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PushWhacker
{
    public class MidiProcessor
    {
        static ConfigValues configValues;
        const string SourceMidi = "Ableton Push 2";

        const int KsOct0Note = 1000;
        const int KsOct1Note = 1001;
        const int KsOct2Note = 1002;
        const int ScalarNote = 2000;

        static MidiIn midiIn = null;
        static MidiOut midiOut = null;
        static MidiOut midiLights = null;
        static bool footSwitchPressed = false;
        static int KsFirstNote = 0;
        static int lastModulationValue = 0;
        static int[] scaleNoteMapping;
        static Dictionary<int, int> ccValues;
        static Dictionary<int, int> notesOn = new Dictionary<int, int>();
        static int[] ScalerKsNotes = new int[] { -1, 47, 45, 43, 41, 40, 38, 36 };
        static int[] ScalerPadNotes = new int[] { 48, 50, 52, 53, 55, 57, 59, 60 };

        public static Dictionary<string, int[]> Scales { get; private set; }
        public static string[] ScaleNames;

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
            
            ScaleNames = Scales.Keys.ToArray();


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
                    FinishInput();
                    return false;
                }
            }
            catch (Exception)
            {
                FinishInput();
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

        static void SetScaleNotesAndLights()
        {
            PushDisplay.WriteText($"{configValues.Key}{configValues.Octave} {configValues.Scale}");

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
            }

            SetButtonLED(Push.Buttons.OctaveDown, Push.Colours.On);
            SetButtonLED(Push.Buttons.OctaveUp, Push.Colours.On);
            SetButtonLED(Push.Buttons.PageLeft, Push.Colours.On);
            SetButtonLED(Push.Buttons.PageRight, Push.Colours.On);
            SetButtonLED(Push.Buttons.ScaleLeft, Push.Colours.On);
            SetButtonLED(Push.Buttons.ScaleRight, Push.Colours.On);

            SetButtonLED(Push.Buttons.ScaleMajor, configValues.Scale == "Major" ? Push.Colours.On : Push.Colours.Dim);
            SetButtonLED(Push.Buttons.ScaleMinor, configValues.Scale == "Minor" ? Push.Colours.On : Push.Colours.Dim);

            SetButtonLED(Push.Buttons.ToggleTouchStrip, Push.Colours.On);

            for (int i = 0; i < Push.Buttons.Layouts.Length; i++)
            {
                int colour = i >= ConfigValues.Layouts.Choices.Length ? Push.Colours.Off :
                             configValues.Layout == ConfigValues.Layouts.Choices[i] ? Push.Colours.Red : Push.Colours.White;
                SetButtonLED(Push.Buttons.Layouts[i], colour);
            }

            SetPressureMode(configValues.Pressure == ConfigValues.Pressures.PolyPressure);
            SetTouchStripMode(configValues.TouchStripMode == ConfigValues.TouchStripModes.PitchBend);
            SetPedalMode(configValues.PedalMode == ConfigValues.PedalModes.FootController);
        }

        static void SetScaleNotesAndLightsInKey(int cycleWidth, int offset = 0)
        {
            var intervals = Scales[configValues.Scale];
            int nearestToC4 = -1;
            int startingNote = configValues.Keys[configValues.Key] + (12 * configValues.OctaveNumber+12);
            int targetNote = startingNote;
            int rowStartNote = startingNote;
            int rowStartPos = 0;

            scaleNoteMapping = new int[64];
            for (int i = 0; i < 64-offset; i++)
            {
                int col = i % 8;
                int pos = (rowStartPos + col) % intervals.Length;
                int sourceNote = Push.FirstPad + i + offset;
                bool isOctaveNote = (targetNote - startingNote) % 12 == 0;
                bool isC4 = targetNote == nearestToC4 || nearestToC4 < 0 && targetNote >= 60;

                if (isC4) nearestToC4 = targetNote;

                scaleNoteMapping[i+offset] = targetNote;

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

        static void SetScaleNotesAndLightsChromatic(int cycleWidth, int offset = 0)
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

            int startingNote = configValues.Keys[configValues.Key] + (12 * configValues.OctaveNumber+12);
            int targetNote = startingNote;
            int rowStartNote = startingNote;

            scaleNoteMapping = new int[64];
            for (int i = 0; i < 64-offset; i++)
            {
                int col = i % 8;
                int sourceNote = Push.FirstPad + i + offset;
                bool isOctaveNote = (targetNote - startingNote) % 12 == 0;
                bool isScaleNote = isInScale[(targetNote - startingNote) % 12];
                bool isC4 = targetNote == nearestToC4 || nearestToC4 < 0 && targetNote >= 60;

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
                    DefineSpecificButton(row, col, ScalarNote, row%2 == 0 ? Push.Colours.LightGrey : Push.Colours.White);
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
            DefineSpecificButton(7, 0, KsOct0Note, KsFirstNote == 0 ? Push.Colours.Green : Push.Colours.DullYellow);
            DefineSpecificButton(7, 1, KsOct1Note, KsFirstNote == 12 ? Push.Colours.Green : Push.Colours.DullYellow);
            DefineSpecificButton(7, 2, KsOct2Note, KsFirstNote == 24 ? Push.Colours.Green : Push.Colours.DullYellow);
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
                return;
            }

            if (midiEvent.CommandCode == MidiCommandCode.ControlChange)
            {
                var ccEvent = midiEvent as ControlChangeEvent;

                if (ccValues.ContainsKey((byte)ccEvent.Controller))
                {
                    int delta = ccEvent.ControllerValue >= 64 ? ccEvent.ControllerValue - 128 : ccEvent.ControllerValue;
                    int ccValue = ccValues[(byte)ccEvent.Controller] + delta;

                    ccValue = Math.Max(Math.Min(ccValue, 127), 0);

                    ccEvent.ControllerValue = ccValue;
                    ccValues[(byte)ccEvent.Controller] = ccValue;
                }

                if (ccEvent.Controller == MidiController.Modulation)
                {
                    lastModulationValue = ccEvent.ControllerValue;
                    midiLights.Send(midiEvent.GetAsShortMessage());
                }

                switch ((byte)ccEvent.Controller)
                {
                    case Push.Buttons.FootSwitch:
                        if (configValues.PedalMode == ConfigValues.PedalModes.RaiseSemitone)
                        {
                            footSwitchPressed = (ccEvent.ControllerValue >= 64);    // Assuming correct calibration for polarity
                            return;
                        }
                        else break;

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

                    case Push.Buttons.PageLeft:
                        if (ccEvent.ControllerValue > 64)
                        {
                            int keyIndex = configValues.Keys[configValues.Key];
                            if (keyIndex > 0)
                            {
                                configValues.Key = configValues.Keys.Keys.ToArray()[keyIndex - 1];
                                configValues.Save();
                                SetScaleNotesAndLights();
                            }
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
                                SetScaleNotesAndLights();
                            }
                        }
                        return;

                    case Push.Buttons.ScaleLeft:
                        if (ccEvent.ControllerValue > 64)
                        {
                            int scaleIndex = Array.IndexOf(ScaleNames, configValues.Scale);
                            if (scaleIndex > 0)
                            {
                                configValues.Scale = ScaleNames[scaleIndex - 1];
                                configValues.Save();
                                SetScaleNotesAndLights();
                            }
                        }
                        return;

                    case Push.Buttons.ScaleRight:
                        if (ccEvent.ControllerValue > 64)
                        {
                            int scaleIndex = Array.IndexOf(ScaleNames, configValues.Scale);
                            if (scaleIndex < ScaleNames.Count() - 1)
                            {
                                configValues.Scale = ScaleNames[scaleIndex + 1];
                                configValues.Save();
                                SetScaleNotesAndLights();
                            }
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
                            SetScaleNotesAndLights();
                        }
                        return;

                    case Push.Buttons.ToggleTouchStrip:
                        if (ccEvent.ControllerValue > 64)
                        {
                            if (configValues.TouchStripMode == ConfigValues.TouchStripModes.Modulation)
                            {
                                configValues.TouchStripMode = ConfigValues.TouchStripModes.PitchBend;
                            }
                            else
                            {
                                configValues.TouchStripMode = ConfigValues.TouchStripModes.Modulation;
                            }
                            SetTouchStripMode(configValues.TouchStripMode == ConfigValues.TouchStripModes.PitchBend);
                        }
                        return;

                    case Push.Buttons.BrightnessCC:
                        SetLedBrightness(ccEvent.ControllerValue);
                        return;
                }

                if (Push.Buttons.Layouts.Contains((byte)ccEvent.Controller) && ccEvent.ControllerValue > 64)
                {
                    int index = Array.IndexOf(Push.Buttons.Layouts, (byte)ccEvent.Controller);
                    if (index < ConfigValues.Layouts.Choices.Length)
                    {
                        configValues.Layout = ConfigValues.Layouts.Choices[index];
                        configValues.Save();
                        SetScaleNotesAndLights();
                    }
                    return;
                }
            }

            if (midiEvent.CommandCode == MidiCommandCode.NoteOn || midiEvent.CommandCode == MidiCommandCode.NoteOff)
            {
                var noteOnEvent = midiEvent as NoteEvent;
                var padNoteNumber = noteOnEvent.NoteNumber;

                if (padNoteNumber < Push.FirstPad || padNoteNumber > Push.LastPad)
                {
                    return;
                }

                var noteEncoded = scaleNoteMapping[padNoteNumber - Push.FirstPad];

                if (noteEncoded == ScalarNote)
                {
                    var row = (padNoteNumber - Push.FirstPad) / 8;
                    var col = (padNoteNumber - Push.FirstPad) % 8;
                    var n1 = new NoteEvent(0, noteOnEvent.Channel, midiEvent.CommandCode, ScalerKsNotes[row], noteOnEvent.Velocity);
                    var n2 = new NoteEvent(0, noteOnEvent.Channel, midiEvent.CommandCode, ScalerPadNotes[col], noteOnEvent.Velocity);
                    midiOut.Send(n1.GetAsShortMessage());
                    midiOut.Send(n2.GetAsShortMessage());
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

                    if (configValues.PedalMode == ConfigValues.PedalModes.RaiseSemitone && footSwitchPressed)
                    {
                        noteEncoded += 1;
                    }

                    noteOnEvent.NoteNumber = noteEncoded;
                    notesOn[padNoteNumber] = noteOnEvent.NoteNumber;
                }
                else if (notesOn.ContainsKey(padNoteNumber))
                {
                    noteOnEvent.NoteNumber = notesOn[padNoteNumber];
                    notesOn.Remove(padNoteNumber);
                    if (notesOn.ContainsValue(noteOnEvent.NoteNumber)) return;
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
            }

#if DEBUG
            System.Diagnostics.Trace.WriteLine(String.Format("{0}  {1,-10} {2}",
                FormatTimeStamp(e.Timestamp), FormatMidiBytes(e.RawMessage), midiEvent));
#endif

            midiOut.Send(midiEvent.GetAsShortMessage());
        }

        private static void SetLedBrightness(int brightness)
        {
            //  Use the rh controller to set the LED brightness
            SendSysex(new byte[] { 0x06, (byte)brightness });
            SendSysex(new byte[] { 0x08, (byte)((brightness % 64) * 2), (byte)(brightness >= 64 ? 1 : 0) });
        }

        private static void SetPressureMode(bool isPoly)
        {
            SendSysex(new byte[] { 0x1E, isPoly ? (byte)1 : (byte)0 });
        }

        private static void SetTouchStripMode(bool isPitchBend)
        {
            SendSysex(new byte[] { 0x17, isPitchBend ? (byte)0x68 : (byte)0x05 });
        }

        private static void SetPedalMode(bool isContinuous)
        {
            //  The voltages in the Sysex 0x31 messages belows (***)  were measured externally via Sysex exchanges within Midi Ox.
            //  They cannot be calibrated in this software as NAudio cannot receive Sysex
            //  The numbers used are for my own two pedals - once continuous and one discrete - which I swap around as needed.
            if (isContinuous)
            {
                SendSysex(new byte[] { 0x30, 0x03, 0x7F, 0x00, 0x00 });
                SendSysex(new byte[] { 0x30, 0x02, 0x04, 0x00, 0x00 });
                SendSysex(new byte[] { 0x31, 0x02, 0x4C, 0x01, 0x79, 0x17 });   // ***
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
                SendSysex(new byte[] { 0x30, 0x02, 0x7F, 0x00, 0x00 });
                SendSysex(new byte[] { 0x30, 0x03, 0x45, 0x00, 0x00 });
                SendSysex(new byte[] { 0x31, 0x03, 0x16, 0x02, 0x71, 0x1D });   // ***
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
