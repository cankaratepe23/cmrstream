using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Comarstream
{
    class Movie:ShowEntry
    {
        public string PosterUrl { get; set; }

        public static async Task<Movie> CreateAsync(string tvdbId)
        {
            Movie movie = new Movie
            {
                TvdbId = tvdbId
            };
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri("https://api.thetvdb.com/movies/")
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.Default.TVDB_Token);

            HttpResponseMessage response = await client.GetAsync(tvdbId);
            if (response.IsSuccessStatusCode)
            {
                string jsonStr = await response.Content.ReadAsStringAsync();
                dynamic jsonObj = JsonConvert.DeserializeObject(jsonStr);
                dynamic primaryTranslation = null;
                foreach (var translation in jsonObj.data.translations)
                {
                    if (Convert.ToBoolean(translation["is_primary"]))
                    {
                        primaryTranslation = translation;
                        break;
                    }
                }
                movie.Name = primaryTranslation.name;
                foreach (var remoteId in jsonObj.data.remoteids)
                {
                    if (remoteId["source_name"] == "IMDB")
                    {
                        movie.ImdbId = remoteId.id;
                    }
                }
                movie.Description = primaryTranslation.overview;
                movie.PosterUrl = string.Format("https://artworks.thetvdb.com/banners/movies/{0}/posters/{0}.jpg", tvdbId);
            }
            else
            {
                throw new HttpRequestException(response.StatusCode.ToString());
            }
            client.Dispose();
            return movie;
        }
    }
}