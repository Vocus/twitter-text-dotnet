using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Vocus.TwitterText
{

    /// <summary>
    /// A class to extract usernames, lists, hashtags, and URLs from Tweet text.
    /// </summary>
    public class Extractor
    {
        #region Entity Class

        public class Entity
        {
            #region declarations
            public enum EntityTypeEnum
            {
                URL, HASHTAG, MENTION, CASHTAG
            }
            protected int _start;
            protected int _end;
            protected string _value;
            // listSlug is used to store the list portion of @mention/list.
            protected readonly string _listSlug;
            protected readonly EntityTypeEnum _entityType;

            protected string _displayURL = null;
            protected string _expandedURL = null;

            #endregion
            
            #region properties

            public int Start
            {
                get
                {
                    return _start;
                }
                set
                {
                    this._start = value;
                }
            }

            public int End
            {
                get
                {
                    return _end;
                }
                set
                {
                    this._end = value;
                }
            }

            public string Value
            {
                get
                {
                    return _value;
                }
                set
                {
                    _value = value;
                }
            }

            public string ListSlug
            {
                get
                {
                    return _listSlug;
                }
            }

            public EntityTypeEnum EntityType
            {
                get
                {
                    return _entityType;
                }
            }

            public string DisplayURL
            {
                get
                {
                    return _displayURL;
                }
                set { this._displayURL = value; }
            }

            public string ExpandedURL
            {
                get
                {
                    return _expandedURL;
                }
                set { this._expandedURL = value; }
            }

            #endregion

            #region constructors
            
            public Entity(int start, int end, String value, String listSlug, EntityTypeEnum type)
            {
                this._start = start;
                this._end = end;
                this._value = value;
                this._listSlug = listSlug;
                this._entityType = type;
            }

            public Entity(int start, int end, String value, EntityTypeEnum type)
                : this(start, end, value, null, type)
            {
            }

            public Entity(Match match, EntityTypeEnum type, int groupNumber) :
                // Offset -1 on start index to include @, # symbols for mentions and hashtags
                this(match, type, groupNumber, -1)
            { }

            public Entity(Match match, EntityTypeEnum type, int groupNumber, int startOffset) :
                this(match.Groups[groupNumber].Index + startOffset, match.Groups[groupNumber].Index + match.Groups[groupNumber].Length, match.Groups[groupNumber].ToString(), type)
            { }

            #endregion

            #region overrides
            
            public override bool Equals(Object obj)
            {
                if (this == obj)
                {
                    return true;
                }

                if (!(obj is Entity))
                {
                    return false;
                }

                Entity other = (Entity)obj;

                if (this.EntityType == other.EntityType &&
                    this.Start == other.Start &&
                    this.End == other.End &&
                    this.Value == other.Value)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode() {
              return this.EntityType.GetHashCode() + this.Value.GetHashCode() + this.Start + this.End;
            }

            public override string ToString()
            {
                return Value + "(" + EntityType + ") [" + Start + "," + End + "]";
            }

            #endregion
        }

        #endregion

        #region IndexConverter Class

        /// <summary>
        /// Convert indicies between code points and code units.
        /// This implementation is probably not efficient.
        /// </summary>
        private class IndexConverter
        {
            #region declarations

            protected readonly String text;
            protected readonly Char[] chars;
            protected int charLength = 0;

            #endregion

            #region constructors

            public IndexConverter(String text)
            {
                this.text = text;
                this.chars = text.ToCharArray();
                this.charLength = this.chars.Length;
            }

            #endregion

            #region public methods

            /// <summary>
            /// Convert multibyte character index in to single byte char index
            /// </summary>
            /// <param name="charIndex">Index into the string measured in code units</param>
            /// <returns>The code point index that corresponds to the specified character index</returns>
            public int CodeUnitsToCodePoints(int charIndex)
            {
                var charCounter = 0;
                var charsIndex = 0;

                while (charsIndex < charIndex)
                {
                    if ((charsIndex + 1 < this.charLength) && char.IsSurrogatePair(this.chars[charsIndex], this.chars[charsIndex + 1]))
                    {
                        charsIndex++;
                    }
                    charCounter++;
                    charsIndex++;
                }
                return charCounter;
            }

            /// <summary>
            /// Convert single byte char index in to multibyte character index
            /// </summary>
            /// <param name="codePointIndex">Index into the string measured in code points.</param>
            /// <returns>The code unit index that corresponds to the specified code point index.</returns>
            public int CodePointsToCodeUnits(int codePointIndex)
            {
                var charCounter = 0;
                var charsIndex = 0;

                while (charCounter < codePointIndex)
                {
                    if ((charsIndex + 1 < this.charLength) && char.IsSurrogatePair(this.chars[charsIndex], this.chars[charsIndex + 1]))
                    {
                        charsIndex++;
                    }
                    charCounter++;
                    charsIndex++;
                }

                return charsIndex;
            }

            #endregion
        }


        #endregion

        #region properties

        public bool ExtractURLWithoutProtocol { get; set; }

        #endregion

        #region constructors

        public Extractor()
        {
            this.ExtractURLWithoutProtocol = true;
        }

        #endregion

        #region private methods

        private void RemoveOverlappingEntities(List<Entity> entities)
        {
            // sort by index
            entities.Sort(delegate(Entity a, Entity b)
               {
                   int xdiff = a.Start.CompareTo(b.Start);
                   if (xdiff != 0) return xdiff;
                   else return a.End.CompareTo(b.End);
               }
            );
            
            // Remove overlapping entities.
            // Two entities overlap only when one is URL and the other is hashtag/mention
            // which is a part of the URL. When it happens, we choose URL over hashtag/mention
            // by selecting the one with smaller start index.
            if (entities.Count > 1)
            {
                List<Entity> keepEntities = new List<Entity>();
                Entity prev = entities[0];
                keepEntities.Add(prev);
                        
                for (var i = 1; i < entities.Count; i++)
                {
                    Entity cur = entities[i];
                    if (prev.End > cur.Start)
                    {
                        //removing current entity
                    }
                    else
                    {
                        keepEntities.Add(cur);
                        prev = cur;
                    }
                }

                entities.Clear();
                entities.AddRange(keepEntities);
            }
        }

        /// <summary>
        /// Extract #hashtag references from Tweet text.
        /// </summary>
        /// <param name="text">text of the tweet from which to extract hashtags</param>
        /// <param name="checkUrlOverlap">if true, check if extracted hashtags overlap URLs and remove overlapping ones</param>
        /// <returns>List of hashtags referenced (without the leading # sign)</returns>
        private List<Entity> ExtractHashtagsWithIndices(String text, bool checkUrlOverlap)
        {
            if (String.IsNullOrEmpty(text))
            {
                return new List<Entity>();
            }

            // Performance optimization.
            // If text doesn't contain #/＃ at all, text doesn't contain
            // hashtag, so we can simply return an empty list.
            bool found = false;
            foreach (char c in text.ToArray())
            {
                if (c == '#' || c == '＃')
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                return new List<Entity>();
            }

            List<Entity> extracted = new List<Entity>();
            MatchCollection matches = Regex.VALID_HASHTAG.Matches(text);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    String after = text.Substring(match.Index + match.Length);
                    if (!Regex.INVALID_HASHTAG_MATCH_END.IsMatch(after))
                    {
                        extracted.Add(new Entity(match, Entity.EntityTypeEnum.HASHTAG, Regex.VALID_HASHTAG_GROUP_TAG));
                    }
                }
            }

            if (checkUrlOverlap)
            {
                // extract URLs
                List<Entity> urls = ExtractURLsWithIndices(text);
                if (urls.Count > 0)
                {
                    extracted.AddRange(urls);
                    // remove overlap
                    RemoveOverlappingEntities(extracted);
                    // remove URL entities
                    extracted = extracted.Where(e => e.EntityType == Entity.EntityTypeEnum.HASHTAG).ToList();
                }
            }

            return extracted;
        }


        #endregion

        #region public methods

        /// <summary>
        /// Extract URLs, @mentions, lists, and #hashtag from a give text/tweet.
        /// </summary>
        /// <param name="text">text of tweet</param>
        /// <returns>list of extracted entities</returns>
        public List<Entity> ExtractEntitiesWithIndices(String text)
        {
            List<Entity> entities = new List<Entity>();
            entities.AddRange(ExtractURLsWithIndices(text));
            entities.AddRange(ExtractHashtagsWithIndices(text, false));
            entities.AddRange(ExtractMentionsOrListsWithIndices(text));
            entities.AddRange(ExtractCashtagsWithIndices(text));

            RemoveOverlappingEntities(entities);
            return entities;
        }

        /// <summary>
        /// Extract @username references from Tweet text. A mention is an occurance of @username anywhere in a Tweet.
        /// </summary>
        /// <param name="text">text of the tweet from which to extract usernames</param>
        /// <returns>List of usernames referenced (without the leading @ sign)</returns>
        public List<String> ExtractMentionedScreennames(String text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return new List<string>();
            }

            List<String> extracted = new List<String>();
            foreach (Entity entity in ExtractMentionedScreennamesWithIndices(text))
            {
                extracted.Add(entity.Value);
            }
            return extracted;
        }

        /// <summary>
        /// Extract @username references from Tweet text. A mention is an occurance of @username anywhere in a Tweet.
        /// </summary>
        /// <param name="text">v</param>
        /// <returns>List of usernames referenced (without the leading @ sign)</returns>
        public List<Entity> ExtractMentionedScreennamesWithIndices(String text)
        {
            List<Entity> extracted = new List<Entity>();
            foreach (Entity entity in ExtractMentionsOrListsWithIndices(text))
            {
                if (entity.ListSlug == null)
                {
                    extracted.Add(entity);
                }
            }
            return extracted;
        }

        public List<Entity> ExtractMentionsOrListsWithIndices(String text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return new List<Entity>();
            }

            // Performance optimization.
            // If text doesn't contain @/＠ at all, the text doesn't
            // contain @mention. So we can simply return an empty list.
            bool found = false;
            foreach (char c in text.ToArray())
            {
                if (c == '@' || c == '＠')
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                return new List<Entity>();
            }

            List<Entity> extracted = new List<Entity>();
            MatchCollection matches = Regex.VALID_MENTION_OR_LIST.Matches(text);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    String after = text.Substring(match.Index + match.Length);
                    if (!Regex.INVALID_MENTION_MATCH_END.IsMatch(after))
                    {
                        if (match.Groups[Regex.VALID_MENTION_OR_LIST_GROUP_LIST].Success == false)
                        {
                            extracted.Add(new Entity(match, Entity.EntityTypeEnum.MENTION, Regex.VALID_MENTION_OR_LIST_GROUP_USERNAME));
                        }
                        else
                        {
                            extracted.Add(new Entity(match.Groups[Regex.VALID_MENTION_OR_LIST_GROUP_USERNAME].Index - 1,
                                match.Groups[Regex.VALID_MENTION_OR_LIST_GROUP_LIST].Index + match.Groups[Regex.VALID_MENTION_OR_LIST_GROUP_LIST].Length,
                                match.Groups[Regex.VALID_MENTION_OR_LIST_GROUP_USERNAME].ToString(),
                                match.Groups[Regex.VALID_MENTION_OR_LIST_GROUP_LIST].ToString(),
                                Entity.EntityTypeEnum.MENTION));
                        }
                    }
                }
            }
            return extracted;
        }

        /// <summary>
        /// Extract a @username reference from the beginning of Tweet text. A reply is an occurance of @username at the
        /// beginning of a Tweet, preceded by 0 or more spaces
        /// </summary>
        /// <param name="text">text of the tweet from which to extract the replied to username</param>
        /// <returns>username referenced, if any (without the leading @ sign). Returns null if this is not a reply.</returns>
        public String ExtractReplyScreenname(String text)
        {
            if (text == null)
            {
                return null;
            }

            MatchCollection matches = Regex.VALID_REPLY.Matches(text);
            if (matches.Count > 0)
            {
                String after = text.Substring(matches[0].Index + matches[0].Length);
                if (Regex.INVALID_MENTION_MATCH_END.IsMatch(after))
                {
                    return null;
                }
                else
                {
                    return matches[0].Groups[Regex.VALID_REPLY_GROUP_USERNAME].ToString();
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Extract URL references from Tweet text.
        /// </summary>
        /// <param name="text">text of the tweet from which to extract URLs</param>
        /// <returns>List of URLs referenced.</returns>
        public List<String> ExtractURLs(String text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return new List<String>();
            }

            List<String> urls = new List<String>();
            foreach (Entity entity in ExtractURLsWithIndices(text))
            {
                urls.Add(entity.Value);
            }
            return urls;
        }

        /// <summary>
        /// Extract URL references from Tweet text.
        /// </summary>
        /// <param name="text">text of the tweet from which to extract URLs</param>
        /// <returns>List of URLs referenced.</returns>
        public List<Entity> ExtractURLsWithIndices(String text)
        {
            if (String.IsNullOrEmpty(text)
                || (ExtractURLWithoutProtocol ? text.IndexOf('.') : text.IndexOf(':')) == -1)
            {
                // Performance optimization.
                // If text doesn't contain '.' or ':' at all, text doesn't contain URL,
                // so we can simply return an empty list.
                return new List<Entity>();
            }

            List<Entity> urls = new List<Entity>();

            MatchCollection matches = Regex.VALID_URL.Matches(text);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    String before = match.Groups[Regex.VALID_URL_GROUP_BEFORE].ToString();
                    String url = match.Groups[Regex.VALID_URL_GROUP_URL].ToString();
                    String protocol = match.Groups[Regex.VALID_URL_GROUP_PROTOCOL].ToString();
                    String domain = match.Groups[Regex.VALID_URL_GROUP_DOMAIN].ToString();
                    Group pathGroup = match.Groups[Regex.VALID_URL_GROUP_PATH];
                    int start = match.Groups[Regex.VALID_URL_GROUP_URL].Index;
                    int end = start + match.Groups[Regex.VALID_URL_GROUP_URL].Length;
                    
                    if (match.Groups[Regex.VALID_URL_GROUP_PROTOCOL].Success == false)
                    {
                        // skip if protocol is not present and 'extractURLWithoutProtocol' is false
                        // or URL is preceded by invalid character.
                        if (!ExtractURLWithoutProtocol
                            || Regex.INVALID_URL_WITHOUT_PROTOCOL_MATCH_BEGIN
                                    .IsMatch(match.Groups[Regex.VALID_URL_GROUP_BEFORE].ToString()))
                        {
                            continue;
                        }

                        Entity lastUrl = null;
                        bool lastUrlInvalidMatch = false;
                        int asciiEndPosition = 0;

                        Regex.VALID_URL_ASCII.Replace(domain, delegate(Match m)
                        {
                            var asciiDomain = m.ToString();
                            var asciiStartPosition = domain.IndexOf(asciiDomain, asciiEndPosition);
                            asciiEndPosition = asciiStartPosition + asciiDomain.Length;

                            lastUrl = new Entity(start + asciiStartPosition, start + asciiEndPosition, asciiDomain, Entity.EntityTypeEnum.URL);
                            
                            lastUrlInvalidMatch = Regex.INVALID_SHORT_DOMAIN.IsMatch(asciiDomain);
                            if (!lastUrlInvalidMatch)
                            {
                                urls.Add(lastUrl);
                            }
                            return m.ToString();
                        });

                        // no ASCII-only domain found. Skip the entire URL.
                        if (lastUrl == null)
                        {
                            continue;
                        }

                        // lastUrl only contains domain. Need to add path and query if they exist.
                        if (pathGroup.Success)
                        {
                            if (lastUrlInvalidMatch)
                            {
                                urls.Add(lastUrl);
                            }
                            lastUrl.Value = url.Replace(domain, lastUrl.Value);
                            lastUrl.End = end;
                        }
                    }
                    else
                    {
                        MatchCollection tco_matches = Regex.VALID_TCO_URL.Matches(url);
                        if (tco_matches.Count > 0)
                        {
                            // In the case of t.co URLs, don't allow additional path characters.
                            url = tco_matches[0].Groups[0].ToString();
                            end = start + url.Length;
                        }

                        urls.Add(new Entity(start, end, url, Entity.EntityTypeEnum.URL));
                    }
                    
                }
            }

            return urls;
        }

        /// <summary>
        /// Extract #hashtag references from Tweet text.
        /// </summary>
        /// <param name="text">text of the tweet from which to extract hashtags</param>
        /// <returns>List of hashtags referenced (without the leading # sign)</returns>
        public List<String> ExtractHashtags(String text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return new List<string>();
            }

            List<String> extracted = new List<String>();
            foreach (Entity entity in ExtractHashtagsWithIndices(text))
            {
                extracted.Add(entity.Value);
            }

            return extracted;
        }

        /// <summary>
        /// Extract #hashtag references from Tweet text.
        /// </summary>
        /// <param name="text">text of the tweet from which to extract hashtags</param>
        /// <returns>List of hashtags referenced (without the leading # sign)</returns>
        public List<Entity> ExtractHashtagsWithIndices(String text)
        {
            return ExtractHashtagsWithIndices(text, true);
        }

        /// <summary>
        /// Extract $cashtag references from Tweet text.
        /// </summary>
        /// <param name="text">text of the tweet from which to extract cashtags</param>
        /// <returns>List of cashtags referenced (without the leading $ sign)</returns>
        public List<String> ExtractCashtags(String text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return new List<String>();
            }

            List<String> extracted = new List<String>();
            foreach (Entity entity in ExtractCashtagsWithIndices(text))
            {
                extracted.Add(entity.Value);
            }

            return extracted;
        }

        /// <summary>
        /// Extract $cashtag references from Tweet text.
        /// </summary>
        /// <param name="text">text of the tweet from which to extract cashtags</param>
        /// <returns>List of cashtags referenced (without the leading $ sign)</returns>
        public List<Entity> ExtractCashtagsWithIndices(String text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return new List<Entity>();
            }

            // Performance optimization.
            // If text doesn't contain $, text doesn't contain
            // cashtag, so we can simply return an empty list.
            if (text.IndexOf('$') == -1)
            {
                return new List<Entity>();

            }

            List<Entity> extracted = new List<Entity>();
            MatchCollection matches = Regex.VALID_CASHTAG.Matches(text);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    extracted.Add(new Entity(match, Entity.EntityTypeEnum.CASHTAG, Regex.VALID_CASHTAG_GROUP_CASHTAG));
                }
            }

            return extracted;
        }

        /// <summary>
        /// Modify Unicode-based indices of the entities to UTF-16 based indices.
        /// 
        /// In UTF-16 based indices, Unicode supplementary characters are counted as two characters.
        /// 
        /// This method requires that the list of entities be in ascending order by start index.
        /// </summary>
        /// <param name="text">original text</param>
        /// <param name="entities">entities with Unicode based indices</param>
        public void ModifyIndicesFromUnicodeToUTF16(String text, List<Entity> entities)
        {
            IndexConverter convert = new IndexConverter(text);

            foreach (Entity entity in entities) {
              entity.Start = convert.CodePointsToCodeUnits(entity.Start);
              entity.End = convert.CodePointsToCodeUnits(entity.End);
            }
        }

        /// <summary>
        /// Modify UTF-16-based indices of the entities to Unicode-based indices.
        /// 
        /// In Unicode-based indices, Unicode supplementary characters are counted as single characters.
        /// 
        /// This method requires that the list of entities be in ascending order by start index.
        /// </summary>
        /// <param name="text">original text</param>
        /// <param name="entities">entities with UTF-16 based indices</param>
        public void ModifyIndicesFromUTF16ToUnicode(String text, List<Entity> entities)
        {
            IndexConverter convert = new IndexConverter(text);

            foreach (Entity entity in entities) {
              entity.Start = convert.CodeUnitsToCodePoints(entity.Start);
              entity.End = convert.CodeUnitsToCodePoints(entity.End);
            }
        }

        #endregion

    }
}