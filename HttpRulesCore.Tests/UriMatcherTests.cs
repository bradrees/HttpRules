using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpRulesCore.Tests
{
    [TestClass]
    public class UriMatcherTests
    {
        [TestMethod]
        public void TestUriParsing()
        {
            var uriTest = new Uri("http://www.test.com");
            var uriExample = new Uri("https://www.example.com/sample.html");
            Assert.AreEqual(UriPatternMatchType.NonMatch, new UriPattern(".ad.").Match(uriTest.CreateSimpleRequest()));
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern(".test.").Match(uriTest.CreateSimpleRequest()));
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern(".example.*^sample.html").Match(uriExample.CreateSimpleRequest()));
            Assert.AreEqual(UriPatternMatchType.NonMatch, new UriPattern(".example.*^/sample.html").Match(uriExample.CreateSimpleRequest()));
        }

        [TestMethod]
        public void DocumentedExamples()
        {
            var uriTest = new Uri("http://example.com/ads/banner123.gif");
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern("http://example.com/ads/banner*.gif").Match(uriTest.CreateSimpleRequest()));
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern("http://example.com/ads/*.").Match(uriTest.CreateSimpleRequest()));
            Assert.AreEqual(UriPatternMatchType.Exception, new UriPattern("@@|http://example.com").Match(uriTest.CreateSimpleRequest()));

            uriTest = new Uri("http://example.com/swf/index.html");
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern("swf").Match(uriTest.CreateSimpleRequest()));
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern("*swf*").Match(uriTest.CreateSimpleRequest()));

            uriTest = new Uri("http://example.com/annoyingflash.swf");
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern("swf|").Match(uriTest.CreateSimpleRequest()));
            uriTest = new Uri("http://example.com/swf/index.html");
            Assert.AreEqual(UriPatternMatchType.NonMatch, new UriPattern("swf|").Match(uriTest.CreateSimpleRequest()));

            uriTest = new Uri("http://baddomain.example/banner.gif");
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern("|http://baddomain.example/").Match(uriTest.CreateSimpleRequest()));
            uriTest = new Uri("http://gooddomain.example/analyze?http://baddomain.example");
            Assert.AreEqual(UriPatternMatchType.NonMatch, new UriPattern("|http://baddomain.example/").Match(uriTest.CreateSimpleRequest()));

            uriTest = new Uri("http://example.com/banner.gif");
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern("||example.com/banner.gif").Match(uriTest.CreateSimpleRequest()));
            uriTest = new Uri("https://example.com/banner.gif");
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern("||example.com/banner.gif").Match(uriTest.CreateSimpleRequest()));
            uriTest = new Uri("http://www.example.com/banner.gif");
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern("||example.com/banner.gif").Match(uriTest.CreateSimpleRequest()));

            uriTest = new Uri("http://badexample.com/banner.gif");
            Assert.AreEqual(UriPatternMatchType.NonMatch, new UriPattern("||example.com/banner.gif").Match(uriTest.CreateSimpleRequest()));
            uriTest = new Uri("http://gooddomain.example/analyze?http://example.com/banner.gif");
            Assert.AreEqual(UriPatternMatchType.NonMatch, new UriPattern("||example.com/banner.gif").Match(uriTest.CreateSimpleRequest()));

            uriTest = new Uri("http://example.com/");
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern("http://example.com^").Match(uriTest.CreateSimpleRequest()));
            uriTest = new Uri("http://example.com:8000/");
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern("http://example.com^").Match(uriTest.CreateSimpleRequest()));
            uriTest = new Uri("http://example.com.ar/");
            Assert.AreEqual(UriPatternMatchType.NonMatch, new UriPattern("http://example.com^").Match(uriTest.CreateSimpleRequest()));

            uriTest = new Uri(@"http://example.com:8000/foo.bar?a=12&b=%D1%82%D0%B5%D1%81%D1%82");
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern("^example.com^").Match(uriTest.CreateSimpleRequest()));
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern(@"^%D1%82%D0%B5%D1%81%D1%82^").Match(uriTest.CreateSimpleRequest()));
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern("^foo.bar^").Match(uriTest.CreateSimpleRequest()));

            uriTest = new Uri(@"http://example.com:8000/banner123");
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern(@"/banner\d+/").Match(uriTest.CreateSimpleRequest()));

            uriTest = new Uri(@"http://example.com:8000/banners");
            Assert.AreEqual(UriPatternMatchType.NonMatch, new UriPattern(@"/banner\d+/").Match(uriTest.CreateSimpleRequest()));
        }

        [TestMethod]
        public void UriExtensions()
        {
            var uriTest = new Uri("http://www.example.com/ads/banner123.gif");
            Assert.IsTrue(uriTest.IsDomainOrSubdomain("example.com"));

            uriTest = new Uri("http://sub.domain.example.com/ads/banner123.gif");
            Assert.IsTrue(uriTest.IsDomainOrSubdomain("example.com"));
            Assert.IsTrue(uriTest.IsDomainOrSubdomain("domain.example.com"));

            uriTest = new Uri("http://example.com/ads/banner123.gif");
            Assert.IsTrue(uriTest.IsDomainOrSubdomain("example.com"));
            Assert.IsFalse(uriTest.IsDomainOrSubdomain("www.example.com"));
            Assert.IsFalse(uriTest.IsDomainOrSubdomain("example.co.uk"));
            Assert.IsFalse(uriTest.IsDomainOrSubdomain("other.com"));
            Assert.IsFalse(uriTest.IsDomainOrSubdomain(""));
            Assert.IsFalse(uriTest.IsDomainOrSubdomain("com"));
        }

        [TestMethod]
        public void OptionsTests()
        {
            var uriTest = new Uri("http://www.example.com/ads/bannerAd.gif");
            Assert.AreEqual(UriPatternMatchType.NonMatch, new UriPattern("*/BannerAd.gif$match-case").Match(uriTest.CreateSimpleRequest()));

            uriTest = new Uri("http://www.example.com/ads/BannerAd.gif");
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern("*/BannerAd.gif$match-case").Match(uriTest.CreateSimpleRequest()));

            uriTest = new Uri("http://www.example.com/ads/BannerAd.js");
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern("*/BannerAd$script").Match(uriTest.CreateSimpleRequest()));

            uriTest = new Uri("http://ad.example.com/ads/BannerAd.gif");
            Assert.AreEqual(UriPatternMatchType.Match, new UriPattern("|http://ad.$~object_subrequest,domain=~europa.eu|~sjsu.edu|~uitm.edu.my|~uni-freiburg.de").Match(uriTest.CreateSimpleRequest()));

            uriTest = new Uri("http://ad.uitm.edu.my/ads/BannerAd.gif");
            Assert.AreEqual(UriPatternMatchType.NonMatch, new UriPattern("|http://ad.$~object_subrequest,domain=~europa.eu|~sjsu.edu|~uitm.edu.my|~uni-freiburg.de").Match(uriTest.CreateSimpleRequest()));
        }

        [TestMethod]
        public void RegressionTests()
        {
            var uriTest = new Uri("http://www.example.com/ads/bannerAd.gif");
            Assert.AreEqual(UriPatternMatchType.NonMatch, new UriPattern("/bannerAd*.aspx").Match(uriTest.CreateSimpleRequest()));
        }
    }
}
