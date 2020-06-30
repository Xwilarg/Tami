using Discord;
using Discord.Commands;
using DiscordUtils;
using System.Threading.Tasks;

namespace Tami.Modules
{
    public class Communication : ModuleBase
    {
        [Command("Info")]
        public async Task Info(params string[] _)
        {
            await ReplyAsync(embed: Utils.GetBotInfo(Program.P.StartTime, "Tami", Program.P.Client.CurrentUser));
        }

        [Command("Status")]
        public async Task Status(params string[] _)
        {
            IGuildUser me = await Context.Guild.GetUserAsync(Program.P.Client.CurrentUser.Id);
            await ReplyAsync(embed: new EmbedBuilder
            {
                Color = Color.Purple,
                Description = "Kick: " + (me.GuildPermissions.KickMembers ? "Yes" : "No") + "\n" +
                    "Create channels: " + (me.GuildPermissions.KickMembers ? "Yes" : "No")
            }.Build());
        }
    }
}
