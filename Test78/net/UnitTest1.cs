using Microsoft.VisualStudio.TestTools.UnitTesting;
using www778878net.net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using www778878net.Net.Response;
using Newtonsoft.Json.Linq;
using System.Security.AccessControl;

namespace www778878net.net.Tests
{
    [TestClass()]
    public class UnitTest1
    {
        [TestMethod()]
        public void test()
        {
           
        
        }

        [TestMethod()]
        public void GetToHtmlDocumentTest()
        {
            //Assert.Fail();
        }

        [TestMethod()]
        public   void DownFile()
        {
            string url = "http://www.778878.net/down/test/v.config";
            string menu = "c:\\downtest\\update\\v.config";
         
            HttpClient78.Client78.DownFile(url, menu);

            Thread.Sleep(3000);
          
        }

        [TestMethod()]
        public void GetToStreamTest()
        {
           // Assert.Fail();
        }

        [TestMethod()]
        public void GetToStringTest()
        {
            Uri uri = new("http://net.778878.net/apinet/services/Services78/test");
            var tmp =  HttpClient78.Client78.GetToString(uri);
            tmp.Wait();
            string? getback = tmp.Result!.Content;
         
            Assert.AreEqual(getback, "\"apinetabc中文def14\"");
        }

     
    }
}