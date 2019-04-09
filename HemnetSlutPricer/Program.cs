using PriceScrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HemnetSlutPricer
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Count() == 0)
            {
                Console.WriteLine("pricer location <hemnet|booli> <area>");
                Console.WriteLine("pricer get <location_id> <type> <timespan> <filepath>");
                return;
            }

            if(args.Count() == 3)
            {
                var task = ScrapperFactory.GetLocations(args[1], args[2]);
                Console.WriteLine("Waiting result from providers...");
                task.Wait();
                var locations = task.Result;
                Console.WriteLine(String.Join("\n", locations.Select(x => x.GetId() + "\t" + x.GetName() + "\t" + x.GetAreaName()).ToArray()));
            }

            if (args.Count() == 6)
            {
                List<string> locationIds = args[1].Split(";").ToList();
                List<string> type = args[2].Split(";").ToList();

                IScrapper scrapper = ScrapperFactory.GetScrapper(args[5], locationIds, type, args[3]);
                var task = scrapper.DoRequest();
                Console.WriteLine("Waiting result from providers...");
                task.Wait();

                var priceInfos = task.Result;

                PriceScrapper.HemnetPricer hemnetPricer = new HemnetPricer(locationIds, type, args[3]);
                hemnetPricer.DoRequest().Wait();

                var priceInfoList = hemnetPricer.GetPriceInfoList();

                using (StreamWriter sw = new StreamWriter(args[4]))
                {

                    sw.WriteLine("Total Object: " + priceInfoList.Count);
                    Console.WriteLine("Total Object: " + priceInfoList.Count);

                    if (priceInfoList.Count > 0)
                    {
                        sw.WriteLine("Searched Time: " + priceInfoList[0].SoldTime.ToShortDateString() + " => " + priceInfoList[priceInfoList.Count - 1].SoldTime.ToShortDateString());
                        Console.WriteLine("Searched Time: " + priceInfoList[0].SoldTime.ToShortDateString() + " => " + priceInfoList[priceInfoList.Count - 1].SoldTime.ToShortDateString());
                    }

                    foreach (var priceInfo in hemnetPricer.GetPriceInfoList())
                    {
                        sw.WriteLine(priceInfo.ToJson());
                        Console.WriteLine(priceInfoList.ToString());
                    }

                }
            }
        }
    }
}
