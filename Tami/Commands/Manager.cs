using Discord;
using Discord.Commands;
using DiscordUtils;
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

            var me = Context.Guild.GetUser(Program.P.Client.CurrentUser.Id);
            Command[] commands;
            try
            {
                commands = await ParseAsync(me.GuildPermissions, msg, Context.Guild, me);
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync(e.Message);
                return;
            }
            if (commands == null || commands.Length == 0) // Invalid command
                return;

            var res = await Context.Channel.SendMessageAsync(embed: new EmbedBuilder
            {
                Color = Color.Blue,
                Title = "Vote in progress...",
                Description = "Command: " + msg,
                Footer = new EmbedFooterBuilder
                {
                    Text = "Vote will end in one minute"
                }
            }.Build());
            var yes = new Emoji("✅");
            var no = new Emoji("❌");
            await res.AddReactionsAsync(new[] { yes, no });

            await Task.Delay(6000); // 60000 ms = 1 min

            await res.UpdateAsync();
            var countYes = res.Reactions.Where(x => x.Key.Name == yes.Name).First().Value.ReactionCount - 1;
            var countNo = res.Reactions.Where(x => x.Key.Name == no.Name).First().Value.ReactionCount - 1;

            if (countNo >= countYes)
            {
                await res.ModifyAsync(x => x.Embed = new EmbedBuilder
                {
                    Color = Color.Red,
                    Title = "Vote failed",
                    Description = "Command: " + msg,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = countYes + " - " + countNo
                    }
                }.Build());
                return;
            }

            await res.ModifyAsync(x => x.Embed = new EmbedBuilder
            {
                Color = Color.Green,
                Title = "Vote succeed",
                Description = "Command: " + msg,
                Footer = new EmbedFooterBuilder
                {
                    Text = countYes + " - " + countNo
                }
            }.Build());
            try
            {
                foreach (var cmd in commands)
                {
                    switch (cmd.order)
                    {
                        case Order.SAY:
                            await Context.Channel.SendMessageAsync((string)cmd.arg);
                            break;

                        case Order.CREATE:
                            await Context.Guild.CreateTextChannelAsync((string)cmd.arg);
                            break;

                        case Order.DESTROY:
                            await ((ITextChannel)cmd.arg).DeleteAsync();
                            break;

                        case Order.KICK:
                            await ((IGuildUser)cmd.arg).KickAsync();
                            break;

                        default:
                            throw new ArgumentException("Invalid order " + cmd.order.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync(e.Message);
            }
        }

        public static async Task<Command[]> ParseAsync(GuildPermissions perms, string message, IGuild guild, IGuildUser me)
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
                        string args = string.Join(" ", tmpSplit.Skip(1));
                        object argsObj;
                        switch (order)
                        {
                            case Order.SAY:
                                if (args.Length == 0) throw new ParsingError("SAY must be given the sentence to say");
                                argsObj = args;
                                break;

                            case Order.CREATE:
                                if (!perms.ManageChannels) throw new ParsingError("CREATE need me to have the Manage Channels permission");
                                if (args.Length == 0) throw new ParsingError("CREATE must be given the channel name to create");
                                argsObj = args;
                                break;

                            case Order.DESTROY:
                                if (!perms.ManageChannels) throw new ParsingError("DESTROY need me to have the Manage Channels permission");
                                if (args.Length == 0) throw new ParsingError("DESTROY must be given the channel name to destroy");
                                argsObj = await Utils.GetTextChannel(args, guild);
                                if (argsObj == null) throw new ParsingError("DESTROY must be given a valid text channel in parameter");
                                break;

                            case Order.KICK:
                                if (!perms.KickMembers) throw new ParsingError("KICK need me to have the Kick Members permission");
                                if (args.Length == 0) throw new ParsingError("KICK must be given the user name to kick");
                                var otherUser = await Utils.GetUser(args, guild);
                                if (otherUser == null) throw new ParsingError("KICK must be given a valid user in parameter");
                                var roles = guild.Roles.Select(x => x.Id).ToList();
                                roles.Remove(guild.Id);
                                if (me.RoleIds.Min(x => roles.ToList().IndexOf(x)) > otherUser.RoleIds.Min(x => roles.ToList().IndexOf(x)))
                                    throw new ParsingError("KICK can't be called on an user with higher permissions than mine");
                                argsObj = otherUser;
                                break;

                            default:
                                throw new ArgumentException("Invalid order " + order.ToString());
                        }
                        commands.Add(new Command
                        {
                            order = order,
                            arg = argsObj
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
