
using NUnit.Framework;
using System;
using System.Text;
namespace Vocus.TwitterText.Tests
{
    [TestFixture]
    public class ValidatorTest
    {
        protected Validator validator = new Validator();

        [Test]
        public void TestBOMCharacter()
        {
            Assert.IsFalse(validator.IsValidTweet("test \uFFFE"));
            Assert.IsFalse(validator.IsValidTweet("test \uFEFF"));
        }

        [Test]
        public void TestInvalidCharacter()
        {
            Assert.IsFalse(validator.IsValidTweet("test \uFFFF"));
            Assert.IsFalse(validator.IsValidTweet("test \uFEFF"));
        }

        [Test]
        public void TestDirectionChangeCharacters()
        {
            Assert.IsFalse(validator.IsValidTweet("test \u202A test"));
            Assert.IsFalse(validator.IsValidTweet("test \u202B test"));
            Assert.IsFalse(validator.IsValidTweet("test \u202C test"));
            Assert.IsFalse(validator.IsValidTweet("test \u202D test"));
            Assert.IsFalse(validator.IsValidTweet("test \u202E test"));
        }

        [Test]
        public void TestAccentCharacters()
        {
            String c = "\u0065\u0301";
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 139; i++)
            {
                builder.Append(c);
            }
            Assert.IsTrue(validator.IsValidTweet(builder.ToString()), "139 accent characters should be a valid tweet.");
            Assert.IsTrue(validator.IsValidTweet(builder.Append(c).ToString()), "140 accent characters should be a valid tweet.");
            Assert.IsFalse(validator.IsValidTweet(builder.Append(c).ToString()), "141 accent characters should not be a valid tweet.");
        }

        [Test]
        public void TestMutiByteCharacters()
        {
            String c = "\ud83d\ude02";
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 139; i++)
            {
                builder.Append(c);
            }
            Assert.IsTrue(validator.IsValidTweet(builder.ToString()), "139 multibyte characters should be a valid tweet.");
            Assert.IsTrue(validator.IsValidTweet(builder.Append(c).ToString()), "140 multibyte characters should be a valid tweet.");
            Assert.IsFalse(validator.IsValidTweet(builder.Append(c).ToString()), "141 multibyte characters should not be a valid tweet.");
        }

        [Test, TestCaseSource("GetTweetLengthTests")]
        public void TestGetTweetLength(Int32OutTestCase test)
        {
            var actual = validator.GetTweetLength(test.Input);
            Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public Int32OutTestCase[] GetTweetLengthTests
        {
            get
            {
                return new Int32OutTestCase[]
                    {
                        new Int32OutTestCase()
                            {
                                Description = "Count a URL starting with http:// as 22 characters",
                                Input = "http://test.com",
                                Expected = 22
                            },
                        new Int32OutTestCase()
                            {
                                Description = "Count a URL starting with https:// as 23 characters",
                                Input = "https://test.com",
                                Expected = 23
                            },
                        new Int32OutTestCase()
                            {
                                Description = "Count unicode chars inside the basic multilingual plane",
                                Input = "저찀쯿쿿",
                                Expected = 4
                            }
                    };
            }
        }
    }
}