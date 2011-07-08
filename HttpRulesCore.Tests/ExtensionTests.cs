using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpRulesCore.Tests
{
    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]
        public void ExtensionMatching()
        {
            var relative = new Uri("/test/test.gif?something.asdadas", UriKind.Relative);
            var absolute = new Uri("http://test.com/test/test.gif?something.asdadas");

            Assert.AreEqual(true, relative.HasFileExtension("mpg", "gif", "jpg"));
            Assert.AreEqual(true, absolute.HasFileExtension("mpg", "gif", "jpg"));
        }
    }
}
