using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace JLSForeclosures
{
    class Program
    {
        static void Main(string[] args)
        {
            Run().Wait();
        }

        static async Task Run()
        {
            HttpClient httpClient = new HttpClient();
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"username", ""},
                    {"password", ""},
                });
            var response = await httpClient.PostAsync("http://www.jlsforeclosures.com/login.php", form);
            var content = await response.Content.ReadAsStringAsync();

            form = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"Submitrelease", "Submit"},
                });
            response = await httpClient.PostAsync("http://www.jlsforeclosures.com/index.php", form);
            content = await response.Content.ReadAsStringAsync();

            var zipCodes = new Dictionary<string, string>()
            {
                {"98004", "Bellevue"},
                {"98005", "Bellevue405to140th"},
                {"98006", "BellevueSouth"},
                {"98007", "Bellevue140th156th"},
                {"98008", "Bellevue156thSammamishLake"},
                {"98033", "Kirkland"},
                {"98034", "KirklandNorth"},
                {"98040", "MercerIsland"},
                {"98052", "Redmond"},
                {"98056", "NewcastleRenton"},
                {"98074", "Sammamish"},
                {"98103", "FremontWallingfordGreenLake"},                
                {"98105", "UW"},
                {"98109", "QueenAnneSouthLakeUnion"},
                {"98115", "GreenLakeMapleLeafNorthUW"},
                {"98121", "Belltown"},
            };

            var today = DateTime.UtcNow.Date;
            var friday = DateTime.UtcNow.Date;

            if (today.DayOfWeek == DayOfWeek.Saturday)
            {
                friday = friday.AddDays(6);
            }
            else if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                friday = friday.AddDays(5);
            }
            else if (today.DayOfWeek == DayOfWeek.Monday)
            {
                friday = friday.AddDays(4);
            }
            else if (today.DayOfWeek == DayOfWeek.Tuesday)
            {
                friday = friday.AddDays(3);
            }
            else if (today.DayOfWeek == DayOfWeek.Wednesday)
            {
                friday = friday.AddDays(2);
            }
            else if (today.DayOfWeek == DayOfWeek.Thursday)
            {
                friday = friday.AddDays(1);
            }

            var saleDates = new List<string>();
            for (int week = 0; week < 8; week++)
            {
                var saleDate = friday.AddDays(7 * week).ToString("yyyy-MM-dd");
                saleDates.Add(saleDate);
            };

            foreach (FileInfo f in new DirectoryInfo(Environment.CurrentDirectory).GetFiles("*.html"))
            {
                f.Delete();
            }

            foreach (var saleDate in saleDates)
            {
                foreach (var zipCode in zipCodes)
                {
                    form = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        {"zip", zipCode.Key},
                        {"sale_date", saleDate},
                    });

                    response = await httpClient.PostAsync("http://www.jlsforeclosures.com/listings.php", form);
                    content = await response.Content.ReadAsStringAsync();

                    if (content.Contains("<p>No Results</p>"))
                    {
                        continue;
                    }

                    content = content.Replace("/css/style.css", "http://www.jlsforeclosures.com/css/style.css");
                    content = content.Replace("/phpThumb/phpThumb.php", "http://www.jlsforeclosures.com/phpThumb/phpThumb.php");
                    content = content.Replace("JLSForecloseureServiGreenTransparent.gif", "http://www.jlsforeclosures.com/JLSForecloseureServiGreenTransparent.gif");
                    content = content.Replace("\\images\\star.jpg", "http://www.jlsforeclosures.com/images/star.jpg");

                    var filename = string.Format("{0}.{1}.{2}.html", saleDate, zipCode.Key, zipCode.Value);
                    Console.WriteLine("writing {0}", filename);
                    File.WriteAllText(filename, content);
                }
            }
        }
    }
}
