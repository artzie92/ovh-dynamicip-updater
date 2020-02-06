using Microsoft.Extensions.Configuration;
using OvhDynamicIpUpdater.Models;
using System;
using System.IO;
using System.Net;
using System.Threading;

namespace OvhDynamicIpUpdater
{
    public class Program
    {
        private static void Main(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .Build();

            var appSettings = new AppSettings();
            configuration.GetSection("AppSettings").Bind(appSettings);

            try
            {
                while (true)
                {
                    string ip = GetIPAddress();
                    string message = $"{DateTime.Now.ToShortDateString()}, {DateTime.Now.ToLongTimeString()} Your current IP: {ip}";
                    Log(message);
                    Console.WriteLine(message);
                    string res = UpdateIp(appSettings.Host, ip, appSettings.Login, appSettings.Password);
                    Console.WriteLine(res);
                    Log(res);
                    Thread.Sleep(new TimeSpan(0, 0, appSettings.Interval, 0, 0));
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                throw;
            }
        }

        private static void Log(string message)
        {
            using (System.IO.StreamWriter file =
              new System.IO.StreamWriter("log.txt", append: true))
            {
                file.WriteLine(message);
            }
        }

        private static string UpdateIp(string host, string ip, string login, string password)
        {
            string responseString = "";
            WebRequest request = WebRequest.Create($"http://www.ovh.com/nic/update?system=dyndns&hostname={host}&myip={ip}");
            request.Headers.Add(HttpRequestHeader.Authorization, $"Basic {Base64Encode($"{login}:{password}")}");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                responseString = stream.ReadToEnd();
            }

            return responseString;
        }

        private static string GetIPAddress()
        {
            String address = "";
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                address = stream.ReadToEnd();
            }

            int first = address.IndexOf("Address: ") + 9;
            int last = address.LastIndexOf("</body>");
            address = address.Substring(first, last - first);

            return address;
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}