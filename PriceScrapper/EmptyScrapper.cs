using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PriceScrapper
{
    public class EmptyLocations : ILocations
    {
        public string GetAreaName()
        {
            return "";
        }

        public long GetId()
        {
            return -1;
        }

        public string GetName()
        {
            return "";
        }
    }
    public class EmptyScrapper : IScrapper
    {
        public async Task<List<PriceInfo>> DoRequest()
        {
            return new List<PriceInfo>();
        }

        public async Task<ILocations[]> GetAreaId(string query)
        {
            return new List<EmptyLocations>().ToArray();
        }

        public List<PriceInfo> GetPriceInfos()
        {
            return new List<PriceInfo>();
        }
    }
}
