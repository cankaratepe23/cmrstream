using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Comarstream
{
    class Season
    {
        public int SeasonNumber { get; set; }
        public string TvdbSeriesId { get; set; }
        public int EpisodeCount { get; set; }
        public List<Episode> Episodes { get; set; }
        public string PosterUrl { get; set; }
        public string Path { get; set; }

        public static async Task<Season> CreateAsync(int seasonNumber, string seriesId)
        {
            Season season = new Season
            {
                SeasonNumber = seasonNumber,
                TvdbSeriesId = seriesId
            };

            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri("https://api.thetvdb.com/series/")
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.Default.TVDB_Token);

            HttpResponseMessage response = await client.GetAsync(seriesId + "/images/query" +
                "?keyType=season&subKey=" + seasonNumber);
            if (response.IsSuccessStatusCode)
            {
                string jsonStr = await response.Content.ReadAsStringAsync();
                dynamic jsonObj = JsonConvert.DeserializeObject(jsonStr);
                season.PosterUrl = "https://artworks.thetvdb.com/banners/" + jsonObj.data[0].fileName;
            }

            response = await client.GetAsync(seriesId + "/episodes/query?airedSeason=" + season.SeasonNumber);
            if (response.IsSuccessStatusCode)
            {
                string jsonStr = await response.Content.ReadAsStringAsync();
                dynamic jsonObj = JsonConvert.DeserializeObject(jsonStr);
                season.EpisodeCount = jsonObj.data.Count;
                season.Episodes = new List<Episode>(season.EpisodeCount);
                for (int i = 1; i <= season.EpisodeCount; i++)
                {
                    season.Episodes.Add(Episode.CreateEpisode(i, season.SeasonNumber, seriesId, jsonObj));
                }
            }
            client.Dispose();
            return season;
        }
    }
}
