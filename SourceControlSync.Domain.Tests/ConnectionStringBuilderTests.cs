using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;

namespace SourceControlSync.Domain.Tests
{
    [TestClass]
    public class ConnectionStringBuilderTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            var csb = new TestConnectionStringBuilder();

            Assert.IsNull(csb.ConnectionString);
        }

        [TestMethod]
        public void EmptyConnectionString()
        {
            var csb = new TestConnectionStringBuilder()
            {
                ConnectionString = string.Empty
            };

            Assert.IsNull(csb.ConnectionString);
        }

        [TestMethod]
        public void OneKeyValuePairInConstructor()
        {
            var csb = new TestConnectionStringBuilder("key=value");

            Assert.AreEqual("key=value", csb.ConnectionString);
        }

        [TestMethod]
        public void OneKeyValuePairInProperty()
        {
            var csb = new TestConnectionStringBuilder()
            {
                ConnectionString = "key=value"
            };

            Assert.AreEqual("key=value", csb.ConnectionString);
        }

        [TestMethod]
        public void TwoKeyValuePairs()
        {
            var csb = new TestConnectionStringBuilder("key1=val1;key2=val2");

            var pairs = csb.ConnectionString.Split(';');
            Assert.AreEqual(2, pairs.Count());
            Assert.IsTrue(pairs.Contains("key1=val1"));
            Assert.IsTrue(pairs.Contains("key2=val2"));
        }

        [TestMethod]
        public void TwoKeyValuePairsGetValue()
        {
            var csb = new TestConnectionStringBuilder("key1=val1;key2=val2");

            Assert.AreEqual("val1", csb.TestGetValue("key1"));
            Assert.AreEqual("val2", csb.TestGetValue("key2"));
        }

        [TestMethod]
        public void TwoKeyValuePairsSetValue()
        {
            var csb = new TestConnectionStringBuilder();
            csb.TestSetValue("key1", "val1");
            csb.TestSetValue("key2", "val2");

            var pairs = csb.ConnectionString.Split(';');
            Assert.AreEqual(2, pairs.Count());
            Assert.IsTrue(pairs.Contains("key1=val1"));
            Assert.IsTrue(pairs.Contains("key2=val2"));
        }

        [TestMethod]
        public void TwoKeyValuePairsSetValueExisting()
        {
            var csb = new TestConnectionStringBuilder("key1=val1;key2=val2");
            csb.TestSetValue("key2", "val2222");

            var pairs = csb.ConnectionString.Split(';');
            Assert.AreEqual(2, pairs.Count());
            Assert.IsTrue(pairs.Contains("key1=val1"));
            Assert.IsTrue(pairs.Contains("key2=val2222"));
        }

        [TestMethod]
        public void TwoKeyValuePairsGetValueMissing()
        {
            var csb = new TestConnectionStringBuilder("key1=val1;key2=val2");

            Assert.IsNull(csb.TestGetValue("key3"));
        }

        [TestMethod]
        public void TwoKeyValuePairsSetValueNull()
        {
            var csb = new TestConnectionStringBuilder();
            csb.TestSetValue("key1", null);

            Assert.AreEqual("key1=", csb.ConnectionString);
        }

        private class TestConnectionStringBuilder : ConnectionStringBuilder
        {
            public TestConnectionStringBuilder(string connectionString)
            {
                ConnectionString = connectionString;
            }

            public TestConnectionStringBuilder()
            {
            }

            public string TestGetValue(string key)
            {
                return base.GetValue(key);
            }

            public void TestSetValue(string key, string value)
            {
                base.SetValue(key, value);
            }
        }
    }
}
