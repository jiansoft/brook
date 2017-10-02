using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace DemoNetCore
{
    class Program
    {
        static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("app.json").AddXmlFile("app.config");
            var config = builder.Build();
            string ikey = config["key"];
            Console.WriteLine($"Hello World! {ikey}");
            Console.ReadKey();
        }
    }
}
