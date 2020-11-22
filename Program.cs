using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StwDaily.Models;

namespace StwDaily
{
    class Program
    {
        static async Task Main(string[] args)
        {
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;
            MainModel data;
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "https://www.epicgames.com/id/logout?redirectUrl=https%3A%2F%2Fwww.epicgames.com%2Fid%2Flogin%3FredirectUrl%3Dhttps%253A%252F%252Fwww.epicgames.com%252Fid%252Fapi%252Fredirect%253FclientId%253Dec684b8c687f479fadea3cb2ad83f5c6%2526responseType%253Dcode",
                UseShellExecute = true
            };
           // Process.Start(psi);
            Console.WriteLine("Enter fnauth code: ");
            var code = Console.ReadLine();
            data = await GetToken(code);
            var errorCode = data.errorCode;
            var access_token = data.access_token;
            var accout_id = data.account_id;
            if (access_token == null && accout_id == null)
            {  
                cancelTokenSource.Cancel();
                Console.WriteLine($"Error: {errorCode}");
            }
            await GetReward(access_token, accout_id, token);
        }
        static async Task<MainModel> GetToken(string code)
        { 
            MainModel data;
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
            nvc.Add(new KeyValuePair<string, string>("code", $"{code}"));
            Uri apiReq = new Uri("https://account-public-service-prod.ol.epicgames.com/account/api/oauth/token");
            var httpClientHandler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler: httpClientHandler, disposeHandler: true);
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, apiReq) { Content = new FormUrlEncodedContent(nvc) };
            req.Headers.TryAddWithoutValidation("Connection", "keep-alive");
            req.Headers.TryAddWithoutValidation("Authorization", "basic ZWM2ODRiOGM2ODdmNDc5ZmFkZWEzY2IyYWQ4M2Y1YzY6ZTFmMzFjMjExZjI4NDEzMTg2MjYyZDM3YTEzZmM4NGQ=");
            var resp = await client.SendAsync(req);
            string jsonContent = await resp.Content.ReadAsStringAsync();
            data = JsonConvert.DeserializeObject<MainModel>(jsonContent);
            return data;
        }
        static async Task GetReward(string access_token, string account_id, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }
            Uri apiReq = new Uri($"https://fortnite-public-service-prod11.ol.epicgames.com/fortnite/api/game/v2/profile/{account_id}/client/ClaimLoginReward?profileId=campaign");
            var httpClientHandler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler: httpClientHandler, disposeHandler: true);
            var req = new HttpRequestMessage(HttpMethod.Post, apiReq);
            req.Content = new JsonContent("{}");
            req.Headers.TryAddWithoutValidation("Authorization", $"bearer {access_token}");
            var resp = await client.SendAsync(req);
            string jsonContent = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode == HttpStatusCode.OK)
                Console.WriteLine("Your daily reward should be claimed.");
            else Console.WriteLine("Something went wrong :(");
        }
    }
  
}
