using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PriceScrapper
{
    public interface ILocations
    {
        Int64 GetId();
        string GetName();
        string GetAreaName();
    }
    public interface IScrapper
    {
        Task<ILocations[]> GetAreaId(string query);
        List<PriceInfo> GetPriceInfos();
        Task<List<PriceInfo>> DoRequest();
    }
}
