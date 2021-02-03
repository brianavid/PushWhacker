using System;
using System.Drawing;
using System.Windows.Forms;

namespace PushWhacker
{
    public class CustomApplicationContext : ApplicationContext
    {
        private static readonly string DefaultTooltip = "PushWhacker";

        private static ConfigValues configValues;
        private static MidiProcessor midiProcessor;

        /// <summary>
        /// This class should be created and passed into Application.Run( ... )
        /// </summary>
        public CustomApplicationContext(ConfigValues values, MidiProcessor processor)
        {
            configValues = values;
            midiProcessor = processor;
            InitializeContext();
        }

        # region generic code framework

        private System.ComponentModel.IContainer components;	// a list of components to dispose when the context is disposed
        private NotifyIcon notifyIcon;				            // the icon that sits in the system tray

        private void InitializeContext()
        {
            components = new System.ComponentModel.Container();
            notifyIcon = new NotifyIcon(components)
            {
                ContextMenuStrip = new ContextMenuStrip(),
                Icon = new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("PushWhacker.Resources.PushWhacker.ico")),
                Text = DefaultTooltip,
                Visible = true
            };

            MenuItem exitContextMenuItem = new System.Windows.Forms.MenuItem();
            exitContextMenuItem.Index = 2;
            exitContextMenuItem.Text = "&Exit";
            exitContextMenuItem.Click += new System.EventHandler(this.exitItem_Click);

            MenuItem configContextMenuItem = new System.Windows.Forms.MenuItem();
            configContextMenuItem.Index = 1;
            configContextMenuItem.Text = "&Config";
            configContextMenuItem.Click += new System.EventHandler(this.configItem_Click);

            ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
            contextMenu.MenuItems.AddRange(new MenuItem[] { configContextMenuItem, exitContextMenuItem });

            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.DoubleClick += new System.EventHandler(this.configItem_Click);


        }

        /// <summary>
		/// When the application context is disposed, dispose things like the notify icon.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) { components.Dispose(); }
        }

        /// <summary>
        /// When the config menu item is clicked, ...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void configItem_Click(object sender, EventArgs e)
        {
            var config = new Config(configValues, midiProcessor);
            config.ShowDialog();
        }

        /// <summary>
        /// When the exit menu item is clicked, make a call to terminate the ApplicationContext.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitItem_Click(object sender, EventArgs e)
        {
            ExitThread();
        }

        /// <summary>
        /// If we are presently showing a form, clean it up.
        /// </summary>
        protected override void ExitThreadCore()
        {
            notifyIcon.Visible = false; // should remove lingering tray icon
            base.ExitThreadCore();
        }

        # endregion generic code framework

    }
}
