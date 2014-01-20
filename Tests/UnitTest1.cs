using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GoogleVoice2;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

                        //File.WriteAllText(@"d:\gv_info.txt", JsonConvert.SerializeObject(acct._Info));
                    }
                    catch(Exception)
                    {
                        Assert.Fail();
                    }
                }).Wait();
        }


        [TestMethod]
        public void AccountFromStoredInfo()
        {
            Task.Run(async () => {

                var acct = new Account(File.ReadAllText(@"d:\gv_u.txt"), File.ReadAllText(@"d:\gv_p.txt"));
                await acct.Login();

                await acct.PostLoginActivities();

            }).Wait();
        }
    }
}
