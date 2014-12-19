using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DNSRootServerResolver.Tests
{
    [TestClass]
    public class ExpirableUnitTest
    {
        [TestMethod]
        public void NonExpired()
        {
            var nonExpired = new Expirable<int>(() => 10, i => DateTime.Now.AddSeconds(i));
            Assert.IsFalse(nonExpired.Expired);
        }

        [TestMethod]
        public void Expired()
        {
            var expired = new Expirable<int>(() => 10, i => DateTime.Now.AddSeconds(-i));
            Assert.IsTrue(expired.Expired);
        }

        [TestMethod]
        public void ExpiredWithCounter()
        {
            int counter = 10;
            Expirable<int> expiredNewValue = new Expirable<int>(() => counter++, i => DateTime.Now.AddSeconds(-i));

            Assert.AreEqual(10, expiredNewValue.Value);
            Assert.AreEqual(11, expiredNewValue.Value);
            Assert.AreEqual(12, expiredNewValue.Value);
        }

        [TestMethod]
        public void ExpiredThenLaterNotExpired()
        {
            bool value = true;
            var expirable = new Expirable<int>(() => 10, i => 
            {
                if(value) //expired first time
                {
                    value = false;

                    return DateTime.Now.AddSeconds(-10);
                }
                else //non-expired second time
                {
                    return DateTime.Now.AddSeconds(10);
                }
            });

            Assert.IsTrue(expirable.Expired);
            var trash = expirable.Value;
            Assert.IsFalse(expirable.Expired);
        }
    }
}
