using www778878net.net;

namespace Test78
{
    internal static class ProgramHelpers
    {
        private static async void Test()
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