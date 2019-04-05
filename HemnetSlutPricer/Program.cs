using System;
using System.Collections.Generic;
using PriceScrapper;

namespace HemnetSlutPricer
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = HemnetPricer.GetAreaId("skarpang");

            result.Wait();

            Console.WriteLine(result.Result);
            PriceScrapper.HemnetPricer hemnetPricer = new HemnetPricer(new List<string> { "473319" }, new List<string>{"villa"}, "48m");
            hemnetPricer.DoRequest().Wait();

            var priceInfoList = hemnetPricer.GetPriceInfoList();

            Console.WriteLine("Total Object: " + priceInfoList.Count);

            if (priceInfoList.Count > 0)
            {
                Console.WriteLine("Searched Time: " + priceInfoList[0].SoldTime.ToShortDateString() + " => " + priceInfoList[priceInfoList.Count-1].SoldTime.ToShortDateString());
            }
            foreach (var priceInfo in hemnetPricer.GetPriceInfoList())
            {
                Console.WriteLine(priceInfo);
            }
        }
    }
}
