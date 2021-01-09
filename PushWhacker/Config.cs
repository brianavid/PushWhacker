using NAudio.Midi;
using System;

using System.Windows.Forms;

namespace PushWhacker
{
    public partial class Config : Form
    {
        private static ConfigValues configValues;
        private static MidiProcessor midiProcessor;

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

            foreach (var scale in MidiProcessor.Scales.Keys)
            {
                comboBoxScale.Items.Add(scale);
            }

            foreach (var key in configValues.Keys.Keys)
            {
                comboBoxKey.Items.Add(key);
            }

            comboBoxOutput.SelectedIndex = 0;
            comboBoxLayout.SelectedIndex = 0;
            comboBoxChannel.SelectedIndex = 0;
            comboBoxScale.SelectedIndex = 0;
            comboBoxKey.SelectedIndex = 0;

            if (!String.IsNullOrEmpty(values.Output)) comboBoxOutput.SelectedItem = values.Output;
            if (!String.IsNullOrEmpty(values.Layout)) comboBoxLayout.SelectedItem = values.Layout;
            if (!String.IsNullOrEmpty(values.Channel)) comboBoxChannel.SelectedItem = values.Channel;
            if (!String.IsNullOrEmpty(values.Scale)) comboBoxScale.SelectedItem = values.Scale;
            if (!String.IsNullOrEmpty(values.Key)) comboBoxKey.SelectedItem = values.Key;
            comboBoxOctave.SelectedItem = !String.IsNullOrEmpty(values.Octave) ? values.Octave : "3";
            debugCheckBox.Checked = configValues.Debug;
            checkBoxSemitonePedal.Checked = configValues.SemitonePedal;
        }

        private void StoreValues()
        {
            configValues.Output = comboBoxOutput.SelectedItem as string;
            configValues.Layout = comboBoxLayout.SelectedItem as string;
            configValues.Channel = comboBoxChannel.SelectedItem as string;
            configValues.Scale = comboBoxScale.SelectedItem as string;
            configValues.Key = comboBoxKey.SelectedItem as string;
            configValues.Octave = comboBoxOctave.SelectedItem as string;
            configValues.Debug = debugCheckBox.Checked;
            configValues.SemitonePedal = checkBoxSemitonePedal.Checked;

            configValues.Save();

            midiProcessor.StopProcessing();
            midiProcessor.StartProcessing();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            StoreValues();

            this.DialogResult = DialogResult.OK;
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            StoreValues();
        }
    }
}
