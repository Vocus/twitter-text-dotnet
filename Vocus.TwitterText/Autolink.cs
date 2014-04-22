using System;
using System.Collections.Generic;
using System.Text;

namespace Vocus.TwitterText
{
    public delegate void LinkAttributeModifier(Extractor.Entity entity, Dictionary<String, String> attributes);
    public delegate string LinkTextModifier(Extractor.Entity entity, string text);
    
    /// <summary>
    /// A Class for adding HTML links to hashtag, username, and list references in Tweet text.
    /// </summary>
    public class Autolink
    {
        #region declarations

        /** Default CSS class for auto-linked list URLs */
        public static readonly String DEFAULT_LIST_CLASS = "tweet-url list-slug";
        /** Default CSS class for auto-linked username URLs */
        public static readonly String DEFAULT_USERNAME_CLASS = "tweet-url username";
        /** Default CSS class for auto-linked hashtag URLs */
        public static readonly String DEFAULT_HASHTAG_CLASS = "tweet-url hashtag";
        /** Default CSS class for auto-linked cashtag URLs */
        public static readonly String DEFAULT_CASHTAG_CLASS = "tweet-url cashtag";
        /** Default href for username links (the username without the @ will be appended) */
        public static readonly String DEFAULT_USERNAME_URL_BASE = "https://twitter.com/";
        /** Default href for list links (the username/list without the @ will be appended) */
        public static readonly String DEFAULT_LIST_URL_BASE = "https://twitter.com/";
        /** Default href for hashtag links (the hashtag without the # will be appended) */
        public static readonly String DEFAULT_HASHTAG_URL_BASE = "https://twitter.com/#!/search?q=%23";
        /** Default href for cashtag links (the cashtag without the $ will be appended) */
        public static readonly String DEFAULT_CASHTAG_URL_BASE = "https://twitter.com/#!/search?q=%24";
        /** Default attribute for invisible span tag */
        public static readonly String DEFAULT_INVISIBLE_TAG_ATTRS = "style='position:absolute;left:-9999px;'";

        protected String _urlClass = null;
        protected String _listClass;
        protected String _usernameClass;
        protected String _hashtagClass;
        protected String _cashtagClass;
        protected String _usernameUrlBase;
        protected String _listUrlBase;
        protected String _hashtagUrlBase;
        protected String _cashtagUrlBase;
        protected String _invisibleTagAttrs;
        protected bool _noFollow = true;
        protected bool _usernameIncludeSymbol = false;
        protected String _symbolTag = null;
        protected String _textWithSymbolTag = null;
        protected String _urlTarget = null;
        protected LinkAttributeModifier _linkAttributeModifier = null;
        protected LinkTextModifier _linkTextModifier = null;

        private Extractor _extractor = new Extractor();

        #endregion

        #region properties

        /// <summary>
        /// CSS class for auto-linked URLs
        /// </summary>
        public String UrlClass
        {
            get { return _urlClass; }
            set { _urlClass = value; }
        }

        /// <summary>
        /// CSS class for auto-linked list URLs
        /// </summary>
        public String ListClass
        {
            get { return _listClass; }
            set { _listClass = value; }
        }

        /// <summary>
        /// CSS class for auto-linked username URLs
        /// </summary>
        public String UsernameClass
        {
            get { return _usernameClass; }
            set { _usernameClass = value; }
        }

        /// <summary>
        /// CSS class for auto-linked hashtag URLs
        /// </summary>
        public String HashtagClass
        {
            get { return _hashtagClass; }
            set { _hashtagClass = value; }
        }

        /// <summary>
        /// CSS class for auto-linked cashtag URLs
        /// </summary>
        public String CashtagClass
        {
            get { return _cashtagClass; }
            set { _cashtagClass = value; }
        }


        /// <summary>
        /// the href value for username links (to which the username will be appended)
        /// </summary>
        public String UsernameUrlBase
        {
            get { return _usernameUrlBase; }
            set { _usernameUrlBase = value; }
        }

        /// <summary>
        /// the href value for list links (to which the username/list will be appended)
        /// </summary>
        public String ListUrlBase
        {
            get { return _listUrlBase; }
            set { _listUrlBase = value; }
        }

        /// <summary>
        /// the href value for hashtag links (to which the hashtag will be appended)
        /// </summary>
        public String HashtagUrlBase
        {
            get { return _hashtagUrlBase; }
            set { _hashtagUrlBase = value; }
        }

        /// <summary>
        /// the href value for cashtag links (to which the cashtag will be appended)
        /// </summary>
        public String CashtagUrlBase
        {
            get { return _cashtagUrlBase; }
            set { _cashtagUrlBase = value; }
        }

        /// <summary>
        /// if the current URL links will include rel="nofollow" (true by default)
        /// </summary>
        public bool NoFollow
        {
            get { return _noFollow; }
            set { _noFollow = value; }
        }

        /// <summary>
        /// Set if the at mark '@' should be included in the link (false by default)
        /// </summary>
        public bool UsernameIncludeSymbol
        {
            set { _usernameIncludeSymbol = value; }
        }

        /// <summary>
        /// Set HTML tag to be applied around #/@/# symbols in hashtags/usernames/lists/cashtag
        /// </summary>
        public string SymbolTag
        {
            set { _symbolTag = value; }
        }

        /// <summary>
        /// Set HTML tag to be applied around text part of hashtags/usernames/lists/cashtag
        /// </summary>
        public string TextWithSymbolTag
        {
            set { _textWithSymbolTag = value; }
        }

        /// <summary>
        /// Set the value of the target attribute in auto-linked URLs
        /// </summary>
        public string UrlTarget
        {
            set { _urlTarget = value; }
        }

        /// <summary>
        /// Set a modifier to modify attributes of a link based on an entity
        /// </summary>
        public LinkAttributeModifier LinkAttributeModifier
        {
            set { _linkAttributeModifier = value; }
        }

        /// <summary>
        /// Set a modifier to modify text of a link based on an entity
        /// </summary>
        public LinkTextModifier LinkTextModifier
        {
            set { _linkTextModifier = value; }
        }


        #endregion

        #region constructors

        public Autolink()
        {
            _urlClass = null;
            _listClass = DEFAULT_LIST_CLASS;
            _usernameClass = DEFAULT_USERNAME_CLASS;
            _hashtagClass = DEFAULT_HASHTAG_CLASS;
            _cashtagClass = DEFAULT_CASHTAG_CLASS;
            _usernameUrlBase = DEFAULT_USERNAME_URL_BASE;
            _listUrlBase = DEFAULT_LIST_URL_BASE;
            _hashtagUrlBase = DEFAULT_HASHTAG_URL_BASE;
            _cashtagUrlBase = DEFAULT_CASHTAG_URL_BASE;
            _invisibleTagAttrs = DEFAULT_INVISIBLE_TAG_ATTRS;
            
            _extractor.ExtractURLWithoutProtocol = false;
        }

        #endregion

        #region public methods

        public String EscapeBrackets(String text)
        {
            int len = text.Length;
            if (len == 0)
                return text;

            StringBuilder sb = new StringBuilder(len + 16);
            for (int i = 0; i < len; ++i)
            {
                char c = text[i];
                if (c == '>')
                    sb.Append("&gt;");
                else if (c == '<')
                    sb.Append("&lt;");
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public void LinkToText(Extractor.Entity entity, String text, Dictionary<String, String> attributes, StringBuilder builder)
        {
            if (_noFollow)
            {
                attributes.Add("rel", "nofollow");
            }
            if (_linkAttributeModifier != null)
            {
                _linkAttributeModifier(entity, attributes);
            }
            if (_linkTextModifier != null)
            {
                text = _linkTextModifier(entity, text);
            }
            // append <a> tag
            builder.Append("<a");
            foreach (var entry in attributes)
            {
                builder.Append(" ").Append(EscapeHTML(entry.Key)).Append("=\"").Append(EscapeHTML(entry.Value)).Append("\"");
            }
            builder.Append(">").Append(text).Append("</a>");
        }

        public void LinkToTextWithSymbol(Extractor.Entity entity, string symbol, string text, Dictionary<String, String> attributes, StringBuilder builder)
        {
            string taggedSymbol = String.IsNullOrEmpty(_symbolTag) ? symbol : String.Format("<{0}>{1}</{0}>", _symbolTag, symbol);
            text = EscapeHTML(text);
            string taggedText = String.IsNullOrEmpty(_textWithSymbolTag) ? text : String.Format("<{0}>{1}</{0}>", _textWithSymbolTag, text);

            bool includeSymbol = _usernameIncludeSymbol || !Regex.AT_SIGNS.IsMatch(symbol);

            if (includeSymbol)
            {
                LinkToText(entity, taggedSymbol + taggedText, attributes, builder);
            }
            else
            {
                builder.Append(taggedSymbol);
                LinkToText(entity, taggedText, attributes, builder);
            }
        }

        public void LinkToHashtag(Extractor.Entity entity, String text, StringBuilder builder)
        {
            // Get the original hash char from text as it could be a full-width char.
            string hashChar = text.Substring(entity.Start, 1);
            string hashtag = entity.Value;

            Dictionary<String, String> attrs = new Dictionary<String, String>();
            attrs.Add("href", _hashtagUrlBase + hashtag);
            attrs.Add("title", "#" + hashtag);
            attrs.Add("class", _hashtagClass);

            if (Regex.RTL_CHARACTERS.IsMatch(hashtag))
            {
                attrs["class"] += " rtl";
            }

            LinkToTextWithSymbol(entity, hashChar, hashtag, attrs, builder);
        }

        public void LinkToCashtag(Extractor.Entity entity, String text, StringBuilder builder)
        {
            string cashtag = entity.Value;

            Dictionary<String, String> attrs = new Dictionary<String, String>();
            attrs.Add("href", _cashtagUrlBase + cashtag);
            attrs.Add("title", "$" + cashtag);
            attrs.Add("class", _cashtagClass);

            LinkToTextWithSymbol(entity, "$", cashtag, attrs, builder);
        }

        public void LinkToMentionAndList(Extractor.Entity entity, String text, StringBuilder builder)
        {
            String mention = entity.Value;
            // Get the original at char from text as it could be a full-width char.
            string atChar = text.Substring(entity.Start, 1);

            Dictionary<String, String> attrs = new Dictionary<String, String>();
            if (entity.ListSlug != null)
            {
                mention += entity.ListSlug;
                attrs.Add("class", _listClass);
                attrs.Add("href", _listUrlBase + mention);
            }
            else
            {
                attrs.Add("class", _usernameClass);
                attrs.Add("href", _usernameUrlBase + mention);
            }

            LinkToTextWithSymbol(entity, atChar, mention, attrs, builder);
        }

        public void LinkToURL(Extractor.Entity entity, String text, StringBuilder builder)
        {
            string url = entity.Value;
            string linkText = EscapeHTML(url);

            if (entity.DisplayURL != null && entity.ExpandedURL != null)
            {
                // Goal: If a user copies and pastes a tweet containing t.co'ed link, the resulting paste
                // should contain the full original URL (expanded_url), not the display URL.
                //
                // Method: Whenever possible, we actually emit HTML that contains expanded_url, and use
                // font-size:0 to hide those parts that should not be displayed (because they are not part of display_url).
                // Elements with font-size:0 get copied even though they are not visible.
                // Note that display:none doesn't work here. Elements with display:none don't get copied.
                //
                // Additionally, we want to *display* ellipses, but we don't want them copied.  To make this happen we
                // wrap the ellipses in a tco-ellipsis class and provide an onCopy handler that sets display:none on
                // everything with the tco-ellipsis class.
                //
                // As an example: The user tweets "hi http://longdomainname.com/foo"
                // This gets shortened to "hi http://t.co/xyzabc", with display_url = "…nname.com/foo"
                // This will get rendered as:
                // <span class='tco-ellipsis'> <!-- This stuff should get displayed but not copied -->
                //   …
                //   <!-- There's a chance the onCopy event handler might not fire. In case that happens,
                //        we include an &nbsp; here so that the … doesn't bump up against the URL and ruin it.
                //        The &nbsp; is inside the tco-ellipsis span so that when the onCopy handler *does*
                //        fire, it doesn't get copied.  Otherwise the copied text would have two spaces in a row,
                //        e.g. "hi  http://longdomainname.com/foo".
                //   <span style='font-size:0'>&nbsp;</span>
                // </span>
                // <span style='font-size:0'>  <!-- This stuff should get copied but not displayed -->
                //   http://longdomai
                // </span>
                // <span class='js-display-url'> <!-- This stuff should get displayed *and* copied -->
                //   nname.com/foo
                // </span>
                // <span class='tco-ellipsis'> <!-- This stuff should get displayed but not copied -->
                //   <span style='font-size:0'>&nbsp;</span>
                //   …
                // </span>
                //
                // Exception: pic.twitter.com images, for which expandedUrl = "https://twitter.com/#!/username/status/1234/photo/1
                // For those URLs, display_url is not a substring of expanded_url, so we don't do anything special to render the elided parts.
                // For a pic.twitter.com URL, the only elided part will be the "https://", so this is fine.
                String displayURLSansEllipses = entity.DisplayURL.Replace("…", "");
                int diplayURLIndexInExpandedURL = entity.ExpandedURL.IndexOf(displayURLSansEllipses);
                if (diplayURLIndexInExpandedURL != -1)
                {
                    String beforeDisplayURL = entity.ExpandedURL.Substring(0, diplayURLIndexInExpandedURL);
                    String afterDisplayURL = entity.ExpandedURL.Substring(diplayURLIndexInExpandedURL + displayURLSansEllipses.Length);
                    String precedingEllipsis = entity.DisplayURL.StartsWith("…") ? "…" : "";
                    String followingEllipsis = entity.DisplayURL.EndsWith("…") ? "…" : "";
                    String invisibleSpan = "<span " + _invisibleTagAttrs + ">";

                    StringBuilder sb = new StringBuilder("<span class='tco-ellipsis'>");
                    sb.Append(precedingEllipsis);
                    sb.Append(invisibleSpan).Append("&nbsp;</span></span>");
                    sb.Append(invisibleSpan).Append(EscapeHTML(beforeDisplayURL)).Append("</span>");
                    sb.Append("<span class='js-display-url'>").Append(EscapeHTML(displayURLSansEllipses)).Append("</span>");
                    sb.Append(invisibleSpan).Append(EscapeHTML(afterDisplayURL)).Append("</span>");
                    sb.Append("<span class='tco-ellipsis'>").Append(invisibleSpan).Append("&nbsp;</span>").Append(followingEllipsis).Append("</span>");

                    linkText = sb.ToString();
                }
                else
                {
                    linkText = entity.DisplayURL;
                }
            }

            Dictionary<String, String> attrs = new Dictionary<String, String>();
            attrs.Add("href", url);
            if (!String.IsNullOrEmpty(_urlClass))
            {
                attrs.Add("class", _urlClass);
            }
            if (!String.IsNullOrEmpty(_urlTarget))
            {
                attrs.Add("target", _urlTarget);
            }
            LinkToText(entity, linkText, attrs, builder);
        }

        public String AutoLinkEntities(String text, List<Extractor.Entity> entities)
        {
            StringBuilder builder = new StringBuilder(text.Length * 2);
            int beginIndex = 0;

            foreach (Extractor.Entity entity in entities)
            {
                builder.Append(text.Substring(beginIndex, entity.Start - beginIndex));

                switch (entity.EntityType)
                {
                    case Extractor.Entity.EntityTypeEnum.URL:
                        LinkToURL(entity, text, builder);
                        break;
                    case Extractor.Entity.EntityTypeEnum.HASHTAG:
                        LinkToHashtag(entity, text, builder);
                        break;
                    case Extractor.Entity.EntityTypeEnum.MENTION:
                        LinkToMentionAndList(entity, text, builder);
                        break;
                    case Extractor.Entity.EntityTypeEnum.CASHTAG:
                        LinkToCashtag(entity, text, builder);
                        break;
                }
                beginIndex = entity.End;
            }
            builder.Append(text.Substring(beginIndex));

            return builder.ToString();
        }

        /// <summary>
        /// Auto-link hashtags, URLs, usernames, and lists.
        /// </summary>
        /// <param name="text">text of the Tweet to auto-link</param>
        /// <returns>text with auto-link HTML added</returns>
        public String AutoLink(String text)
        {
            text = EscapeBrackets(text);

            // extract entities
            List<Extractor.Entity> entities = _extractor.ExtractEntitiesWithIndices(text);
            return AutoLinkEntities(text, entities);
        }

        /// <summary>
        /// Auto-link the @username and @username/list references in the provided text. Links to @username references will
        /// have the usernameClass CSS classes added. Links to @username/list references will have the listClass CSS class
        /// added.
        /// </summary>
        /// <param name="text">text of the Tweet to auto-link</param>
        /// <returns>text with auto-link HTML added</returns>
        public String AutoLinkUsernamesAndLists(String text)
        {
            return AutoLinkEntities(text, _extractor.ExtractMentionsOrListsWithIndices(text));
        }

        /// <summary>
        /// Auto-link #hashtag references in the provided Tweet text. The #hashtag links will have the hashtagClass CSS class
        /// added.
        /// </summary>
        /// <param name="text">text of the Tweet to auto-link</param>
        /// <returns>text with auto-link HTML added</returns>
        public String AutoLinkHashtags(String text)
        {
            return AutoLinkEntities(text, _extractor.ExtractHashtagsWithIndices(text));
        }

        /// <summary>
        /// Auto-link URLs in the Tweet text provided.
        /// 
        /// This only auto-links URLs with protocol.
        /// </summary>
        /// <param name="text">text of the Tweet to auto-link</param>
        /// <returns>text with auto-link HTML added</returns>
        public String AutoLinkURLs(String text)
        {
            return AutoLinkEntities(text, _extractor.ExtractURLsWithIndices(text));
        }

        /// <summary>
        /// Auto-link $cashtag references in the provided Tweet text. The $cashtag links will have the cashtagClass CSS class
        /// added.
        /// </summary>
        /// <param name="text">text of the Tweet to auto-link</param>
        /// <returns>text with auto-link HTML added</returns>
        public String AutoLinkCashtags(String text)
        {
            return AutoLinkEntities(text, _extractor.ExtractCashtagsWithIndices(text));
        }

        #endregion

        #region static methods

        private static string EscapeHTML(string text)
        {
            StringBuilder builder = new StringBuilder(text.Length * 2);
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                switch (c)
                {
                    case '&': builder.Append("&amp;"); break;
                    case '>': builder.Append("&gt;"); break;
                    case '<': builder.Append("&lt;"); break;
                    case '"': builder.Append("&quot;"); break;
                    case '\'': builder.Append("&#39;"); break;
                    default: builder.Append(c); break;
                }
            }
            return builder.ToString();
        }

        #endregion
    }
}