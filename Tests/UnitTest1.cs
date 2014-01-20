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
                    try
                    {
                        var acct = new Account(File.ReadAllText(@"d:\gv_u.txt"), File.ReadAllText(@"d:\gv_p.txt"));
                        await acct.Login();
                    }
                    catch(Exception ex)
                    {
                        Assert.Fail();
                    }
                }).Wait();
        }
    }
}
