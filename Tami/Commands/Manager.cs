using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tami.Commands
{
    public static class Manager
    {
        public static async Task LaunchCommandAsync(SocketCommandContext Context, int pos)
        {
            string msg = Context.Message.Content.Substring(pos);
            if (msg.Length == 0)
                return;

            var commands = Parse(msg);
            if (commands == null || commands.Length == 0) // Invalid command
                return;

            foreach (var cmd in commands)
            {
                switch (cmd.order)
                {
                    case Order.SAY:
                        await Context.Channel.SendMessageAsync(cmd.arg);
                        break;

                    default:
                        throw new ArgumentException("Invalid order " + cmd.order.ToString());
                }
            }
        }

        public static Command[] Parse(string message)
        {
            List<Command> commands = new List<Command>();
            foreach (string s in message.Split("&&"))
            {
                string[] tmpSplit = s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
                if (tmpSplit.Length == 0)
                    return null;
                foreach (var order in Enum.GetValues(typeof(Order)).Cast<Order>())
                {
                    if (order.ToString() == tmpSplit[0].ToUpper())
                    {
                        commands.Add(new Command
                        {
                            order = order,
                            arg = tmpSplit.Length > 0 ? string.Join(" ", tmpSplit.Skip(1)) : null
                        });
                        goto success;
                    }
                }
                return null;
                success:;
            }
            return commands.ToArray();
        }
    }
}
