using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Comarstream
{
    class Series:ShowEntry
    {
        public int SeasonCount { get; set; }
        public List<Season> Seasons { get; set; }
        public string PosterUrl { get; set; }

        public static async Task<Series> CreateAsync(string tvdbId)
        {
            Series series = new Series
            {
                TvdbId = tvdbId
            };

            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri("https://api.thetvdb.com/series/")
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.Default.TVDB_Token);

            HttpResponseMessage response = await client.GetAsync($"{tvdbId}/filter?keys=seriesName%2Cid%2CimdbId%2Coverview%2Cposter%2Cseason");
            if (response.IsSuccessStatusCode)
            {
                string jsonStr = await response.Content.ReadAsStringAsync();
                dynamic jsonObj = JsonConvert.DeserializeObject(jsonStr);
                series.Name = jsonObj.data.seriesName;
                series.ImdbId = jsonObj.data.imdbId;
                series.Description = jsonObj.data.overview;
                series.PosterUrl = "https://artworks.thetvdb.com/banners/" + jsonObj.data.poster;
                series.SeasonCount = Convert.ToInt32(jsonObj.data.season);
                series.Seasons = new List<Season>(series.SeasonCount);
                for (int i = 1; i <= series.SeasonCount; i++)
                {
                    series.Seasons.Add(await Season.CreateAsync(i, tvdbId));
                }
                ImdbParser parser = new ImdbParser(series.ImdbId);
                series.Rating = parser.Rating;
            }
            else
            {
                throw new HttpRequestException(response.StatusCode.ToString());
            }
            client.Dispose();
            return series;
        }
    }
}
