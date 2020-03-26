namespace Comarstream
{
    class Episode : ShowEntry
    {
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string SeriesId { get; set; }
        public string PosterUrl { get; set; }

        public static Episode CreateEpisode(int episodeNumber, int seasonNumber, string seriesId, dynamic jsonObj)
        {
            Episode episode = new Episode
            {
                EpisodeNumber = episodeNumber,
                SeasonNumber = seasonNumber,
                SeriesId = seriesId
            };
            var episodeData = jsonObj.data[episodeNumber - 1];
            episode.Name = episodeData.episodeName;
            episode.TvdbId = episodeData.id;
            episode.ImdbId = episodeData.imdbId;
            episode.Description = episodeData.overview;
            //ImdbParser parser = new ImdbParser(episode.ImdbId); //TODO Make sure this works!
            //episode.Rating = parser.Rating;
            return episode;
        }
    }
}