using System;
using System.Collections.Generic;
using System.Text;
namespace Vocus.TwitterText
{
    /// <summary>
    /// A class for adding HTML highlighting in Tweet text (such as would be returned from a Search)
    /// </summary>
    public class HitHighlighter
    {
        /** Default HTML tag for highlight hits */
        public static readonly String DEFAULT_HIGHLIGHT_TAG = "em";

        /** the current HTML tag used for hit highlighting */
        protected String _highlightTag;

        /** Create a new HitHighlighter object. */
        public HitHighlighter()
        {
            _highlightTag = DEFAULT_HIGHLIGHT_TAG;
        }

        /// <summary>
        /// Surround the <code>hits</code> in the provided <code>text</code> with an HTML tag. This is used with offsets
        /// from the search API to support the highlighting of query terms.
        /// </summary>
        /// <param name="text">text of the Tweet to highlight</param>
        /// <param name="hits">A List of highlighting offsets (themselves lists of two elements)</param>
        /// <returns>text with highlight HTML added</returns>
        public String Highlight(String text, List<List<int>> hits)
        {
            if (hits == null || hits.Count == 0)
            {
                return (text);
            }

            StringBuilder sb = new StringBuilder(text.Length);
            CharEnumerator iterator = text.GetEnumerator();
            bool isCounting = true;
            bool tagOpened = false;
            int currentIndex = 0;
            bool iteratorContinue = iterator.MoveNext();
            char currentChar = iterator.Current;

            while (iteratorContinue)
            {
                // TODO: this is slow.
                foreach (List<int> start_end in hits)
                {
                    if (start_end[0] == currentIndex)
                    {
                        sb.Append(Tag(false));
                        tagOpened = true;
                    }
                    else if (start_end[1] == currentIndex)
                    {
                        sb.Append(Tag(true));
                        tagOpened = false;
                    }
                }

                if (currentChar == '<')
                {
                    isCounting = false;
                }
                else if (currentChar == '>' && !isCounting)
                {
                    isCounting = true;
                }

                if (isCounting)
                {
                    currentIndex++;
                }
                sb.Append(currentChar);
                if (iteratorContinue = iterator.MoveNext())
                {
                    currentChar = iterator.Current;
                }
            }

            if (tagOpened)
            {
                sb.Append(Tag(true));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Format the current <code>highlightTag</code> by adding &lt; and >. If <code>closeTag</code> is <code>true</code>
        /// then the tag returned will include a <code>/</code> to signify a closing tag.
        /// </summary>
        /// <param name="closeTag">true if this is a closing tag, false otherwise</param>
        /// <returns></returns>
        protected String Tag(bool closeTag)
        {
            StringBuilder sb = new StringBuilder(_highlightTag.Length + 3);
            sb.Append("<");
            if (closeTag)
            {
                sb.Append("/");
            }
            sb.Append(_highlightTag).Append(">");
            return sb.ToString();
        }

        /// <summary>
        /// HTML tagname used for phrase highlighting
        /// </summary>
        public String HighlightTag
        {
            get { return _highlightTag; }
            set { _highlightTag = value; }
        }
    }
}