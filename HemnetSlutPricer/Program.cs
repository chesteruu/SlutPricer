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
                if(args[1] == "hemnet")
                {
                    var result = HemnetPricer.GetAreaId(args[2]);

                    result.Wait();
                    var locations = result.Result;
                    Console.WriteLine(String.Join("\n", locations.Select(x => x.id.ToString() + "\t" + x.name + "\t\t" + x.parent_location.name).ToArray()));
                    return;
                }
                
                if(args[1] == "booli")
                {
                    var result = BooliPricer.GetAreaId(args[2]);

                    result.Wait();
                    var locations = result.Result;
                    Console.WriteLine(String.Join("\n", locations.Select(x => x.area.id.ToString() + "\t" + x.area.name + "\t" + x.area.url + "\t\t" + x.label).ToArray()));
                    return;
                }
            }

            if (args.Count() == 6)
            {
                List<string> locationIds = args[1].Split(";").ToList();
                List<string> type = args[2].Split(";").ToList();

                if (args[5] == "hemnet")
                {
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

                if(args[5] == "booli")
                {

                    BooliPricer booliPricer = new BooliPricer(locationIds, type, args[3]);
                    booliPricer.DoRequest().Wait();
                    var priceInfoList = booliPricer.GetPriceInfoList();

                    using (StreamWriter sw = new StreamWriter(args[4]))
                    {

                        sw.WriteLine("Total Object: " + priceInfoList.Count);
                        Console.WriteLine("Total Object: " + priceInfoList.Count);

                        if (priceInfoList.Count > 0)
                        {
                            sw.WriteLine("Searched Time: " + priceInfoList[0].SoldTime.ToShortDateString() + " => " + priceInfoList[priceInfoList.Count - 1].SoldTime.ToShortDateString());
                            Console.WriteLine("Searched Time: " + priceInfoList[0].SoldTime.ToShortDateString() + " => " + priceInfoList[priceInfoList.Count - 1].SoldTime.ToShortDateString());
                        }

                        foreach (var priceInfo in booliPricer.GetPriceInfoList())
                        {
                            sw.WriteLine(priceInfo.ToJson());
                            Console.WriteLine(priceInfo.ToString());
                        }

                    }

                }
            }
        }
    }
}
