using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SourceControlSync.DataAWS.Tests
{
    [TestClass]
    public class AWSS3ConnectionStringBuilderTests
    {
        [TestMethod]
        public void PathWithNameAndSeparator()
        {
            var csb = new AWSS3ConnectionStringBuilder("Path=subfolder/;BucketName=bucket;RegionSystemName=us-east-1;AccessKeyId=username;SecretAccessKey=password");
            
            Assert.AreEqual("subfolder/", csb.Path);
        }

        [TestMethod]
        public void PathWithNameAndNoSeparator()
        {
            var csb = new AWSS3ConnectionStringBuilder("Path=subfolder;BucketName=bucket;RegionSystemName=us-east-1;AccessKeyId=username;SecretAccessKey=password");
            
            Assert.AreEqual("subfolder/", csb.Path);
        }

        [TestMethod]
        public void NoPath()
        {
            var csb = new AWSS3ConnectionStringBuilder("BucketName=bucket;RegionSystemName=us-east-1;AccessKeyId=username;SecretAccessKey=password");
            
            Assert.AreEqual("", csb.Path);
        }

        [TestMethod]
        public void PathWithoutValue()
        {
            var csb = new AWSS3ConnectionStringBuilder("Path=;BucketName=bucket;RegionSystemName=us-east-1;AccessKeyId=username;SecretAccessKey=password");
            
            Assert.AreEqual("", csb.Path);
        }
    }
}
