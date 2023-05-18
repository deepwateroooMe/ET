using System;
using System.IO;
using System.Net.Http;
namespace ET.Client {
    public static class HttpClientHelper {

        public static async ETTask<string> Get(string link) {
            try {
                using HttpClient httpClient = new HttpClient();
                HttpResponseMessage response =  await httpClient.GetAsync(link);
                // 【返回】：这里返回的，应该是，这个特殊路由器服的相关服务器信息
                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (Exception e) {
                throw new Exception($"http request fail: {link.Substring(0,link.IndexOf('?'))}\n{e}");
            }
        }
    }
}