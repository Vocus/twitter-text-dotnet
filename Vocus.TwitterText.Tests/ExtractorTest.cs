using NUnit.Framework;
//using Rhino.Mocks;
using System;
using System.Collections.Generic;
using TRE = System.Text.RegularExpressions;
using System.Text.RegularExpressions;

namespace Vocus.TwitterText.Tests
{
    [TestFixture]
    public class ExtractorTest
    {
        protected Extractor extractor;

        [SetUp]
        public void Setup()
        {
            extractor = new Extractor();
        }

        [Test, TestCaseSource("ConvertIndiciesTests")]
        public void TestConvertIndicies(ConvertIndiciesTest test)
        {
            var entities = new List<Extractor.Entity>();
            for (var j = 0; j < test.Indexes.Count; j++)
            {
                entities.Add(new Extractor.Entity(test.Indexes[j].Start, test.Indexes[j].End, "", Extractor.Entity.EntityTypeEnum.HASHTAG));
            }
            extractor.ModifyIndicesFromUTF16ToUnicode(test.Text, entities);
            for (var j = 0; j < entities.Count; j++)
            {
                Assert.AreEqual(test.UnicodeIndexes[j].Start, entities[j].Start, "Convert UTF16 indices to Unicode indices for text '" + test.Text + "' (Start)");
                Assert.AreEqual(test.UnicodeIndexes[j].End, entities[j].End, "Convert UTF16 indices to Unicode indices for text '" + test.Text + "' (End)");
            }
            extractor.ModifyIndicesFromUnicodeToUTF16(test.Text, entities);
            for (var j = 0; j < entities.Count; j++)
            {
                Assert.AreEqual(test.Indexes[j].Start, entities[j].Start, "Convert UTF16 indices to Unicode indices for text '" + test.Text + "' (Start)");
                Assert.AreEqual(test.Indexes[j].End, entities[j].End, "Convert UTF16 indices to Unicode indices for text '" + test.Text + "' (End)");
            }
        }
        #region data
        public class ConvertIndiciesTest {
            public class Index
            {
                public Index(int start, int end)
                {
                    Start = start;
                    End = end;
                }
                public int Start;
                public int End;
            }
            public String Text;
            public List<Index> Indexes;
            public List<Index> UnicodeIndexes;

            public override string ToString()
            {
                return Text;
            }
        }
        public ConvertIndiciesTest[] ConvertIndiciesTests = new ConvertIndiciesTest[]
            {
                new ConvertIndiciesTest() 
                    {
                        Text = "abc", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(0, 3)
                            }, 
                        UnicodeIndexes =  new List<ConvertIndiciesTest.Index>() 
                            { 
                                new ConvertIndiciesTest.Index(0, 3)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "\uD83D\uDE02abc", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(2, 5)
                            }, 
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            { 
                                new ConvertIndiciesTest.Index(1, 4)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "\uD83D\uDE02abc\uD83D\uDE02", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(2, 5)
                            }, 
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            { 
                                new ConvertIndiciesTest.Index(1, 4)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "\uD83D\uDE02abc\uD838\uDE02abc", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(2, 5), 
                                new ConvertIndiciesTest.Index(7, 10)
                            },
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            { 
                                new ConvertIndiciesTest.Index(1, 4), 
                                new ConvertIndiciesTest.Index(5, 8)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "\uD83D\uDE02abc\uD838\uDE02abc\uD83D\uDE02", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(2, 5), 
                                new ConvertIndiciesTest.Index(7, 10)
                            },
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            { 
                                new ConvertIndiciesTest.Index(1, 4), 
                                new ConvertIndiciesTest.Index(5, 8)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "\uD83D\uDE02\uD83D\uDE02abc", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(4, 7)
                            },
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            { 
                                new ConvertIndiciesTest.Index(2, 5)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02abc", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(6, 9)
                            },
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            { 
                                new ConvertIndiciesTest.Index(3, 6)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02abc\uD83D\uDE02", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(6, 9)
                            },
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(3, 6)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02abc\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(6, 9)
                            },
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            { 
                                new ConvertIndiciesTest.Index(3, 6)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02abc\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02abc\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(8, 11), 
                                new ConvertIndiciesTest.Index(19, 22)
                            },
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(4, 7),
                                new ConvertIndiciesTest.Index(11, 14)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "\uDE02\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02abc\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02abc\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(7, 10), 
                                new ConvertIndiciesTest.Index(18, 21)
                            },
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            { 
                                new ConvertIndiciesTest.Index(4, 7),
                                new ConvertIndiciesTest.Index(11, 14)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02abc\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02abc\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02\uDE02", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(8, 11),
                                new ConvertIndiciesTest.Index(19, 22)
                            },
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(4, 7),
                                new ConvertIndiciesTest.Index(11, 14)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02abc\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02\uD83D\uDE02abc\uD83D\uDE02\uD83D\uDE02\uD83D\uD83D\uDE02\uDE02", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(8, 11),
                                new ConvertIndiciesTest.Index(19, 22)
                            },
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(4, 7),
                                new ConvertIndiciesTest.Index(11, 14)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "\uD83Dabc\uD83D", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(1, 4)
                            },
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(1, 4)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "\uDE02abc\uDE02", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(1, 4)
                            },
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(1, 4)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "\uDE02\uDE02abc\uDE02\uDE02", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(2, 5)
                            },
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(2, 5)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "abcabc", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(0, 3), 
                                new ConvertIndiciesTest.Index(3, 6)
                            },
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(0, 3), 
                                new ConvertIndiciesTest.Index(3, 6)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "abc\uD83D\uDE02abc", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(0, 3), 
                                new ConvertIndiciesTest.Index(5, 8)
                            },
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            { 
                                new ConvertIndiciesTest.Index(0, 3), 
                                new ConvertIndiciesTest.Index(4, 7)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "aa", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(0, 1), 
                                new ConvertIndiciesTest.Index(1, 2)
                            },
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            { 
                                new ConvertIndiciesTest.Index(0, 1), 
                                new ConvertIndiciesTest.Index(1, 2)
                            }
                    },
                new ConvertIndiciesTest() 
                    {
                        Text = "\uD83D\uDE02a\uD83D\uDE02a\uD83D\uDE02", 
                        Indexes = new List<ConvertIndiciesTest.Index>() 
                            {
                                new ConvertIndiciesTest.Index(2, 3), 
                                new ConvertIndiciesTest.Index(5, 6)
                            },
                        UnicodeIndexes = new List<ConvertIndiciesTest.Index>() 
                            { 
                                new ConvertIndiciesTest.Index(1, 2), 
                                new ConvertIndiciesTest.Index(3, 4)
                            }
                    }
            };
        #endregion

        [Test, TestCaseSource("ExtractReplyScreennameTests")]
        public void TestExtractReplyScreenname(StringOutTestCase test) 
        {
            String actual = extractor.ExtractReplyScreenname(test.Input);
            Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public StringOutTestCase[] ExtractReplyScreennameTests
        {
            get
            {
                return new StringOutTestCase[]
                    {
                        new StringOutTestCase() 
                            {
                                Description = "Reply at the start",
                                Input = "@user reply",
                                Expected = "user"
                            },
                        new StringOutTestCase() 
                            {
                                Description = "Reply with leading space",
                                Input = " @user reply",
                                Expected = "user"
                            }
                    };
            }
        }

        [Test, TestCaseSource("ExtractMentionedScreenNamesTests")]
        public void TestExtractMentionedScreennames(StringListOutTestCase test)
        {
            var actual = extractor.ExtractMentionedScreennames(test.Input);
            Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public StringListOutTestCase[] ExtractMentionedScreenNamesTests
        {
            get
            {
                return new StringListOutTestCase[]
                    {
                        new StringListOutTestCase()
                            {
                                Input = "@user mention",
                                Description = "Mention at the beginning", 
                                Expected = new List<string> { "user" }
                            },
                        new StringListOutTestCase()
                            {
                                Input = " @user mention",
                                Description = "Mention with leading space",
                                Expected = new List<string> { "user" }
                            },
                        new StringListOutTestCase()
                            {
                                Input = "mention @user here",
                                Description = "Mention in mid text", 
                                Expected = new List<string> { "user" }
                            },
                        new StringListOutTestCase()
                            {
                                Input = "mention @user1 here and @user2 here",
                                Description = "Multiple mentioned users", 
                                Expected = new List<string> { "user1", "user2" }
                            }
                    };
            }
        }
        
        [Test]
        public void TestMentionWithIndices()
        {
            List<Extractor.Entity> extracted = extractor.ExtractMentionedScreennamesWithIndices(" @user1 mention @user2 here @user3 ");
            Assert.AreEqual(3, extracted.Count);
            Assert.AreEqual(1, extracted[0].Start);
            Assert.AreEqual(7, extracted[0].End);
            Assert.AreEqual(16, extracted[1].Start);
            Assert.AreEqual(22, extracted[1].End);
            Assert.AreEqual(28, extracted[2].Start);
            Assert.AreEqual(34, extracted[2].End);
        }

        [Test]
        public void TestMentionWithSupplementaryCharacters()
        {
            // insert U+10400 before " @mention"
            String text = String.Format("{0} @mention {0} @mention", "\uD801\uDC00");

            // count U+10400 as 2 characters (as in UTF-16)
            List<Extractor.Entity> extracted = extractor.ExtractMentionedScreennamesWithIndices(text);
            Assert.AreEqual(2, extracted.Count);
            Assert.AreEqual("mention", extracted[0].Value);
            Assert.AreEqual(3, extracted[0].Start);
            Assert.AreEqual(11, extracted[0].End);
            Assert.AreEqual("mention", extracted[1].Value);
            Assert.AreEqual(15, extracted[1].Start);
            Assert.AreEqual(23, extracted[1].End);

            // count U+10400 as single character
            extractor.ModifyIndicesFromUTF16ToUnicode(text, extracted);
            Assert.AreEqual(2, extracted.Count);
            Assert.AreEqual(2, extracted[0].Start);
            Assert.AreEqual(10, extracted[0].End);
            Assert.AreEqual(13, extracted[1].Start);
            Assert.AreEqual(21, extracted[1].End);

            // count U+10400 as 2 characters (as in UTF-16)
            extractor.ModifyIndicesFromUnicodeToUTF16(text, extracted);
            Assert.AreEqual(2, extracted.Count);
            Assert.AreEqual(3, extracted[0].Start);
            Assert.AreEqual(11, extracted[0].End);
            Assert.AreEqual(15, extracted[1].Start);
            Assert.AreEqual(23, extracted[1].End);
        }

        [Test, TestCaseSource("ExtractHashtagsTests")]
        public void TestExtractHashtags(StringListOutTestCase test)
        {
            var actual = extractor.ExtractHashtags(test.Input);
            Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public StringListOutTestCase[] ExtractHashtagsTests
        {
            get
            {
                return new StringListOutTestCase[] 
                    {
                        new StringListOutTestCase()
                            {
                                Input = "#hashtag mention",
                                Description = "Hashtag at the beginning", 
                                Expected = new List<String> { "hashtag" }
                            },
                        new StringListOutTestCase()
                            {
                                Input = " #hashtag mention",
                                Description = "Hashtag with leading space", 
                                Expected = new List<String> { "hashtag" }
                            },
                        new StringListOutTestCase()
                            {
                                Input = "mention #hashtag here",
                                Description = "Hashtag in mid text", 
                                Expected = new List<String> { "hashtag" }
                            },
                        new StringListOutTestCase()
                            {
                                Input = "text #hashtag1 #hashtag2",
                                Description = "Multiple hashtags", 
                                Expected = new List<String> { "hashtag1", "hashtag2" }
                            }
                    };
            }
        }

        [Test]
        public void TestHashtagAtBeninningIndices()
        {
            List<Extractor.Entity> extracted = extractor.ExtractHashtagsWithIndices("#hashtag mention");
            Assert.AreEqual(1, extracted.Count, "Should have found one entity");
            Assert.AreEqual(0, extracted[0].Start, "Should start at 0");
            Assert.AreEqual(8, extracted[0].End, "Should end at 8");
        }

        [Test]
        public void TestHashtagWithIndices()
        {
            List<Extractor.Entity> extracted = extractor.ExtractHashtagsWithIndices(" #user1 mention #user2 here #user3 ");
            Assert.AreEqual(3, extracted.Count);
            Assert.AreEqual(1, extracted[0].Start);
            Assert.AreEqual(7, extracted[0].End);
            Assert.AreEqual(16, extracted[1].Start);
            Assert.AreEqual(22, extracted[1].End);
            Assert.AreEqual(28, extracted[2].Start);
            Assert.AreEqual(34, extracted[2].End);
        }

        [Test]
        public void TestHashtagWithSupplementaryCharacters()
        {
            // insert U+10400 before " #hashtag"
            String text = String.Format("{0} #hashtag {0} #hashtag", "\uD801\uDC00");

            // count U+10400 as 2 characters (as in UTF-16)
            List<Extractor.Entity> extracted = extractor.ExtractHashtagsWithIndices(text);
            Assert.AreEqual(2, extracted.Count);
            Assert.AreEqual("hashtag", extracted[0].Value);
            Assert.AreEqual(3, extracted[0].Start);
            Assert.AreEqual(11, extracted[0].End);
            Assert.AreEqual("hashtag", extracted[1].Value);
            Assert.AreEqual(15, extracted[1].Start);
            Assert.AreEqual(23, extracted[1].End);

            // count U+10400 as single character
            extractor.ModifyIndicesFromUTF16ToUnicode(text, extracted);
            Assert.AreEqual(2, extracted.Count);
            Assert.AreEqual(2, extracted[0].Start);
            Assert.AreEqual(10, extracted[0].End);
            Assert.AreEqual(13, extracted[1].Start);
            Assert.AreEqual(21, extracted[1].End);

            // count U+10400 as 2 characters (as in UTF-16)
            extractor.ModifyIndicesFromUnicodeToUTF16(text, extracted);
            Assert.AreEqual(2, extracted.Count);
            Assert.AreEqual(3, extracted[0].Start);
            Assert.AreEqual(11, extracted[0].End);
            Assert.AreEqual(15, extracted[1].Start);
            Assert.AreEqual(23, extracted[1].End);
        }

        [Test]
        public void TestUrlWithIndices()
        {
            List<Extractor.Entity> extracted = extractor.ExtractURLsWithIndices("http://t.co url https://www.twitter.com ");
            Assert.AreEqual(0, extracted[0].Start);
            Assert.AreEqual(11, extracted[0].End);
            Assert.AreEqual(16, extracted[1].Start);
            Assert.AreEqual(39, extracted[1].End);
        }

        [Test]
        public void TestUrlWithoutProtocol()
        {
            String text = "www.twitter.com, www.yahoo.co.jp, t.co/blahblah, www.poloshirts.uk.com";
            AssertList("Failed to extract URLs without protocol",
                new String[] { "www.twitter.com", "www.yahoo.co.jp", "t.co/blahblah", "www.poloshirts.uk.com" },
                extractor.ExtractURLs(text));

            List<Extractor.Entity> extracted = extractor.ExtractURLsWithIndices(text);
            Assert.AreEqual(0, extracted[0].Start);
            Assert.AreEqual(15, extracted[0].End);
            Assert.AreEqual(17, extracted[1].Start);
            Assert.AreEqual(32, extracted[1].End);
            Assert.AreEqual(34, extracted[2].Start);
            Assert.AreEqual(47, extracted[2].End);

            extractor.ExtractURLWithoutProtocol = false;
            Assert.AreEqual(extractor.ExtractURLs(text).Count, 0, "Should not extract URLs w/o protocol");
        }

        [Test]
        public void TestURLFollowedByPunctuations()
        {
            String text = "http://games.aarp.org/games/mahjongg-dimensions.aspx!!!!!!";
            AssertList("Failed to extract URLs followed by punctuations",
                new String[] { "http://games.aarp.org/games/mahjongg-dimensions.aspx" },
                extractor.ExtractURLs(text));
        }

        [Test, TestCaseSource("UrlWithPunctuationTests")]
        public void TestUrlWithPunctuation(string url)
        {
            Assert.AreEqual(url, extractor.ExtractURLs(url)[0]);
        }
        public String[] UrlWithPunctuationTests
        {
            get
            {
                return new String[] 
                    {
                       "http://www.foo.com/foo/path-with-period./",
                       "http://www.foo.org.za/foo/bar/688.1",
                       "http://www.foo.com/bar-path/some.stm?param1=foo;param2=P1|0||P2|0",
                       "http://foo.com/bar/123/foo_&_bar/",
                       "http://foo.com/bar(test)bar(test)bar(test)",
                       "www.foo.com/foo/path-with-period./",
                       "www.foo.org.za/foo/bar/688.1",
                       "www.foo.com/bar-path/some.stm?param1=foo;param2=P1|0||P2|0",
                       "foo.com/bar/123/foo_&_bar/"
                   };
            }
        }

        [Test]
        public void TestUrlWithSupplementaryCharacters()
        {
            // insert U+10400 before " http://twitter.com"
            String text = String.Format("{0} http://twitter.com {0} http://twitter.com", "\uD801\uDC00");

            // count U+10400 as 2 characters (as in UTF-16)
            List<Extractor.Entity> extracted = extractor.ExtractURLsWithIndices(text);
            Assert.AreEqual(2, extracted.Count);
            Assert.AreEqual("http://twitter.com", extracted[0].Value);
            Assert.AreEqual(3, extracted[0].Start);
            Assert.AreEqual(21, extracted[0].End);
            Assert.AreEqual("http://twitter.com", extracted[1].Value);
            Assert.AreEqual(25, extracted[1].Start);
            Assert.AreEqual(43, extracted[1].End);

            // count U+10400 as single character
            extractor.ModifyIndicesFromUTF16ToUnicode(text, extracted);
            Assert.AreEqual(2, extracted.Count);
            Assert.AreEqual(2, extracted[0].Start);
            Assert.AreEqual(20, extracted[0].End);
            Assert.AreEqual(23, extracted[1].Start);
            Assert.AreEqual(41, extracted[1].End);

            // count U+10400 as 2 characters (as in UTF-16)
            extractor.ModifyIndicesFromUnicodeToUTF16(text, extracted);
            Assert.AreEqual(2, extracted.Count);
            Assert.AreEqual(3, extracted[0].Start);
            Assert.AreEqual(21, extracted[0].End);
            Assert.AreEqual(25, extracted[1].Start);
            Assert.AreEqual(43, extracted[1].End);
        }


        /**
         * Helper method for asserting that the List of extracted Strings match the expected values.
         *
         * @param message to display on failure
         * @param expected Array of Strings that were expected to be extracted
         * @param actual List of Strings that were extracted
         */
        protected void AssertList(String message, String[] expected, List<String> actual)
        {
            List<String> expectedList = new List<string>(expected);
            if (expectedList.Count != actual.Count)
            {
                Assert.Fail(message + "\n\nExpected list and extracted list are differnt sizes:\n" +
                "  Expected (" + expectedList.Count + "): " + String.Join(", ", expectedList.ToArray()) + "\n" +
                "  Actual   (" + actual.Count + "): " + String.Join(", ", actual.ToArray()));
            }
            else
            {
                for (int i = 0; i < expectedList.Count; i++)
                {
                    Assert.AreEqual(expectedList[i], actual[i]);
                }
            }
        }
    }
}