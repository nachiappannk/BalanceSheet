using Nachiappan.BalanceSheetViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nachiappan.BalanceSheetViewModel.Model;
using NUnit.Framework;

namespace Nachiappan.BalanceSheetViewModel.Tests
{
    [TestFixture]
    public class LedgerClassiferTests
    {
        [Test]
        public void IsNominalLedger()
        {
            var result = LedgerClassifer.IsNominalLedger("ssssss/ddd");
            Assert.AreEqual(true, result);
        }


        [Test]
        public void IsDoubleNominalLedger()
        {
            var result = LedgerClassifer.IsDoubleNominalLedger("ssssss/ddd/sssdfs");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void IsRealLedger()
        {
            var result = LedgerClassifer.IsRealLedger("ssssss");
            Assert.AreEqual(true, result);
        }

        [Test]
        public void NominalTests()
        {
            var nominalPart = LedgerClassifer.GetNominalPartOfName("s1234/d1234");
            Assert.AreEqual("d1234", nominalPart);
        }


        [Test]
        public void DoubleNominalTests()
        {
            var nominalPart = LedgerClassifer.GetNominalPartOfName("s1234/d1234/f1234");
            Assert.AreEqual("f1234", nominalPart);
        }



        [Test]
        public void NominalTestsRealPart()
        {
            var nominalPart = LedgerClassifer.GetBasePartOfName("s1234/d1234");
            Assert.AreEqual("s1234", nominalPart);
        }


        [Test]
        public void DoubleNominalTestsRealPart()
        {
            var nominalPart = LedgerClassifer.GetBasePartOfName("s1234/d1234/f1234");
            Assert.AreEqual("s1234/d1234", nominalPart);
        }
    }
}