using System.Collections.Generic;
using System.Threading.Tasks;

namespace PriceScrapper
{
    public static class ScrapperFactory
    {
        public static IScrapper GetScrapper(string provider, List<string> locationIds, List<string> itemTypes, string timespan)
        {
            switch(provider.ToLower())
            {
                case "hemnet":
                    {
                        return new HemnetPricer(locationIds, itemTypes, timespan);
                    }
                case "booli":
                    {
                        return new BooliPricer(locationIds, itemTypes, timespan);
                    }
            }

            return new EmptyScrapper();
        }

        public static async Task<ILocations[]> GetLocations(string provider, string query)
        {
            switch(provider.ToLower())
            {
                case "hemnet":
                    {
                        return await HemnetPricer.GetAreaIdStatic(query);
                    }
                case "booli":
                    {
                        return await BooliPricer.GetAreaIdStatic(query);
                    }
            }

            return new List<EmptyLocations>().ToArray();
        }
    }
}
