﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pkmn_ntr.Sub_forms.Scripting
{
    public partial class ScriptBuilder : Form
    {
        // Main class handling
        private List<ScriptAction> actions;
        private bool scriptRunning = false;
        private bool stopScript = false;

        public ScriptBuilder()
        {
            InitializeComponent();
        }

        private void ScriptBuilder_Load(object sender, EventArgs e)
        {
            actions = new List<ScriptAction>();
            numTime.Maximum = Int32.MaxValue;
            string buttonMsg = "Click for a simple action.\r\nShift + Click for a timed action using the Time value.\r\nControl + Click for hold the action until another or a release command.";
            Control[] tooltTpButtons = new Control[] { btnA, btnB, btnX, btnY, btnL, btnR, btnStart, btnSelect, btnUp, btnDown, btnLeft, btnRight, btnStick, btnTouch, btnTouch };
            foreach (Control ctrl in tooltTpButtons)
            {
                toolTip1.SetToolTip(ctrl, buttonMsg);
            }
            if (Program.gCmdWindow.isConnected)
            {
                btnStartStop.Text = "Start Script";
                btnStartStop.Enabled = true;
            }
        }

        delegate void MethodDelegate();

        // Action list handling
        private void UpdateActionList()
        {
            if (this.InvokeRequired)
            {
                BeginInvoke(new MethodDelegate(UpdateActionList));
                return;
            }
            lstActions.Items.Clear();
            foreach (ScriptAction act in actions)
            {
                lstActions.Items.Add(act.Tag);
            }
        }

        private void AddActionToList(ScriptAction act)
        {
            int index = lstActions.SelectedIndex;
            if (index > 0)
            {
                actions.Insert(index + 1, act);
                UpdateActionList();
                lstActions.SelectedIndex = index + 1;
            }
            else
            {
                actions.Add(act);
                UpdateActionList();
            }
        }

        private void BtnActionUp_Click(object sender, EventArgs e)
        {
            if (lstActions.SelectedIndex > 0) // Checks if an item is selected, but also that it isn't the first of the list
            {
                int index = lstActions.SelectedIndex;
                ScriptAction temp = actions[index];
                actions.RemoveAt(index);
                actions.Insert(index - 1, temp);
                UpdateActionList();
                lstActions.SelectedIndex = index - 1;
            }
            else if (lstActions.SelectedIndex == 0)
            {
                MessageBox.Show("The first element of the list cannot be moved up", "Script Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("There is no command selected on the list.", "Script Builder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void BtnActionDown_Click(object sender, EventArgs e)
        {
            if (lstActions.SelectedIndex >= 0 && lstActions.SelectedIndex < (lstActions.Items.Count - 1)) // Checks if an item is selected, but also that it isn't the last of the list
            {
                int index = lstActions.SelectedIndex;
                ScriptAction temp = actions[index];
                actions.RemoveAt(index);
                actions.Insert(index + 1, temp);
                UpdateActionList();
                lstActions.SelectedIndex = index + 1;
            }
            else if (lstActions.SelectedIndex == lstActions.Items.Count - 1)
            {
                MessageBox.Show("The last element of the list cannot be moved down", "Script Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("There is no command selected on the list.", "Script Builder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void RemoveAction(object sender, EventArgs e)
        {
            if (lstActions.SelectedIndex > 0)
            {
                actions.RemoveAt(lstActions.SelectedIndex);
                UpdateActionList();
            }
            else
            {
                MessageBox.Show("There is no command selected on the list.", "Script Builder", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ClearAllActions(object sender, EventArgs e)
        {
            actions.Clear();
            UpdateActionList();
        }

        private void StartStopScript(object sender, EventArgs e)
        {
            if (scriptRunning)
            {
                stopScript = true;
                btnStartStop.Text = "Start Script";
                btnStartStop.Enabled = false;
            }
            else
            {
                stopScript = false;
                btnStartStop.Text = "Stop Script";
                btnStartStop.Enabled = true;
                ExecuteScript();
            }
        }

        private async void ExecuteScript()
        {
            if (this.InvokeRequired)
            {
                BeginInvoke(new MethodDelegate(ExecuteScript));
                return;
            }
            if (actions.Count < 1)
            {
                MessageBox.Show("No actions to execute.", "Script Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Program.gCmdWindow.ScriptMode(true);
            scriptRunning = true;
            ScriptAction.Report("Script: Start script");
            for (int index = 0; index < actions.Count; index++)
            {
                lstActions.SelectedIndex = index;
                if (stopScript || !Program.gCmdWindow.isConnected)
                {
                    break;
                }
                await actions[index].Excecute();
            }
            ScriptAction.Report("Script: Stop script");
            scriptRunning = false;
            Program.gCmdWindow.ScriptMode(false);
            MessageBox.Show("Script finished.", "Script Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (Program.gCmdWindow.isConnected)
            {
                btnStartStop.Text = "Start Script";
                btnStartStop.Enabled = true;
            }
            else
            {
                btnStartStop.Text = "Not connected";
                btnStartStop.Enabled = false;
            }
        }

        // Delays
        private void ClickDelay(object sender, EventArgs e)
        {
            AddActionToList(new DelayAction((int)numTime.Value));
        }

        // Buttons
        private void ClickConsoleButton(object sender, EventArgs e)
        {
            var input = ButtonAction.ConsoleButton.None;
            if (sender == btnA) input = ButtonAction.ConsoleButton.A;
            if (sender == btnB) input = ButtonAction.ConsoleButton.B;
            if (sender == btnX) input = ButtonAction.ConsoleButton.X;
            if (sender == btnY) input = ButtonAction.ConsoleButton.Y;
            if (sender == btnL) input = ButtonAction.ConsoleButton.L;
            if (sender == btnR) input = ButtonAction.ConsoleButton.R;
            if (sender == btnStart) input = ButtonAction.ConsoleButton.Start;
            if (sender == btnSelect) input = ButtonAction.ConsoleButton.Select;
            if (sender == btnUp) input = ButtonAction.ConsoleButton.Up;
            if (sender == btnDown) input = ButtonAction.ConsoleButton.Down;
            if (sender == btnLeft) input = ButtonAction.ConsoleButton.Left;
            if (sender == btnRight) input = ButtonAction.ConsoleButton.Right;
            if (ModifierKeys == Keys.Shift)
            {
                AddActionToList(new ButtonAction(input, (int)numTime.Value));
            }
            else if (ModifierKeys == Keys.Control)
            {
                AddActionToList(new ButtonAction(input, -1));
            }
            else
            {
                AddActionToList(new ButtonAction(input, 0));
            }
        }

        private void ClickReleaseButtons(object sender, EventArgs e)
        {
            AddActionToList(new ButtonAction(ButtonAction.ConsoleButton.None, 0));
        }

        // Touch Screen
        private void ClickTouchButton(object sender, EventArgs e)
        {
            if (ModifierKeys == Keys.Shift)
            {
                AddActionToList(new TouchAction((int)numTouchX.Value, (int)numTouchY.Value, (int)numTime.Value));
            }
            else if (ModifierKeys == Keys.Control)
            {
                AddActionToList(new TouchAction((int)numTouchX.Value, (int)numTouchY.Value, -1));
            }
            else
            {
                AddActionToList(new TouchAction((int)numTouchX.Value, (int)numTouchY.Value, 0));
            }
        }

        private void ClickReleaseTouch(object sender, EventArgs e)
        {
            AddActionToList(new TouchAction(-1, -1, 0));
        }
    }
}