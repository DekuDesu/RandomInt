using System;
using System.Net.Http;
using System.Net;
using System.Linq;
using System.Threading.Tasks;

namespace RandomInt
{
    /// <summary>
    /// Generators random integers using Random.Org's online API. If the maximum numbers have been requested from Random.Org(the daily quota) then the System.Random number generator is used instead. Limited to IPv4 HTTP requests.
    /// </summary>
    public static class Generator
    {
        // For API Documentation: https://www.random.org/clients/http/

        public static HttpClient Client { get; private set; }

        public static int Quota { get; private set; } = 0;

        public const int RandomDotOrgMin = -1000000000;
        public const int RandomDotOrgMax = 1000000000;
        public const int RandomDotOrgMaxN = 10000;

        private const int RandomDotOrgCooldown = 10;
        private const int RandomDotOrgQuotaCooldown = 60000;

        private static IPAddress PublicIP;
        private static readonly Random LocalGenerator = new Random();
        private static bool Initialized = false;

        private static DateTime LastCheckedQuota = DateTime.UtcNow;
        private static DateTime LastUsedApi = DateTime.MinValue;

        private static string ErrorNotInitialized => $"Client not initialized! Use {nameof(Generator)}.{nameof(InitializeClient)}() to initialize the client before attempt to get numbers.";

        private static async Task GetIPAddresses()
        {
            var IPConfig = await Dns.GetHostAddressesAsync(Dns.GetHostName());

            PublicIP = IPConfig.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) ?? throw new Exception("Failed to get public ip from local hostname.");
        }

        private static async Task<int> GetQuota()
        {
            // Random.Org doesn't want us to check our quota faster than every 10 minutes if it was negative the last time we checked.
            if (Quota < 0)
            {
                if ((DateTime.UtcNow - LastCheckedQuota).TotalMilliseconds < RandomDotOrgQuotaCooldown)
                {
                    return Quota;
                }
            }

            string request = $@"https://www.random.org/quota/?{PublicIP}&format=plain";

            try
            {
                string content = await Client.GetStringAsync(request);

                LastCheckedQuota = DateTime.UtcNow;

                if (int.TryParse(content, out int result))
                {
                    Quota = result;

                    return result;
                }

                throw new HttpRequestException($"Failed to get parsable result: {content}");
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }

        public static async Task InitializeClient()
        {
            Client ??= new HttpClient();
            await GetIPAddresses();
            await GetQuota();
            Initialized = true;
        }

        /// <summary>
        /// Get a random number from Random.Org. If the daily quota is met it will use a local random number generator(usually <see cref="System.Random"/>)
        /// </summary>
        /// <param name="min">The smallest a returned number can be (inclusive) Range: [1-1e4]</param>
        /// <param name="max">The largest a returned number can be (inclusive) Range: [-1e9-1e9]</param>
        /// <param name="numberBase">The base of the number that should be returned, available supported bases: 2 | 8 | 10 | 16</param>
        /// <returns>
        /// <see langword="int"/> Random Number
        /// <para>
        /// throws <see cref="Exception"/> When client is not initialized
        /// </para>
        /// </returns>
        /// <exception cref="Exception"></exception> 
        public static async Task<int> Next(int min = RandomDotOrgMin, int max = RandomDotOrgMax)
        {
            if (Initialized is false)
            {
                throw new Exception(ErrorNotInitialized);
            }

            int[] results = await NextSet(min, max);

            return results?[0] ?? LocalGenerator.Next(min, max + 1);
        }

        /// <summary>
        /// Get an array of random numbers from Random.Org. If the daily quota is met it will use a local random number generator(usually <see cref="System.Random"/>)
        /// </summary>
        /// <param name="min">The smallest a returned number can be (inclusive) Range: [1-1e4]</param>
        /// <param name="max">The largest a returned number can be (inclusive) Range: [-1e9-1e9]</param>
        /// <param name="n">The maximum amount of numbers that should be returned in the array. Range: [-1e9,1e9]</param>
        /// <param name="numberBase">The base of the number that should be returned, available supported bases: 2 | 8 | 10 | 16</param>
        /// <returns>
        /// <see langword="int"/>[] Random Numbers
        /// <para>
        /// throws <see cref="Exception"/> When client is not initialized
        /// </para>
        /// </returns>
        /// <exception cref="HttpRequestException"></exception>
        /// <exception cref="Exception"></exception> 
        public static async Task<int[]> NextSet(int min = RandomDotOrgMin, int max = RandomDotOrgMax, int n = 10, int numberBase = 10)
        {
            if (Initialized is false)
            {
                throw new Exception(ErrorNotInitialized);
            }

            // make sure that we don't exceed Random.Org's requirements on API calls
            min = Math.Clamp(min, RandomDotOrgMin, RandomDotOrgMax);
            max = Math.Clamp(max, RandomDotOrgMin, RandomDotOrgMax);
            n = Math.Clamp(n, 1, RandomDotOrgMaxN);

            if (Quota <= n * 2 || (DateTime.UtcNow - LastUsedApi).TotalMilliseconds <= RandomDotOrgCooldown)
            {
                int[] nums = new int[n];
                for (int i = 0; i < n; i++)
                {
                    nums[i] = LocalGenerator.Next(min, max + 1);
                }

                return nums;
            }

            string baseRequest = @"http://www.random.org/integers/?";
            string request = $"{baseRequest}num={n}&min={min}&max={max}&col=1&base={numberBase}&format=plain";

            try
            {
                string content = await Client.GetStringAsync(request);

                LastUsedApi = DateTime.UtcNow;

                string[] splitLines = content.Split('\n');

                var query = from line in splitLines where line.Length > 0 select int.Parse(line);

                return query.ToArray();
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }
    }
}
