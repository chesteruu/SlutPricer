using System;
using System.IO;
using Newtonsoft;

namespace PriceScrapper
{
    public class PriceInfo
    {
        public string Address;
        public string Area;
        public string City;
        public string Broker;
        public double Price;
        public double Rooms;
        public double LivingSize;
        public double BiSize;
        public double YardSize;
        public DateTime SoldTime;
        public double PriceChange;

        public override string ToString()
        {
            return String.Format("Address: {0}| Area: {1}| City: {2}| LivingSize: {3}| BiSize: {4}| YardSize: {5}|" +
                " Price: {6}| SoldTime: {7}| PriceChange: {8}%|", Address, Area, City, LivingSize, BiSize, YardSize, Price, SoldTime.ToShortDateString(), PriceChange);
        }

        public string ToJson()
        {
            StringWriter sw = new StringWriter();
            Newtonsoft.Json.JsonSerializer jsonSerializer = new Newtonsoft.Json.JsonSerializer();
            jsonSerializer.Serialize(sw, this);

            return sw.ToString();
        }
        
    }
}
