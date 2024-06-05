using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace unsplashApi
{
    class Program
    {
        private static readonly string AccessKey = "QilKyffYwDFYAaX9kN_V31cxDHdWGKeG9UDYUNu0gk0";

        static async Task Main(string[] args)
        {
            bool continueSearching = true;

            while (continueSearching)
            {
                Console.Write("Search Photo: ");
                string query = Console.ReadLine();
                string photoUrl = await FetchFirstPhotoUrlAsync(query);

                if (photoUrl != null)
                {
                    Console.WriteLine($"First photo URL: {photoUrl}");
                    await PrintImageAsAscii(photoUrl);
                }
                else
                {
                    Console.WriteLine("No photos found");
                }

                Console.Write("Search again [Y/N]: ");
                string response = Console.ReadLine().ToUpper();

                if (response != "Y")
                {
                    continueSearching = false;
                }
            }
        }
        static async Task<string> FetchFirstPhotoUrlAsync(string query)
        {
            using HttpClient client = new HttpClient();
            string url = $"https://api.unsplash.com/search/photos?query={query}&client_id={AccessKey}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(responseBody);
                    JArray results = (JArray)json["results"];

                    if (results.Count > 0)
                    {
                        return results[0]["urls"]["regular"].ToString();
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
            }

            return null;
        }

        static async Task PrintImageAsAscii(string url)
        {
            using HttpClient client = new HttpClient();
            Stream imageStream = await client.GetStreamAsync(url);

            using (Bitmap bitmap = new Bitmap(imageStream))
            {
                Bitmap grayBitmap = ConvertToGrayscale(bitmap);

                Bitmap resizedBitmap = ResizeImage(grayBitmap, 500);

                string asciiArt = ConvertToAscii(resizedBitmap);

                Console.WriteLine(asciiArt);
            }
        }

        static Bitmap ConvertToGrayscale(Bitmap original)
        {
            Bitmap grayBitmap = new Bitmap(original.Width, original.Height);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color originalColor = original.GetPixel(x, y);
                    int grayScale = (int)((originalColor.R * 0.3) + (originalColor.G * 0.59) + (originalColor.B * 0.11));
                    Color grayColor = Color.FromArgb(grayScale, grayScale, grayScale);
                    grayBitmap.SetPixel(x, y, grayColor);
                }
            }

            return grayBitmap;
        }

        static Bitmap ResizeImage(Bitmap original, int newWidth)
        {
            int originalWidth = original.Width;
            int originalHeight = original.Height;
            float ratio = (float)newWidth / originalWidth;
            int newHeight = (int)(originalHeight * ratio);

            Bitmap resizedBitmap = new Bitmap(newWidth, newHeight);
            using (Graphics graphics = Graphics.FromImage(resizedBitmap))
            {
                graphics.DrawImage(original, 0, 0, newWidth, newHeight);
            }

            return resizedBitmap;
        }

        static string ConvertToAscii(Bitmap image)
        {
            string asciiChars = "@%#*+=-:. ";
            int width = image.Width;
            int height = image.Height;
            StringBuilder asciiArt = new StringBuilder();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    int gray = pixelColor.R;
                    int index = gray * (asciiChars.Length - 1)  / 255;
                    asciiArt.Append(asciiChars[index]);
                }
                asciiArt.AppendLine();
            }

            return asciiArt.ToString();
        }
    }
}
