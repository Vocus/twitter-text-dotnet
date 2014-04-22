using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.RepresentationModel;
using System.Dynamic;
using System.Linq;

namespace Vocus.TwitterText.Tests
{
    [TestFixture]
    public class ConformanceTest
    {
        private static readonly String CONFORMANCE_DIR_RELATIVE = @"\..\..\..\test-data\twitter-text-conformance\";
        protected static readonly String KEY_DESCRIPTION = "description";
        protected static readonly String KEY_INPUT = "text";
        protected static readonly String KEY_EXPECTED_OUTPUT = "expected";
        protected static readonly String KEY_HIGHLIGHT_HITS = "hits";
        protected string conformanceDir;
        protected Extractor extractor = new Extractor();
        protected Autolink linker = new Autolink();
        protected Validator validator = new Validator();
        protected HitHighlighter hitHighlighter = new HitHighlighter();

        public ConformanceTest()
        {
            conformanceDir = Directory.GetCurrentDirectory() + CONFORMANCE_DIR_RELATIVE;
            Assert.IsTrue(Directory.Exists(conformanceDir), "Conformance directory " + conformanceDir + " is not a directory.");

            Assert.NotNull(extractor, "No extractor configured.");
            Assert.NotNull(linker, "No linker configured.");
            linker.NoFollow = false;
        }

        [Test, TestCaseSource("MentionsExtractorTests")]
        public void TestMentionsExtractor(DynamicOutTestCase test) {
            var actual = extractor.ExtractMentionedScreennames(test.Input);
            Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public List<DynamicOutTestCase> MentionsExtractorTests
        {
            get
            {
                return DynamicOutTestCases("extract.yml", "mentions");
            }
        }

        [Test, TestCaseSource("ReplyExtractorTests")]
        public void TestReplyExtractor(StringOutTestCase test) {
                var actual = extractor.ExtractReplyScreenname(test.Input);
                Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public List<StringOutTestCase> ReplyExtractorTests
        {
            get
            {
                return StringOutTestCases("extract.yml", "replies");
            }
        }


        [Test, TestCaseSource("HashtagsExtractorTests")]
        public void TestHashtagsExtractor(DynamicOutTestCase test) {
                var actual = extractor.ExtractHashtags(test.Input);
                Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public List<DynamicOutTestCase> HashtagsExtractorTests
        {
            get
            {
                return DynamicOutTestCases("extract.yml", "hashtags");
            }
        }

        [Test, TestCaseSource("HashtagsWithIndicesExtractorTests")]
        public void TestHashtagsWithIndicesExtractor(DynamicOutTestCase test)
        {
            var expected = new List<Extractor.Entity>();

            foreach (dynamic e in test.Expected)
            {
                expected.Add(new Extractor.Entity(Convert.ToInt32(e.indices[0]), Convert.ToInt32(e.indices[1]), e.hashtag.ToString(), Extractor.Entity.EntityTypeEnum.HASHTAG));
            }

            var actual = extractor.ExtractHashtagsWithIndices(test.Input);
            Assert.AreEqual(expected, actual, test.Description);
        }
        public List<DynamicOutTestCase> HashtagsWithIndicesExtractorTests
        {
            get
            {
                return DynamicOutTestCases("extract.yml", "hashtags_with_indices");
            }
        }

        [Test, TestCaseSource("URLsExtractorTests")]
        public void TestURLsExtractor(DynamicOutTestCase test)
        {
            var actual = extractor.ExtractURLs(test.Input);
            Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public List<DynamicOutTestCase> URLsExtractorTests
        {
            get
            {
                return DynamicOutTestCases("extract.yml", "urls");
            }
        }

        [Test, TestCaseSource("CashtagsExtractorTests")]
        public void TestCashtagsExtractor(DynamicOutTestCase test)
        {
            var actual = extractor.ExtractCashtags(test.Input);
            CollectionAssert.AreEquivalent(test.Expected, actual);
            //Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public List<DynamicOutTestCase> CashtagsExtractorTests
        {
            get
            {
                return DynamicOutTestCases("extract.yml", "cashtags");
            }
        }

        [Test, TestCaseSource("CashtagsWithIndicesExtractorTests")]
        public void TestCashtagsWithIndicesExtractor(DynamicOutTestCase test)
        {
            var expected = new List<Extractor.Entity>();

            foreach (dynamic e in test.Expected)
            {
                expected.Add(new Extractor.Entity(Convert.ToInt32(e.indices[0]), Convert.ToInt32(e.indices[1]), e.cashtag.ToString(), Extractor.Entity.EntityTypeEnum.CASHTAG));
            }

            var actual = extractor.ExtractCashtagsWithIndices(test.Input);
            Assert.AreEqual(expected, actual, test.Description);
        }
        public List<DynamicOutTestCase> CashtagsWithIndicesExtractorTests
        {
            get
            {
                return DynamicOutTestCases("extract.yml", "cashtags_with_indices");

            }
        }

        [Test, TestCaseSource("UsernameAutlinkingTests")]
        public void TestUsernameAutolinking(StringOutTestCase test)
        {
            var actual = linker.AutoLinkUsernamesAndLists(test.Input);
            Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public List<StringOutTestCase> UsernameAutlinkingTests
        {
            get
            {
                return StringOutTestCases("autolink.yml", "usernames");
            }
        }

        [Test, TestCaseSource("ListAutolinkingTests")]
        public void TestListAutolinking(StringOutTestCase test)
        {
            var actual = linker.AutoLinkUsernamesAndLists(test.Input);
            Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public List<StringOutTestCase> ListAutolinkingTests
        {
            get
            {
                return StringOutTestCases("autolink.yml", "lists");
            }
        }

        [Test, TestCaseSource("HashtagAutolinkingTests")]
        public void TestHashtagAutolinking(StringOutTestCase test)
        {
            var actual = linker.AutoLinkHashtags(test.Input);
            Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public List<StringOutTestCase> HashtagAutolinkingTests
        {
            get
            {
                return StringOutTestCases("autolink.yml", "hashtags");
            }
        }

        [Test, TestCaseSource("URLAutoLinkingTests")]
        public void TestURLAutolinking(StringOutTestCase test)
        {
            var actual = linker.AutoLinkURLs(test.Input);
            Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public List<StringOutTestCase> URLAutoLinkingTests
        {
            get
            {
                return StringOutTestCases("autolink.yml", "urls");
            }
        }

        [Test, TestCaseSource("CashtagAutolinkingTests")]
        public void TestCashtagAutolinking(StringOutTestCase test)
        {
            var actual = linker.AutoLinkCashtags(test.Input);
            Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public List<StringOutTestCase> CashtagAutolinkingTests
        {
            get
            {
                return StringOutTestCases("autolink.yml", "cashtags");
            }
        }

        [Test, TestCaseSource("AllAutolinkingTests")]
        public void TestAllAutolinking(StringOutTestCase test) {
            var actual = linker.AutoLink(test.Input);
            Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public List<StringOutTestCase> AllAutolinkingTests
        {
            get
            {
                return StringOutTestCases("autolink.yml", "all");
            }
        }

        [Test, TestCaseSource("IsValidTweetTests")]
        public void TestIsValidTweet(BooleanOutTestCase test)
        {
            var actual = validator.IsValidTweet(test.Input);
            Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public List<BooleanOutTestCase> IsValidTweetTests
        {
            get
            {
                return BooleanOutTestCases("validate.yml", "tweets");
            }
        }

        [Test]
        public void TestDoubleWordUnicodeYamlRetrieval()
        {
            var yamlFile = "validate.yml";
            Assert.IsTrue(File.Exists(conformanceDir + yamlFile), "Yaml file " + conformanceDir + yamlFile + " does not exist.");

            var stream = new StreamReader(Path.Combine(conformanceDir, yamlFile));
            var yaml = new YamlStream();
            yaml.Load(stream);

            var root = yaml.Documents[0].RootNode as YamlMappingNode;
            var testNode = new YamlScalarNode("tests");
            Assert.IsTrue(root.Children.ContainsKey(testNode), "Document is missing test node.");
            var tests = root.Children[testNode] as YamlMappingNode;
            Assert.IsNotNull(tests, "Test node is not YamlMappingNode");

            var typeNode = new YamlScalarNode("lengths");
            Assert.IsTrue(tests.Children.ContainsKey(typeNode), "Test type lengths not found in tests.");
            var typeTests = tests.Children[typeNode] as YamlSequenceNode;
            Assert.IsNotNull(typeTests, "lengths tests are not YamlSequenceNode");

            var list = new List<dynamic>();
            var count = 0;
            foreach (YamlMappingNode item in typeTests)
            {
                var text = ConvertNode<string>(item.Children.Single(x => x.Key.ToString() == "text").Value) as string;
                var description = ConvertNode<string>(item.Children.Single(x => x.Key.ToString() == "description").Value) as string;
                Assert.DoesNotThrow(() => {text.Normalize(NormalizationForm.FormC);}, String.Format("Yaml couldn't parse a double word unicode string at test {0} - {1}.", count, description));
                count++;
            }
        }

        [Test, TestCaseSource("GetTweetLengthTests")]
        public void TestGetTweetLength(Int32OutTestCase test)
        {
            var actual = validator.GetTweetLength(test.Input);
            Assert.AreEqual(test.Expected, actual, test.Description);
        }
        public List<Int32OutTestCase> GetTweetLengthTests
        {
            get
            {
                return Int32OutTestCases("validate.yml", "lengths");
            }
        }

        [Test, TestCaseSource("PlainTextHitHighlightingTests")]
        public void TestPlainTextHitHighlighting(HitHighlightingTestCase test) {
            var actual = hitHighlighter.Highlight(test.Input, test.Hits);
            Assert.AreEqual(test.Expected, actual, test.Description);
        }
        #region data
        public List<HitHighlightingTestCase> PlainTextHitHighlightingTests
        {
            get
            {
                return HitHighlightingTestCases("hit_highlighting.yml", "plain_text");
            }
        }
        public class HitHighlightingTestCase : TestCase<String>
        {
            public List<List<int>> Hits;
        }
        protected List<HitHighlightingTestCase> HitHighlightingTestCases(string file, string testType)
        {
            var dynamicTests = LoadConformanceData<String>(file, testType);
            var testCases = new List<HitHighlightingTestCase>(dynamicTests.Count);
            foreach (var dynamicTest in dynamicTests)
            {
                var description = dynamicTest.description as string;
                var input = dynamicTest.text as string;
                var expected = dynamicTest.expected as string;
                var hits = dynamicTest.hits as List<List<int>>;

                testCases.Add(new HitHighlightingTestCase()
                    {
                        Description = description,
                        Input = input,
                        Expected = expected,
                        Hits = hits
                    }
                );
            }
            return testCases;
        }
        #endregion

        #region yaml test case loading
        //yaml parsing logic from https://github.com/Mindroute/twitter-text-cs/blob/master/Tests/TwitterTextTests.cs
        protected List<dynamic> LoadConformanceData<TExpected>(String yamlFile, String testType) {
            Assert.IsTrue(File.Exists(conformanceDir + yamlFile), "Yaml file " + conformanceDir + yamlFile + " does not exist.");

            var stream = new StreamReader(Path.Combine(conformanceDir, yamlFile));
            var yaml = new YamlStream();
            yaml.Load(stream);

            var root = yaml.Documents[0].RootNode as YamlMappingNode;
            var testNode = new YamlScalarNode("tests");
            Assert.IsTrue(root.Children.ContainsKey(testNode), "Document is missing test node.");
            var tests = root.Children[testNode] as YamlMappingNode;
            Assert.IsNotNull(tests, "Test node is not YamlMappingNode");

            var typeNode = new YamlScalarNode(testType);
            Assert.IsTrue(tests.Children.ContainsKey(typeNode), "Test type " + testType + " not found in tests.");
            var typeTests = tests.Children[typeNode] as YamlSequenceNode;
            Assert.IsNotNull(typeTests, testType + " tests are not YamlSequenceNode");

            var list = new List<dynamic>();
            foreach (YamlMappingNode item in typeTests) {
                dynamic test = new ExpandoObject();
                test.description = ConvertNode<string>(item.Children.Single(x => x.Key.ToString() == "description").Value);
                test.text = ConvertNode<string>(item.Children.Single(x => x.Key.ToString() == "text").Value);
                test.expected = ConvertNode<TExpected>(item.Children.Single(x => x.Key.ToString() == "expected").Value);
                test.hits = ConvertNode<List<List<int>>>(item.Children.SingleOrDefault(x => x.Key.ToString() == "hits").Value);
                list.Add(test);
            }

            return list;
        }

        private dynamic ConvertNode<T>(YamlNode node)
        {
            dynamic dynnode = node as dynamic;

            if (node is YamlScalarNode)
            {
                if (string.IsNullOrEmpty(dynnode.Value))
                {
                    return null;
                }
                else if (typeof(T) == typeof(int))
                {
                    return int.Parse(dynnode.Value);
                }
                else if (typeof(T) == typeof(bool))
                {
                    return dynnode.Value == "true";
                }
                else
                {
                    return dynnode.Value;
                }
            }
            else if (node is YamlSequenceNode)
            {
                dynamic list;
                if (typeof(T) == typeof(List<List<int>>))
                {
                    list = new List<List<int>>();
                    foreach (var item in dynnode.Children)
                    {
                        list.Add(ConvertNode<List<int>>(item));
                    }
                }
                else if (typeof(T) == typeof(List<int>))
                {
                    list = new List<int>();
                    foreach (var item in dynnode.Children)
                    {
                        list.Add(ConvertNode<int>(item));
                    }
                }
                else
                {
                    list = new List<dynamic>();
                    foreach (var item in dynnode.Children)
                    {
                        list.Add(ConvertNode<T>(item));
                    }
                }
                return list;
            }
            else if (node is YamlMappingNode)
            {
                dynamic mapnode = new ExpandoObject();
                foreach (var item in ((YamlMappingNode)node).Children)
                {
                    var key = item.Key.ToString();
                    if (key == "indices")
                    {
                        ((IDictionary<string, object>)mapnode).Add(key, ConvertNode<int>(item.Value));
                    }
                    else
                    {
                        ((IDictionary<string, object>)mapnode).Add(key, ConvertNode<T>(item.Value));
                    }
                }
                return mapnode;
            }
            return null;
        }

        #endregion

        #region yaml test case conversion
        protected List<DynamicOutTestCase> DynamicOutTestCases(string file, string testType)
        {
            var dynamicTests = LoadConformanceData<dynamic>(file, testType);

            var testCases = new List<DynamicOutTestCase>(dynamicTests.Count);
            foreach (var dynamicTest in dynamicTests)
            {
                var description = dynamicTest.description as string;
                var input = dynamicTest.text as string;
                var expected = dynamicTest.expected as dynamic;

                testCases.Add(new DynamicOutTestCase()
                    {
                        Description = description,
                        Input = input,
                        Expected = expected
                    }
                );
            }
            return testCases;
        }

        protected List<BooleanOutTestCase> BooleanOutTestCases(string file, string testType)
        {
            var dynamicTests = LoadConformanceData<bool>(file, testType);

            var testCases = new List<BooleanOutTestCase>(dynamicTests.Count);
            foreach (var dynamicTest in dynamicTests)
            {
                var description = dynamicTest.description as string;
                var input = dynamicTest.text as string;
                var expected = Convert.ToBoolean(dynamicTest.expected);

                testCases.Add(new BooleanOutTestCase()
                    {
                        Description = description,
                        Input = input,
                        Expected = expected
                    }
                );
            }
            return testCases;
        }

        protected List<Int32OutTestCase> Int32OutTestCases(string file, string testType)
        {
            var dynamicTests = LoadConformanceData<int>(file, testType);
            
            var testCases = new List<Int32OutTestCase>(dynamicTests.Count);
            foreach (var dynamicTest in dynamicTests)
            {
                var description = dynamicTest.description as string;
                var input = dynamicTest.text as string;
                var expected = Convert.ToInt32(dynamicTest.expected);

                testCases.Add(new Int32OutTestCase()
                    {
                        Description = description,
                        Input = input,
                        Expected = expected
                    }
                );

            }
            return testCases;
        }

        protected List<StringOutTestCase> StringOutTestCases(string file, string testType)
        {
            var dynamicTests = LoadConformanceData<String>(file, testType);
            var testCases = new List<StringOutTestCase>(dynamicTests.Count);
            foreach(var dynamicTest in dynamicTests)
            {
                var description = dynamicTest.description as string;
                var input = dynamicTest.text as string;
                var expected = dynamicTest.expected as string;

                testCases.Add(new StringOutTestCase()
                    {
                        Description = description,
                        Input = input,
                        Expected = expected
                    }
                );
            }
            return testCases;
        }

        #endregion
    }
}