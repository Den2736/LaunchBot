using LaunchBot.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;

namespace LaunchBot
{
    public partial class MainForm : Form
    {
        public event EventHandler<LaunchBotEventArgs> OnLaunchButtonClick;
        public event EventHandler OnTerminateButtonClick;

        public MainForm()
        {
            InitializeComponent();
        }

        public void SetLaunchButton(bool setTo)
        {
            LaunchButton.Enabled = setTo;
        }
        public void SetTerminateButton(bool setTo)
        {
            TerminateButton.Enabled = setTo;
        }

        public void SetNeutralStatus(string message)
        {
            StatusLabel.ForeColor = Color.Black;
            StatusLabel.Text = message;
        }
        public void SetSuccessStatus(string message)
        {
            StatusLabel.ForeColor = Color.Green;
            StatusLabel.Text = message;
        }
        public void SetDangerStatus(string message)
        {
            StatusLabel.ForeColor = Color.Red;
            StatusLabel.Text = message;
        }

        public void SetBotName(string botName)
        {
            if (!this.Text.Contains(botName))
            {
                this.Text = botName;
            }
        }

        public void ShowError(string errorMessage)
        {
            SetLaunchButton(false);
            SetTerminateButton(false);
            SetDangerStatus(errorMessage);
        }
        public void ShowError(Exception e)
        {
            SetLaunchButton(false);
            SetTerminateButton(false);
            SetDangerStatus(e.ToString());
        }

        private void LaunchButton_Click(object sender, EventArgs e)
        {
            OnLaunchButtonClick?.Invoke(this, new LaunchBotEventArgs() {Token = TokenInput.Text});
        }
        private void TerminateButton_Click(object sender, EventArgs e)
        {
            OnTerminateButtonClick?.Invoke(this, null);
        }

        private void TokenInput_Enter(object sender, EventArgs e)
        {
            TokenInput.Clear();
        }
    }
}
