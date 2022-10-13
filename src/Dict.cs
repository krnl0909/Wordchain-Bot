using Newtonsoft.Json.Linq;

namespace WordchainBot
{
	public enum WordState
	{
		Existent, Non_existent, Failed_request,
		No_match_with_previous, One_letter_long,
		Repitition
	}

	static internal class Dict
	{
		static private readonly string? key = Environment.GetEnvironmentVariable("Dictionary_Key");
		static private string? URL;

		static public async Task<WordState> WordExist(string word)
		{
			try
			{
				URL = $"https://stdict.korean.go.kr/api/search.do?key={key}&req_type=json&q={word}&advanced=y&type1=word&pos=1,2,3";

				HttpClient client = new();
				HttpRequestMessage request = new(HttpMethod.Get, URL);

				// request.Headers.UserAgent.Append(new ProductInfoHeaderValue("끝말잇기봇#6912", "v0.1"));

				return Verify(await client.SendAsync(request)
					.Result.EnsureSuccessStatusCode().Content
					.ReadAsStringAsync())
					? WordState.Existent : WordState.Non_existent;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return WordState.Failed_request;
			}
		}

		static private bool Verify(string response)
		{
			string? total = JObject.Parse(response)["channel"]?["total"]?.ToString();

			if (total != null && int.Parse(total) > 0) return true;
			else return false;
		}
	}
}