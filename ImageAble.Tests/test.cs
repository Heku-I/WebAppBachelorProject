using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageAble.Tests
{
    [TestFixture]
    internal class test
    {

        [Test]
        public void TestAssertion()
        {
            Assert.IsNotNull(null, "This should fail because the value is null.");
            Assert.AreEqual(1, 1, "This should pass because 1 equals 1.");
        }

    }
}
