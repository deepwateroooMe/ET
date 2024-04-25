using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ET {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】

	// 静态帮助类：可以把它想成是，一台物理机的【守护进程】的网络相关帮助类。
    public static class NetworkHelper {
        public static string[] GetAddressIPs() { // 拿这台物理机上，所有进程相关的IP 地址？
            List<string> list = new List<string>();
			// Networklnterface 类：这个类可以得到本机所有的物理网络接口，和虚拟机等软件利用本机的物理网络接口创建的逻辑网络接口的信息
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces()) {
                if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet) { // 代表 Ethernet 联机的 NetworklnterfaceType.Ethemet 
                    continue;
                } // GetIPProperties(): 返回描述此网络接口的配置的对象
				// GetIPProperties()方法返回的是 IPInterfaceProperties 对象,该对象提供支持IPv4或IPv6的网络接口相关信息
                foreach (UnicastIPAddressInformation add in networkInterface.GetIPProperties().UnicastAddresses) {
                    list.Add(add.Address.ToString());
                }
            }
            return list.ToArray();
        }
        // 优先获取IPV4的地址
        public static IPAddress GetHostAddress(string hostName) {
            IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
            IPAddress returnIpAddress = null;
            foreach (IPAddress ipAddress in ipAddresses) {
                returnIpAddress = ipAddress;
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork) {
                    return ipAddress;
                }
            }
            return returnIpAddress;
        }
        public static IPEndPoint ToIPEndPoint(string host, int port) {
            return new IPEndPoint(IPAddress.Parse(host), port);
        }
        public static IPEndPoint ToIPEndPoint(string address) { // 从一个字符串，来 parse 出IP 地址
            int index = address.LastIndexOf(':');      // idx-of 最后一个：
            string host = address.Substring(0, index); // ：前是IP 地址
            string p = address.Substring(index + 1);   // 最后：后是、端口
            int port = int.Parse(p); // 端口
            return ToIPEndPoint(host, port);
        }
        public static void SetSioUdpConnReset(Socket socket) {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                return;
            }
			// Windows 平台下的：特殊处理
			const uint IOC_IN = 0x80000000;
            const uint IOC_VENDOR = 0x18000000;
            const int SIO_UDP_CONNRESET = unchecked((int)(IOC_IN | IOC_VENDOR | 12));
            socket.IOControl(SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
        }
    }
}
