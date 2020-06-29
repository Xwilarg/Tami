using Discord.Commands;
using DiscordUtils;
using System.Threading.Tasks;

namespace Tami.Modules
{
    public class Communication : ModuleBase
    {
        [Command("Info")]
        public async Task Info()
        {
            await ReplyAsync(embed: Utils.GetBotInfo(Program.P.StartTime, "Tami", Program.P.Client.CurrentUser));
        }
    }
}
