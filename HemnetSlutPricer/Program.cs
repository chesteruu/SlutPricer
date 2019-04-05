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
                Console.WriteLine("pricer location stockholm.");
                Console.WriteLine("pricer get <location_id> <type> <timespan> <filepath>");
                return;
            }

            if(args.Count() == 2)
            {
                var result = HemnetPricer.GetAreaId(args[1]);

                result.Wait();
                var locations = result.Result;
                Console.WriteLine(String.Join("\n", locations.Select(x => x.id.ToString() + "\t" + x.name + "\t\t" + x.parent_location.name).ToArray()));
                return;
            }

            if (args.Count() == 5)
            {
                List<string> locationIds = args[1].Split(";").ToList();
                List<string> type = args[2].Split(";").ToList();
           
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
                        Console.WriteLine(priceInfo.ToString());
                    }

                }
            }
        }
    }
}
