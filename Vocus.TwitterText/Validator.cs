using System;
using System.Linq;
using System.Text;
namespace Vocus.TwitterText
{
    /// <summary>
    /// A class for validating Tweet texts.
    /// </summary>
    public class Validator
    {
        #region declarations

        public static readonly int MAX_TWEET_LENGTH = 140;

        protected int _shortUrlLength = 22;
        protected int _shortUrlLengthHttps = 23;

        private Extractor _extractor = new Extractor();

        #endregion

        #region properties

        public int ShortUrlLength
        {
            get { return _shortUrlLength; }
            set { _shortUrlLength = value; }
        }

        public int ShortUrlLengthHttps
        {
            get { return _shortUrlLengthHttps; }
            set { _shortUrlLengthHttps = value; }
        }

        #endregion

        #region public methods

        public int GetTweetLength(String text)
        {
            text = text.Normalize(NormalizationForm.FormC);
            int surrogateCount = 0;
            var chars = text.ToCharArray();
            for (var i = 0; i < chars.Length - 1; i++)
            {
                if (char.IsSurrogatePair(chars[i], chars[i + 1]))
                {
                    surrogateCount++;
                    i++;
                }
            }
            int length = text.Length - surrogateCount;
          
            foreach (Extractor.Entity urlEntity in _extractor.ExtractURLsWithIndices(text))
            {
                length += urlEntity.Start - urlEntity.End;
                length += urlEntity.Value.ToLower().StartsWith("https://") ? _shortUrlLengthHttps : _shortUrlLength;
            }

            return length;
        }

        public bool IsValidTweet(String text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return false;
            }

            foreach (char c in text.ToCharArray())
            {
                if (c == '\uFFFE' || c == '\uFEFF' ||   // BOM
                    c == '\uFFFF' ||                     // Special
                    (c >= '\u202A' && c <= '\u202E'))
                {  // Direction change
                    return false;
                }
            }

            return GetTweetLength(text) <= MAX_TWEET_LENGTH;
        }

        #endregion
    }
}