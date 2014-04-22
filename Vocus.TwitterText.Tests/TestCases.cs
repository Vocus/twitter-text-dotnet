using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vocus.TwitterText.Tests
{
    public class TestCase<TExpected>
    {
        public string Description;
        public string Input;
        public TExpected Expected;

        public override string ToString()
        {
            return Description;
        }
    }
    public class DynamicOutTestCase : TestCase<dynamic> { }
    public class BooleanOutTestCase : TestCase<bool> { }
    public class Int32OutTestCase : TestCase<int> { }
    public class StringOutTestCase : TestCase<String> { }
    public class StringListOutTestCase : TestCase<List<String>> { }
}
