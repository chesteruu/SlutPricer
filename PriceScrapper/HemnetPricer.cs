﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;

namespace PriceScrapper
{
    public class HemnetPricer
    {
        private const string HEMNET_PREFIX = "https://www.hemnet.se/salda/bostader?";
        private List<string> m_locationIds;
        private List<string> m_itemTypes;
        private string m_timeSpan;
        private List<PriceInfo> m_returnedPriceInfo = new List<PriceInfo>();

        public HemnetPricer(List<string> locationId, List<string> itemTypes, string timeSpan)
        {
            m_locationIds = locationId;
            m_itemTypes = itemTypes;
            m_timeSpan = timeSpan;
        }

        public List<PriceInfo> GetPriceInfoList()
        {
            return m_returnedPriceInfo;
        }

        public async Task DoRequest()
        {
            string locationIdsQuery = String.Join("", m_locationIds.Select(location => "&location_ids[]=" + location).ToArray());
            string itemTypesQuery = String.Join("", m_itemTypes.Select(item => "&item_types[]=" + item).ToArray());
            string soldAgesQuery = "&sold_age=" + m_timeSpan;
            string pageQuery = "&page=";

            string fullQuery = HEMNET_PREFIX + locationIdsQuery + itemTypesQuery + soldAgesQuery;
            int pageNum = 1;

            HttpClient client = new HttpClient();

            try
            {

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
                            var resultNodes = document.DocumentNode.SelectNodes("//li[@class='sold-results__normal-hit']/div");

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
                                        case "sold-property-listing__location":
                                            {
                                                priceInfo.Address = property.SelectSingleNode("./h2/span[@class='item-result-meta-attribute-is-bold item-link']").InnerText.Trim();
                                                priceInfo.Area = property.SelectSingleNode("./div/span[@class='item-link']").InnerText.Replace(",", "").Trim();
                                                priceInfo.City = property.SelectSingleNode("./div/text()[last()]").InnerText.Trim();
                                                break;
                                            }
                                        case "sold-property-listing__size":
                                            {
                                                // Room Size
                                                var sizeNode = property.SelectSingleNode("./div[@class='clear-children']/div/text()[last()]").InnerText.Trim();
                                                var values = sizeNode.Split("&nbsp;");
                                                priceInfo.LivingSize = double.Parse(values[0].Trim());
                                                if (values[2].Count() != 0)
                                                {
                                                    priceInfo.Rooms = double.Parse(values[2].Trim());
                                                }
                                                // Yard Size
                                                var yardNode = property.SelectSingleNode("./div[@class='sold-property-listing__land-area sold-property-listing--left']");
                                                if (yardNode != null)
                                                {
                                                    var yardString = yardNode.InnerText.Trim().Split("&nbsp;");
                                                    priceInfo.YardSize = double.Parse(yardString[0].Replace(" ", "").Trim());
                                                }

                                                // Bi Size
                                                var biNode = property.SelectSingleNode("./div[@class='sold-property-listing__supplemental-area sold-property-listing--left']");
                                                if (biNode != null)
                                                {
                                                    var biString = biNode.InnerText.Trim().Split("&nbsp;");
                                                    priceInfo.BiSize = double.Parse(biString[0].Replace(" ", "").Trim());
                                                }

                                                break;
                                            }
                                        case "sold-property-listing__price":
                                            {
                                                // Price
                                                var priceNode = property.SelectSingleNode("./div[1]/span").InnerText.Trim().Replace("&nbsp;", "");
                                                var priceString = priceNode.Split(" ");
                                                priceInfo.Price = double.Parse(priceString[1]);

                                                // Time
                                                var timeNode = property.SelectSingleNode("./div[2]").InnerText.Trim().Replace("&nbsp;", "");
                                                var timeString = timeNode.Substring(priceNode.IndexOf("Såld") + 5);
                                                priceInfo.SoldTime = DateTime.Parse(timeString);
                                                break;
                                            }
                                        case "sold-property-listing__price-change":
                                            {
                                                // Price-Change
                                                var changeNode = property.InnerText.Trim().Replace("&nbsp;", "");
                                                changeNode = changeNode.Replace("%", "").Trim();
                                                if (changeNode.Count() != 0)
                                                {
                                                    priceInfo.PriceChange = double.Parse(changeNode);
                                                }
                                                break;
                                            }
                                    }
                                }
                                m_returnedPriceInfo.Add(priceInfo);
                            }

                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}