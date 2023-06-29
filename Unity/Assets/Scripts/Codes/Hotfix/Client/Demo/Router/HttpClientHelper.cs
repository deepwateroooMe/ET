using System;
using System.IO;
using System.Net.Http;
namespace ET.Client {
    public static class HttpClientHelper {

        // 【诸多帮助类】：框架重构后，就崩出了狠多的帮助类。狠多时候，更多的是感觉这个帮助类，出现得不明所以。。。
        // 得弄明白：为什么需要这些帮助类，可不可以砍掉，可不可以与其它类合并？或是挪出热更域到ET 命名空间？
        // 就这一个方法：一处使用的地方，也得一个专门的帮助类。。。
        public static async ETTask<string> Get(string link) {
            try {
                using HttpClient httpClient = new HttpClient();
                HttpResponseMessage response =  await httpClient.GetAsync(link); // 上下到这里两行：都是底层方法 
                // 【返回】：这里返回的，应该是，这个特殊路由器服的相关服务端信息
                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (Exception e) {
                throw new Exception($"http request fail: {link.Substring(0,link.IndexOf('?'))}\n{e}");
            }
        }
    }
}


