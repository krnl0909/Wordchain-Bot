using Discord;
using Discord.Net;
using Discord.WebSocket;

namespace WordchainBot
{
	static internal class Commands
	{
		static public async Task CreateSlashCommands()
		{
			var comms = new SlashCommandBuilder[]
			{
			new SlashCommandBuilder()
				.WithName("join")
				.WithDescription("게임에 참가합니다. 게임 도중에는 참여할 수 없습니다."),

			new SlashCommandBuilder()
				.WithName("leave")
				.WithDescription("게임을 떠납니다."),

			new SlashCommandBuilder()
				.WithName("start")
				.WithDescription("게임을 시작합니다."),

			new SlashCommandBuilder()
				.WithName("end")
				.WithDescription("게임을 끝냅니다."),

			new SlashCommandBuilder()
				.WithName("chain")
				.WithDescription("단어를 연결합니다. 표준국어대사전 API를 사용합니다.")
				.AddOption
				(
				new SlashCommandOptionBuilder()
					.WithName("word")
					.WithDescription("단어")
					.WithRequired(true)
					.WithType(ApplicationCommandOptionType.String)
				)
			};

			await AddCommands(comms);
		}

		static public async Task CreateSlashCommands(SocketGuild arg)
		{
			await CreateSlashCommands();
		}

		static private async Task AddCommands(SlashCommandBuilder[] args)
		{
			try
			{
				foreach (var command in args)
				{
					await Program.client.Rest.CreateGlobalCommand(command.Build());
				}
			}
			catch (HttpException e)
			{
				Console.WriteLine(e.Message);
			}
		}

		static public async Task HandleSlashCommands(SocketSlashCommand arg)
		{
			switch (arg.Data.Name)
			{
				case "join":
					Program.game ??= new Game();
					if (Program.game.AddPlayer(arg.User))
						await arg.RespondAsync($"{arg.User.Mention}이/가 게임에 참가했습니다.");
					else await arg.RespondAsync("게임에 참가하는 데 실패했습니다.", ephemeral: true);
					break;

				case "leave":
					if (Program.game != null && Program.game.RemovePlayer(arg.User))
						await arg.RespondAsync($"{arg.User.Mention}이/가 게임을 떠났습니다.");
					else await arg.RespondAsync("게임을 떠나는 데 실패했습니다.", ephemeral: true);
					break;

				case "start":
					if (Program.game != null)
					{
						if (!Program.game.started)
						{
							await arg.RespondAsync($"게임 시작! {Program.game.CurrentPlayer().Mention}의 차례입니다.");
							Program.game.StartGame();
						}
						else
						{
							await arg.RespondAsync("게임이 이미 시작되었습니다.", ephemeral: true);
						}
					}
					else await arg.RespondAsync("게임을 시작하는 데 실패했습니다.", ephemeral: true);
					break;

				case "end":
					if (Program.game != null)
					{
						if(Program.game.started)
						{
							await arg.RespondAsync("게임을 끝냈습니다.");
							Program.game.EndGame();
						}
						else
						{
							await arg.RespondAsync("진행중인 게임이 없습니다.");
						}
					}
					else await arg.RespondAsync("게임을 끝내는 데 실패했습니다.", ephemeral: true);
					break;

				case "chain":
					if (Program.game != null)
					{
						string word = arg.Data.Options.ToArray()[0].Value.ToString() ?? "";

						await arg.RespondAsync("입력한 단어: " + word, ephemeral: true);

						WordState ret = await Program.game.Chain(word);

						switch (ret)
						{
							case WordState.Existent:
								await arg.FollowupAsync($"\"{word}\"! {Program.game.CurrentPlayer().Mention}의 차례입니다.");
								break;

							case WordState.Failed_request:
								await arg.FollowupAsync("표준국어대사전 API와의 연결이 원활하지 않습니다.");
								break;

							case WordState.Repitition:
							case WordState.Non_existent:
							case WordState.One_letter_long:
							case WordState.No_match_with_previous:
								await arg.FollowupAsync("어허!!");
								break;

							default:
								await arg.FollowupAsync("???");
								break;
						}
					}
					break;
				default:
					await arg.RespondAsync();
					break;
			}
		}
	}
}
