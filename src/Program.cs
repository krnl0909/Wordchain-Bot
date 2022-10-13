using Discord;
using Discord.WebSocket;

namespace WordchainBot
{
	static public class Program
	{
		static public DiscordSocketClient client = new();

		static public Game? game;
		static private string? key = Environment.GetEnvironmentVariable("WordchainBot_Key");

		static async Task Main() => await MainAsync();

		static private async Task MainAsync()
		{
			try
			{
				await client.LoginAsync(TokenType.Bot, key);
				await client.StartAsync();

				client.Ready += Commands.CreateSlashCommands;
				client.JoinedGuild += Commands.CreateSlashCommands;
				client.SlashCommandExecuted += Commands.HandleSlashCommands;

				await Task.Delay(Timeout.Infinite);
			}
			finally
			{
				await client.StopAsync();
				await client.LogoutAsync();
			}
		}
	}
}