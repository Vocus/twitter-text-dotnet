using NUnit.Framework;
//using Rhino.Mocks;
using System;
using System.Collections.Generic;
using TRE = System.Text.RegularExpressions;
namespace Vocus.TwitterText.Tests
{
    [TestFixture]
    public class AutolinkTest
    {
        private Autolink linker;
            

        protected void AssertAutolink(String expected, String linked)
        {
            Assert.AreEqual(expected, linked, "Autolinked text should equal the input");
        }
        [SetUp]
        public void SetUp()
        {
            linker = new Autolink();
        }

        [Test, TestCaseSource("AllAutolinkingTests")]
        public void TestAllAutolinking(StringOutTestCase test)
        {
            linker.NoFollow = false;
            var actual = linker.AutoLink(test.Input);
            Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public StringOutTestCase[] AllAutolinkingTests
        {
            get
            {
                return new StringOutTestCase[] 
                    {
                        new StringOutTestCase()
                            {
                                Description = "Correctly handles URL followed directly by @user",
                                Input = "See: http://example.com/@user",
                                Expected = "See: <a href=\"http://example.com/@user\">http://example.com/@user</a>"
                            },
                        new StringOutTestCase()
                            {
                                Description = "Correctly handles URL params containing @user",
                                Input = "See: http://example.com/?@user=@user",
                                Expected = "See: <a href=\"http://example.com/?@user=@user\">http://example.com/?@user=@user</a>"
                            }
                    };
            }
        }

        [Test]
        public void TestNoFollowByDefault()
        {
            String tweet = "This has a #hashtag";
            String expected = "This has a <a href=\"https://twitter.com/#!/search?q=%23hashtag\" title=\"#hashtag\" class=\"tweet-url hashtag\" rel=\"nofollow\">#hashtag</a>";
            AssertAutolink(expected, linker.AutoLinkHashtags(tweet));
        }

        [Test]
        public void TestNoFollowDisabled()
        {
            linker.NoFollow = false;
            String tweet = "This has a #hashtag";
            String expected = "This has a <a href=\"https://twitter.com/#!/search?q=%23hashtag\" title=\"#hashtag\" class=\"tweet-url hashtag\">#hashtag</a>";
            AssertAutolink(expected, linker.AutoLinkHashtags(tweet));
        }

        /** See Also: http://github.com/mzsanford/twitter-text-rb/issues#issue/5 */
        [Test]
        public void TestBlogspotWithDash()
        {
            linker.NoFollow = false;
            String tweet = "Url: http://samsoum-us.blogspot.com/2010/05/la-censure-nuit-limage-de-notre-pays.html";
            String expected = "Url: <a href=\"http://samsoum-us.blogspot.com/2010/05/la-censure-nuit-limage-de-notre-pays.html\">http://samsoum-us.blogspot.com/2010/05/la-censure-nuit-limage-de-notre-pays.html</a>";
            AssertAutolink(expected, linker.AutoLinkURLs(tweet));
        }

        /** See also: https://github.com/mzsanford/twitter-text-java/issues/8 */
        [Test]
        public void TestURLWithDollarThatLooksLikeARegex()
        {
            linker.NoFollow = false;
            String tweet = "Url: http://example.com/$ABC";
            String expected = "Url: <a href=\"http://example.com/$ABC\">http://example.com/$ABC</a>";
            AssertAutolink(expected, linker.AutoLinkURLs(tweet));
        }

        [Test]
        public void TestURLWithoutProtocol()
        {
            linker.NoFollow = false;
            String tweet = "Url: www.twitter.com http://www.twitter.com";
            String expected = "Url: www.twitter.com <a href=\"http://www.twitter.com\">http://www.twitter.com</a>";
            AssertAutolink(expected, linker.AutoLinkURLs(tweet));
        }

        [Test]
        public void TestURLEntities()
        {
            Extractor.Entity entity = new Extractor.Entity(0, 19, "http://t.co/0JG5Mcq", Extractor.Entity.EntityTypeEnum.URL);
            entity.DisplayURL = "blog.twitter.com/2011/05/twitte…";
            entity.ExpandedURL = "http://blog.twitter.com/2011/05/twitter-for-mac-update.html";
            List<Extractor.Entity> entities = new List<Extractor.Entity>();
            entities.Add(entity);
            String tweet = "http://t.co/0JG5Mcq";
            String expected = "<a href=\"http://t.co/0JG5Mcq\" rel=\"nofollow\"><span class='tco-ellipsis'><span style='position:absolute;left:-9999px;'>&nbsp;</span></span><span style='position:absolute;left:-9999px;'>http://</span><span class='js-display-url'>blog.twitter.com/2011/05/twitte</span><span style='position:absolute;left:-9999px;'>r-for-mac-update.html</span><span class='tco-ellipsis'><span style='position:absolute;left:-9999px;'>&nbsp;</span>…</span></a>";

            AssertAutolink(expected, linker.AutoLinkEntities(tweet, entities));
        }

        [Test]
        public void TestWithAngleBrackets()
        {
            linker.NoFollow = false;
            String tweet = "(Debugging) <3 #idol2011";
            String expected = "(Debugging) &lt;3 <a href=\"https://twitter.com/#!/search?q=%23idol2011\" title=\"#idol2011\" class=\"tweet-url hashtag\">#idol2011</a>";
            AssertAutolink(expected, linker.AutoLink(tweet));

            tweet = "<link rel='true'>http://example.com</link>";
            expected = "<link rel='true'><a href=\"http://example.com\">http://example.com</a></link>";
            AssertAutolink(expected, linker.AutoLinkURLs(tweet));
        }

        [Test]
        public void TestUsernameIncludeSymbol()
        {
            linker.UsernameIncludeSymbol = true;
            String tweet = "Testing @mention and @mention/list";
            String expected = "Testing <a class=\"tweet-url username\" href=\"https://twitter.com/mention\" rel=\"nofollow\">@mention</a> and <a class=\"tweet-url list-slug\" href=\"https://twitter.com/mention/list\" rel=\"nofollow\">@mention/list</a>";
            AssertAutolink(expected, linker.AutoLink(tweet));
        }

        [Test]
        public void TestUrlClass()
        {
            linker.NoFollow = false;

            String tweet = "http://twitter.com";
            String expected = "<a href=\"http://twitter.com\">http://twitter.com</a>";
            AssertAutolink(expected, linker.AutoLink(tweet));

            linker.UrlClass = "testClass";
            expected = "<a href=\"http://twitter.com\" class=\"testClass\">http://twitter.com</a>";
            AssertAutolink(expected, linker.AutoLink(tweet));

            tweet = "#hash @tw";
            String result = linker.AutoLink(tweet);
            Assert.IsTrue(result.Contains("class=\"" + Autolink.DEFAULT_HASHTAG_CLASS + "\""));
            Assert.IsTrue(result.Contains("class=\"" + Autolink.DEFAULT_USERNAME_CLASS + "\""));
            Assert.IsFalse(result.Contains("class=\"testClass\""));
        }

        [Test]
        public void TestSymbolTag()
        {
            linker.SymbolTag = "s";
            linker.TextWithSymbolTag = "b";
            linker.NoFollow = false;

            String tweet = "#hash";
            String expected = "<a href=\"https://twitter.com/#!/search?q=%23hash\" title=\"#hash\" class=\"tweet-url hashtag\"><s>#</s><b>hash</b></a>";
            AssertAutolink(expected, linker.AutoLink(tweet));

            tweet = "@mention";
            expected = "<s>@</s><a class=\"tweet-url username\" href=\"https://twitter.com/mention\"><b>mention</b></a>";
            AssertAutolink(expected, linker.AutoLink(tweet));

            linker.UsernameIncludeSymbol = true;
            expected = "<a class=\"tweet-url username\" href=\"https://twitter.com/mention\"><s>@</s><b>mention</b></a>";
            AssertAutolink(expected, linker.AutoLink(tweet));
        }

        [Test]
        public void TestUrlTarget()
        {
            linker.UrlTarget = "_blank";

            String tweet = "http://test.com";
            String result = linker.AutoLink(tweet);

            Assert.IsFalse(TRE.Regex.IsMatch(result, ".*<a[^>]+hashtag[^>]+target[^>]+>.*"), "urlTarget shouldn't be applied to auto-linked hashtag");
            Assert.IsFalse(TRE.Regex.IsMatch(result, ".*<a[^>]+username[^>]+target[^>]+>.*"), "urlTarget shouldn't be applied to auto-linked mention");
            Assert.IsTrue(TRE.Regex.IsMatch(result, ".*<a[^>]+test.com[^>]+target=\"_blank\"[^>]*>.*"), "urlTarget should be applied to auto-linked URL");
            Assert.IsFalse(result.ToLower().Contains("urlclass"), "urlClass should not appear in HTML");
        }

        [Test]
        public void TestLinkAttributeModifier()
        {
            linker.LinkAttributeModifier = delegate(Extractor.Entity entity, Dictionary<String, String> attributes)
            {
                if (entity.EntityType == Extractor.Entity.EntityTypeEnum.HASHTAG)
                {
                    attributes.Add("dummy-hash-attr", "test");
                }
            };

            String result = linker.AutoLink("#hash @mention");
            Assert.IsTrue(TRE.Regex.IsMatch(result, ".*<a[^>]+hashtag[^>]+dummy-hash-attr=\"test\"[^>]*>.*"), "HtmlAttributeModifier should be applied to hashtag");
            Assert.IsFalse(TRE.Regex.IsMatch(result, ".*<a[^>]+username[^>]+dummy-hash-attr=\"test\"[^>]*>.*"), "HtmlAttributeModifier should not be applied to mention");

            linker.LinkAttributeModifier = delegate(Extractor.Entity entity, Dictionary<String, String> attributes)
            {
                if (entity.EntityType == Extractor.Entity.EntityTypeEnum.URL)
                {
                    attributes.Add("dummy-url-attr", entity.Value);
                }
            };

            result = linker.AutoLink("@mention http://twitter.com/");
            Assert.IsFalse(TRE.Regex.IsMatch(result, ".*<a[^>]+username[^>]+dummy-url-attr[^>]*>.*"), "HtmlAttributeModifier should not be applied to mention");
            Assert.IsTrue(TRE.Regex.IsMatch(result, ".*<a[^>]+dummy-url-attr=\"http://twitter.com/\".*"), "htmlAttributeBlock should be applied to URL");
        }

        [Test]
        public void TestLinkTextModifier()
        {
            linker.LinkTextModifier = delegate(Extractor.Entity entity, string text)
            {
                return entity.EntityType == Extractor.Entity.EntityTypeEnum.HASHTAG ? "#replaced" : "pre_" + text + "_post";
            };


            String result = linker.AutoLink("#hash @mention");
            Assert.IsTrue(TRE.Regex.IsMatch(result, ".*<a[^>]+>#replaced</a>.*"), "LinkTextModifier should modify a hashtag link text");
            Assert.IsTrue(TRE.Regex.IsMatch(result, ".*<a[^>]+>pre_mention_post</a>.*"), "LinkTextModifier should modify a username link text");

            linker.LinkTextModifier = delegate(Extractor.Entity entity, string text)
            {
                return "pre_" + text + "_post";
            };
            linker.SymbolTag = "s";
            linker.TextWithSymbolTag = "b";
            linker.UsernameIncludeSymbol = true;
            result = linker.AutoLink("#hash @mention");
            Assert.IsTrue(TRE.Regex.IsMatch(result, ".*<a[^>]+>pre_<s>#</s><b>hash</b>_post</a>.*"), "LinkTextModifier should modify a hashtag link text");
            Assert.IsTrue(TRE.Regex.IsMatch(result, ".*<a[^>]+>pre_<s>@</s><b>mention</b>_post</a>.*"), "LinkTextModifier should modify a username link text");
        }


    }
}
