using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
    public class BooliLocations
    {
        public BooliArea area;
        public string label;
        public string type;
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    public class BooliPricer
    {
        private const string BOOLI_PREFIX = "https://www.booli.se/slutpriser/";
        private const string BOOLI_AREA_ID_PREFIX = "https://www.booli.se/search/suggestions?q=";

        private List<string> m_locationIds;
        private List<string> m_itemTypes;
        private string m_timeSpan;

        public BooliPricer(List<string> locationId, List<string> itemTypes, string timeSpan)
        {
            m_locationIds = locationId;
            m_itemTypes = itemTypes;
            m_timeSpan = timeSpan;
        }

        public static async Task<BooliLocations[]> GetAreaId(string query)
        {
            query = HttpUtility.UrlEncode(query);
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

        public async Task DoRequest()
        {
            if (m_timeSpan.EndsWith("m"))
            {
                return;
            }

            int month = 0;
            int.TryParse(m_timeSpan.Substring(0, m_timeSpan.IndexOf('m')), out month);

            if (month == 0)
            {
                throw new Exception("Wrong time, it should be <number>m, e.g. 36m");
            }

            string locationIdsQuery = "/" + String.Join(",", m_locationIds.ToArray()) +"/";
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
                                    
                                }
                            }
                        }

                    }
                }
            }
        }
    }
}