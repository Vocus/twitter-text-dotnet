
using NUnit.Framework;
using TRE = System.Text.RegularExpressions;
using System.Text.RegularExpressions;
using System;
namespace Vocus.TwitterText.Tests
{
    [TestFixture]
    public class RegexTest
    {
        [Test]
        public void TestAutoLinkHashtags()
        {
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#hashtag");
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#Azərbaycanca");
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#mûǁae");
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#Čeština");
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#Ċaoiṁín");
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#Caoiṁín");
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#ta\u0301im");
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#hag\u0303ua");
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#caf\u00E9");
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#\u05e2\u05d1\u05e8\u05d9\u05ea"); // "#Hebrew"
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#\u05d0\u05b2\u05e9\u05b6\u05c1\u05e8"); // with marks
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#\u05e2\u05b7\u05dc\u05be\u05d9\u05b0\u05d3\u05b5\u05d9"); // with maqaf 05be
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#\u05d5\u05db\u05d5\u05f3"); // with geresh 05f3
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#\u05de\u05f4\u05db"); // with gershayim 05f4
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#\u0627\u0644\u0639\u0631\u0628\u064a\u0629"); // "#Arabic"
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#\u062d\u0627\u0644\u064a\u0627\u064b"); // with mark
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#\u064a\u0640\ufbb1\u0640\u064e\u0671"); // with pres. form
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#ประเทศไทย");
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#ฟรี"); // with mark
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "#日本語ハッシュタグ");
            AssertCaptureCount(3, Regex.VALID_HASHTAG, "＃日本語ハッシュタグ");

            Assert.IsTrue(Regex.VALID_HASHTAG.IsMatch("これはOK #ハッシュタグ"));
            Assert.IsTrue(Regex.VALID_HASHTAG.IsMatch("これもOK。#ハッシュタグ"));
            Assert.IsFalse(Regex.VALID_HASHTAG.IsMatch("これはダメ#ハッシュタグ"));

            Assert.IsFalse(Regex.VALID_HASHTAG.IsMatch("#1"));
            Assert.IsFalse(Regex.VALID_HASHTAG.IsMatch("#0"));
        }

        [Test]
        public void TestAutoLinkUsernamesOrLists()
        {
            AssertCaptureCount(4, Regex.VALID_MENTION_OR_LIST, "@username");
            AssertCaptureCount(4, Regex.VALID_MENTION_OR_LIST, "@username/list");
        }

        [Test]
        public void TestValidURL()
        {
            AssertCaptureCount(8, Regex.VALID_URL, "http://example.com");
        }

        [Test]
        public void TestValidURLDoesNotCrashOnLongPaths()
        {
            String longPathIsLong = "Check out http://example.com/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            Assert.IsTrue(Regex.VALID_URL.IsMatch(longPathIsLong), "Failed to correctly match a very long path");
        }

        [Test]
        public void TestValidUrlDoesNotTakeForeverOnRepeatedPuctuationAtEnd()
        {
            String[] repeatedPaths = new String[] {
                    "Try http://example.com/path**********************",
                    "http://foo.org/bar/foo-bar-foo-bar.aspx!!!!!! Test"
                };

            foreach (String text in repeatedPaths)
            {
                DateTime start = DateTime.Now;
                bool isValid = Regex.VALID_URL.IsMatch(text);
                Regex.VALID_URL.Matches(text);
                DateTime end = DateTime.Now;

                Assert.IsTrue(isValid, "Should be able to extract a valid URL even followed by punctuations in " + text);

                double duration = (end - start).TotalMilliseconds;
                Assert.IsTrue((duration < 10), "Matching a repeated path end should take less than 10ms (took " + duration + "ms)");
            }
        }

        [Test, TestCaseSource("ValidURLWithoutProtocolTests")]
        public void TestValidURLWithoutProtocol(BooleanOutTestCase test)
        {
            var actual = Regex.VALID_URL.IsMatch(test.Input);
            Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public BooleanOutTestCase[] ValidURLWithoutProtocolTests
        {
            get
            {
                return new BooleanOutTestCase[]
                    {
                        new BooleanOutTestCase()
                            {
                                Expected = true,
                                Input = "twitter.com", 
                                Description = "Matching a URL with gTLD without protocol."
                            },
                        new BooleanOutTestCase()
                            {
                                Expected = true,
                                Input = "www.foo.co.jp", 
                                Description = "Matching a URL with ccTLD without protocol."
                            },
                        new BooleanOutTestCase()
                            {
                                Expected = true,
                                Input = "www.foo.org.za", 
                                Description = "Matching a URL with gTLD followed by ccTLD without protocol."
                            },
                        new BooleanOutTestCase()
                            {
                                Expected = true,
                                Input = "http://t.co", 
                                Description = "Should match a short URL with ccTLD with protocol."
                            },
                        new BooleanOutTestCase()
                            {
                                Expected = false,
                                Input = "t.co", 
                                Description = "Should not match a short URL with ccTLD without protocol."
                            },
                        new BooleanOutTestCase()
                            {
                                Expected = false,
                                Input = "www.foo.baz", 
                                Description = "Should not match a URL with invalid gTLD."
                            },
                        new BooleanOutTestCase()
                            {
                                Expected = true,
                                Input = "t.co/blahblah", 
                                Description = "Match a short URL with ccTLD and '/' but without protocol."
                            }
                    };
            }
        }

        [Test]
        public void TestInvalidUrlWithInvalidCharacter()
        {
            char[] invalid_chars = new char[] { '\u202A', '\u202B', '\u202C', '\u202D', '\u202E' };
            foreach (char c in invalid_chars)
            {
                Assert.IsFalse(Regex.VALID_URL.IsMatch("http://twitt" + c + "er.com"), "Should not extract URLs with invalid character");
            }
        }

        [Test]
        public void TestExtractMentions()
        {
            AssertCaptureCount(4, Regex.VALID_MENTION_OR_LIST, "sample @user mention");
        }

        [Test]
        public void TestInvalidMentions()
        {
            char[] invalid_chars = new char[] { '!', '@', '#', '$', '%', '&', '*' };
            foreach (char c in invalid_chars)
            {
                Assert.IsFalse(Regex.VALID_MENTION_OR_LIST.IsMatch("f" + c + "@kn"), "Failed to ignore a mention preceded by " + c);
            }
        }

        [Test]
        public void TestExtractReply()
        {
            AssertCaptureCount(1, Regex.VALID_REPLY, "@user reply");
            AssertCaptureCount(1, Regex.VALID_REPLY, " @user reply");
            AssertCaptureCount(1, Regex.VALID_REPLY, "\u3000@user reply");
        }

        private void AssertCaptureCount(int expectedCount, TRE.Regex pattern, String sample)
        {
            Assert.IsTrue(pattern.IsMatch(sample), "Pattern failed to match sample: '" + sample + "'");
            //-1 because groups[0] is the full match
            var groupCount = pattern.Match(sample).Groups.Count - 1;
            Assert.AreEqual(expectedCount, groupCount, "Does not have " + expectedCount + " captures as expected: '" + sample + "'");
        }
    }
}
