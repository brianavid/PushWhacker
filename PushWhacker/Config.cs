using NAudio.Midi;
using System;

using System.Windows.Forms;

namespace PushWhacker
{
    public partial class Config : Form
    {
        private static ConfigValues configValues;
        private static MidiProcessor midiProcessor;
        int previousLayoutStoreSelectedIndex = 0;

        public Config(ConfigValues values, MidiProcessor processor)
        {
            InitializeComponent();
            configValues = values;
            midiProcessor = processor;

            for (int device = 0; device < MidiOut.NumberOfDevices; device++)
            {
                comboBoxOutput.Items.Add(MidiOut.DeviceInfo(device).ProductName);
            }

            foreach (var layout in ConfigValues.Layouts.Choices)
            {
                comboBoxLayout.Items.Add(layout);
            }

            foreach (var pressure in ConfigValues.Pressures.Choices)
            {
                comboBoxPressure.Items.Add(pressure);
            }

            foreach (var touchStripMode in ConfigValues.TouchStripModes.Choices)
            {
                comboBoxTouchStrip.Items.Add(touchStripMode);
            }

            foreach (var pedalMode in ConfigValues.PedalModes.Choices)
            {
                comboBoxPedal.Items.Add(pedalMode);
            }

            foreach (var scale in MidiProcessor.Scales.Keys)
            {
                comboBoxScale.Items.Add(scale);
                comboBoxSwitchedScale.Items.Add(scale);
            }

            foreach (var key in configValues.Keys.Keys)
            {
                comboBoxKey.Items.Add(key);
            }

            comboBoxLayoutStore.Items.Add("Current");
            foreach (var store in Push.StoreageButtonLabels.Values)
            {
                comboBoxLayoutStore.Items.Add("Store " + store);
            }

            comboBoxOutput.SelectedIndex = 0;
            comboBoxLayout.SelectedIndex = 0;
            comboBoxLayoutStore.SelectedIndex = 0;
            comboBoxScale.SelectedIndex = 0;
            comboBoxSwitchedScale.SelectedIndex = 0;
            comboBoxKey.SelectedIndex = 0;
            comboBoxPressure.SelectedIndex = 0;
            comboBoxTouchStrip.SelectedIndex = 0;
            comboBoxPedal.SelectedIndex = 0;

            if (!String.IsNullOrEmpty(values.Output)) comboBoxOutput.SelectedItem = values.Output;
            if (!String.IsNullOrEmpty(values.Layout)) comboBoxLayout.SelectedItem = values.Layout;
            comboBoxPressure.SelectedItem = values.Pressure;
            comboBoxTouchStrip.SelectedItem = values.TouchStripMode;
            comboBoxPedal.SelectedItem = values.PedalMode;
            checkBoxUserModeOnly.Checked = configValues.UserModeOnly;
            comboBoxPadStartNote.SelectedIndex = configValues.FixLayout ? 1 : 0;
            comboBoxKeyChangeAmount.SelectedIndex = configValues.KeyChangeFifths ? 1 : 0;
            
            LoadScaleValues();
        }

        private void StoreValues()
        {
            configValues.UserModeOnly = checkBoxUserModeOnly.Checked;
            configValues.Output = comboBoxOutput.SelectedItem as string;
            configValues.Layout = comboBoxLayout.SelectedItem as string;
            configValues.FixLayout = comboBoxPadStartNote.SelectedIndex != 0;
            configValues.KeyChangeFifths = comboBoxKeyChangeAmount.SelectedIndex != 0;
            configValues.Pressure = comboBoxPressure.SelectedItem as string;
            configValues.TouchStripMode = comboBoxTouchStrip.SelectedItem as string;
            configValues.PedalMode = comboBoxPedal.SelectedItem as string;
            
            SaveScaleValues();

            configValues.Save();

            midiProcessor.StopProcessing();
            if (!midiProcessor.StartProcessing())
            {
                MessageBox.Show("Can't start Push Midi Processing\n\nThis is usually because another program or browser web page is using Midi and has locked all ports");
            }
        }

        private void LoadScaleValues()
        {
            if (!String.IsNullOrEmpty(configValues.Scale)) comboBoxScale.SelectedItem = configValues.Scale;
            if (!String.IsNullOrEmpty(configValues.SwitchedScale))
                comboBoxSwitchedScale.SelectedItem = configValues.SwitchedScale;
            else
                comboBoxSwitchedScale.SelectedItem = comboBoxScale.SelectedItem;
            if (!String.IsNullOrEmpty(configValues.Key)) comboBoxKey.SelectedItem = configValues.Key;
            comboBoxOctave.SelectedItem = !String.IsNullOrEmpty(configValues.Octave) ? configValues.Octave : "3";
        }

        private void SaveScaleValues()
        {
            configValues.Scale = comboBoxScale.SelectedItem as string;
            configValues.SwitchedScale = comboBoxSwitchedScale.SelectedItem as string;
            configValues.Key = comboBoxKey.SelectedItem as string;
            configValues.Octave = comboBoxOctave.SelectedItem as string;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            StoreValues();
            if (comboBoxLayoutStore.SelectedIndex != 0)
            {
                configValues.SaveStore(" ABCD".Substring(comboBoxLayoutStore.SelectedIndex, 1));
            }

            this.DialogResult = DialogResult.OK;
        }

        private void buttonColLo_Click(object sender, EventArgs e)
        {
            MidiProcessor.DisplayColours(0);
        }

        private void buttonColHi_Click(object sender, EventArgs e)
        {
            MidiProcessor.DisplayColours(1);
        }

        private void buttonCalibrateUp_Click(object sender, EventArgs e)
        {
            MidiProcessor.CalibrateFootPedal(ConfigValues.PedalCalibrationId.SwitchOff);
        }

        private void buttonCalibrateDown_Click(object sender, EventArgs e)
        {
            MidiProcessor.CalibrateFootPedal(ConfigValues.PedalCalibrationId.SwitchOn);
        }

        private void buttonCalibrateHeel_Click(object sender, EventArgs e)
        {
            MidiProcessor.CalibrateFootPedal(ConfigValues.PedalCalibrationId.ControlHeel);
        }

        private void buttonCalibrateToe_Click(object sender, EventArgs e)
        {
            MidiProcessor.CalibrateFootPedal(ConfigValues.PedalCalibrationId.ControlToe);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBoxLayoutStore_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (previousLayoutStoreSelectedIndex != 0)
            {
                SaveScaleValues();
                configValues.SaveStore(" ABCD".Substring(previousLayoutStoreSelectedIndex, 1));
            }
            if (comboBoxLayoutStore.SelectedIndex != 0)
            {
                configValues.LoadStore(" ABCD".Substring(comboBoxLayoutStore.SelectedIndex, 1));
                LoadScaleValues();
            }
            previousLayoutStoreSelectedIndex = comboBoxLayoutStore.SelectedIndex;
        }
    }
}
