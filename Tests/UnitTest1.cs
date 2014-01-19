using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GoogleVoice2;
using System.IO;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void AbilityToLogin()
        {
            Task.Run(async () =>
                {
                    var acct = new Account(File.ReadAllText(@"d:\gv_u.txt"), File.ReadAllText(@"d:\gv_p.txt"));
                    await acct.Login();

                    Assert.AreEqual(1, 1);
                }).Wait();


        }
    }
}
