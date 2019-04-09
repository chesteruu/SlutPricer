using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PriceScrapper
{
    public class BooliArea
    {
        public string name;
        public Int64 id;
        public string type;
        public string url;
    }

    public class BooliLocations : ILocations
    {
        public BooliArea area;
        public string label;
        public string type;

        public string GetAreaName()
        {
            return label;
        }

        public long GetId()
        {
            return area.id;
        }

        public string GetName()
        {
            return area.name;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    public class BooliPricer : IScrapper
    {
        private const string BOOLI_PREFIX = "https://www.booli.se/slutpriser/";
        private const string BOOLI_AREA_ID_PREFIX = "https://www.booli.se/search/suggestions?q=";

        private List<string> m_locationIds;
        private List<string> m_itemTypes;
        private string m_timeSpan;

        private List<PriceInfo> m_returnedPriceInfo = new List<PriceInfo>();

        public BooliPricer(List<string> locationId, List<string> itemTypes, string timeSpan)
        {
            m_locationIds = locationId;
            m_itemTypes = itemTypes;
            m_timeSpan = timeSpan;
        }

        public async Task<ILocations[]> GetAreaId(string query)
        {
            return await BooliPricer.GetAreaIdStatic(query);
        }

        public static async Task<ILocations[]> GetAreaIdStatic(string query)
        {
            query = HttpUtility.UrlEncode(query);
            query = query.Replace("+", "%20");
            HttpClient client = new HttpClient();
            string url = BOOLI_AREA_ID_PREFIX + query;
            using (var response = await client.GetAsync(url))
            {
                using (var content = response.Content)
                {
                    var result = await content.ReadAsStringAsync();
                    BooliLocations[] booliLocations = JsonConvert.DeserializeObject<BooliLocations[]>(result);
                    return booliLocations;
                }
            }
        }

        public async Task<List<PriceInfo>> DoRequest()
        {
            m_returnedPriceInfo.Clear();
            if (!m_timeSpan.EndsWith("m"))
            {
                return m_returnedPriceInfo;
            }

            int.TryParse(m_timeSpan.Substring(0, m_timeSpan.IndexOf('m')), out int month);

            if (month == 0)
            {
                throw new Exception("Wrong time, it should be <number>m, e.g. 36m");
            }

            string locationIdsQuery = "/" + String.Join(",", m_locationIds.ToArray()) + "/?";
            string itemTypesQuery = "&objectType=" + String.Join(",", m_itemTypes.ToArray());
            string minSoldDate = "&minSoldDate=" + DateTime.Now.AddMonths(-month).ToShortDateString();
            string maxSoldDate = "&maxSoldDate=" + DateTime.Now.ToShortDateString();
            string pageQuery = "&page=";
            int pageNum = 1;

            string fullQuery = BOOLI_PREFIX + locationIdsQuery + itemTypesQuery + minSoldDate + maxSoldDate;
            HttpClient client = new HttpClient();

            while (true)
            {
                string url = fullQuery + pageQuery + pageNum;
                using (var response = await client.GetAsync(url))
                {
                    using (var content = response.Content)
                    {
                        // read answer in non-blocking way
                        var result = await content.ReadAsStringAsync();
                        var document = new HtmlDocument();
                        document.LoadHtml(result);
                        var resultNodes = document.DocumentNode.SelectNodes("//li[@class='search-list__item']/a");

                        if (resultNodes == null)
                        {
                            break;
                        }

                        pageNum++;

                        foreach (var node in resultNodes)
                        {
                            PriceInfo priceInfo = new PriceInfo();
                            foreach (var property in node.ChildNodes)
                            {
                                if (!property.HasAttributes)
                                {
                                    continue;
                                }

                                switch (property.Attributes["class"].Value)
                                {
                                    case "search-list__column search-list__column--price-change":
                                        {
                                            var sizeNode = property.SelectSingleNode("./text()[last()]")?.InnerText.Trim().Replace("%", "");
                                            double.TryParse(sizeNode, out priceInfo.PriceChange);
                                            break;
                                        }
                                    case "search-list__column search-list__column--info-1":
                                        {
                                            priceInfo.Address = property.SelectSingleNode("./span[@class='search-list__row search-list__row--address']")?.InnerText.Trim();

                                            var sizeNode = property.SelectSingleNode("./span[@class='search-list__row'][1]")?.InnerText.Trim();
                                            if (sizeNode.Contains(","))
                                            {
                                                var rowStrings = sizeNode?.Split(" ");
                                                double.TryParse(rowStrings[0], out priceInfo.Rooms);
                                                if (priceInfo.Rooms == 0)
                                                {
                                                    break;
                                                }
                                                if (rowStrings.Count() > 3)
                                                {
                                                    priceInfo.LivingSize = double.Parse(rowStrings?[2]);
                                                    if (rowStrings.Count() == 6)
                                                    {
                                                        priceInfo.BiSize = double.Parse(rowStrings?[4]);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var rowStrings = sizeNode?.Split(" ");
                                                priceInfo.Rooms = 0;
                                                if(rowStrings.Count() >= 2)
                                                {
                                                    priceInfo.LivingSize = double.Parse(rowStrings?[0]);
                                                    if(sizeNode.Contains("+"))
                                                    {
                                                        priceInfo.BiSize = double.Parse(rowStrings?[3]);
                                                    }
                                                }
                                            }

                                            var areaNode = property.SelectSingleNode("./span[@class='search-list__row'][2]")?.InnerText.Trim();
                                            var rowSplitStrings = areaNode?.Split(",");
                                            priceInfo.Type = rowSplitStrings?[0].Trim();
                                            priceInfo.Area = rowSplitStrings?[1].Trim();

                                            break;
                                        }
                                    case "search-list__column search-list__column--info-2":
                                        {
                                            var price = property.SelectSingleNode("./span[@class='search-list__row search-list__row--price']")?.InnerText.Trim().Replace(" ", "").Replace("kr", "");
                                            double.TryParse(price, out priceInfo.Price);

                                            var yard = property.SelectSingleNode("./span[@class='search-list__row']")?.InnerText.Trim();
                                            var yardStrings = yard?.Split(" ");
                                            if (yardStrings.Count() >= 2)
                                            {
                                                DateTime.TryParse(yardStrings?[1].Trim(), out priceInfo.SoldTime);
                                            }

                                            var date = property.SelectSingleNode("./span[@class='search-list__row search-list__row--sold-date']")?.InnerText.Trim();
                                            DateTime.TryParse(date, out priceInfo.SoldTime);
                                            break;
                                        }
                                }
                            }
                            m_returnedPriceInfo.Add(priceInfo);
                        }
                    }
                }

                // If too quick, it may reject call
                Thread.Sleep(50);
            }

            return m_returnedPriceInfo;
        }

        public List<PriceInfo> GetPriceInfos()
        {
            return m_returnedPriceInfo;
        }
    }
}