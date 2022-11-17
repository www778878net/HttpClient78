using www778878net.net;

namespace Test78
{
    internal static class ProgramHelpers
    {
        private static async void Test()
        {
            string url = "http://www.778878.net/down/test/v.config";
            string menu = "c:\\downtest\\update\\v.config";
            await HttpClient78.Client78.DownFile(url, menu);
            //Uri uri = new("http://net.778878.net/apinet/services/Services78/test");
            //var tmp = await HttpClient78.Client78.GetToString(uri);
            //string? getback = tmp!.Content;
        }

        private static async void Test2()
        {
            Uri uri = new("http://net.778878.net/apinet/services/Services78/test");
            var tmp = await HttpClient78.Client78.GetToString(uri);
            string? getback = tmp!.Content;
        }
        public static void Main(string[] args)
        {
            Test();
            Thread.Sleep(9999);
        }
    }
}