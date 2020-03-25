using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Comarstream
{
    class ImdbParser
    {
        public string Rating { get; set; }
        public int ReviewCount { get; set; }
        public ImdbParser(string id)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load("https://imdb.com/title/" + id);
            HtmlNode ratingNode = doc.DocumentNode.SelectSingleNode("//*[@id=\"title-overview-widget\"]/div[1]/div[2]/div/div[1]/div[1]/div[1]/strong/span");
            HtmlNode reviewCountNode = doc.DocumentNode.SelectSingleNode("//*[@id=\"title-overview-widget\"]/div[1]/div[2]/div/div[1]/div[1]/a/span");
            Rating = ratingNode.InnerText;
            ReviewCount = Convert.ToInt32(reviewCountNode.InnerText.Replace(",", string.Empty));
        }
    }
}
