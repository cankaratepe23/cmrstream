using System;
using System.Collections.Generic;
using System.Text;

namespace Comarstream
{
    class ShowEntry
    {
        public string Name { get; set; }
        public string TvdbId { get; set; }
        public string ImdbId { get; set; }
        public string Description { get; set; }
        public string Rating { get; set; }
        public bool Downloaded { get; set; } = true;
        public string Path { get; set; }
    }
}
