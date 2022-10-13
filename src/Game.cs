using Discord.WebSocket;

namespace WordchainBot
{
	public class Game
	{
		private List<SocketUser> players;
		private List<string> history;

		private int index;

		public bool started;

		public Game()
		{
			players = new List<SocketUser>();
			history = new List<string>();
			index = 0;

			started = false;
		}

		public bool AddPlayer(SocketUser player)
		{
			if (!players.Contains(player))
			{
				players.Add(player);
				return true;
			}
			else return false;
		}

		public bool RemovePlayer(SocketUser player)
		{
			if (players.Contains(player))
			{
				players.Remove(player);
				if (players.Count == 0)
				{
					Console.WriteLine("모든 플레이어들이 게임을 떠났습니다.");
					EndGame();
				}
				return true;
			}
			else return false;
		}

		public void StartGame()
		{
			started = true;
		}

		public void EndGame()
		{
			players = new List<SocketUser>();
			history = new List<string>();
			index = 0;

			started = false;
		}

		public SocketUser CurrentPlayer()
		{
			return players[index];
		}

		public async Task<WordState> Chain(string word)
		{
			if (word.Length >= 2)
			{
				if (!history.Contains(word))
				{
					WordState ret = await Dict.WordExist(word);

					if (ret == WordState.Existent)
					{
						if (history.Count == 0 || (history.Last().Last() == word.First()))
						{
							history.Add(word);
							NextPlayer();
						}
						else ret = WordState.No_match_with_previous;
					}
					return ret;
				}
				else return WordState.Repitition;
			}
			else return WordState.One_letter_long;
		}

		private void NextPlayer()
		{
			if (++index == players.Count) index = 0;
		}
	}
}
