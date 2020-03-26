using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Comarstream
{
    class ShowEntry : IEquatable<ShowEntry>
    {
        public string Name { get; set; }
        public string TvdbId { get; set; }
        public string ImdbId { get; set; }
        public string Description { get; set; }
        public string Rating { get; set; }
        public bool Downloaded { get; set; } = false;
        public string Path { get; set; }

        public override bool Equals(object other)
        {
            if (other == null)
            {
                return false;
            }
            ShowEntry showEntry = other as ShowEntry;
            if (showEntry == null)
            {
                return false;
            }
            else
            {
                return Equals(showEntry);
            }
        }

        public bool Equals(ShowEntry other)
        {
            if (other == null)
            {
                return false;
            }
            return this.TvdbId == other.TvdbId;
        }
    }
}
