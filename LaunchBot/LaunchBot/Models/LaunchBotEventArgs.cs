using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaunchBot.Models
{
    public class LaunchBotEventArgs: EventArgs
    {
        public string Token { get; set; }
    }
}
