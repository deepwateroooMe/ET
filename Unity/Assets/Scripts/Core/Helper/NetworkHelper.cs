﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
namespace ET {
    // ET 命名空间：应该不是热更域，提供最底层最基础的帮助方法。热更域里可以实时调用。
    // 这个类，不用了解细节，太底层了
    public static class NetworkHelper {
        // 方法的逻辑细节：极底层。现在并不想花时间去弄懂。
        // 只了解什么情况下会调用这个底层方法：有个工监服，实时扫描服务端系统，有没有哪个宕机了掉线了WatcherComponentSystem.cs WatcherHelp.cs 里会调用这个方法
        public static string[] GetAddressIPs() {
            List<string> list = new List<string>();
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces()) {
                if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet) { // 必须是 Ethernet
                    continue;
                }
                foreach (UnicastIPAddressInformation add in networkInterface.GetIPProperties().UnicastAddresses) {
                    list.Add(add.Address.ToString());
                }
            }
            return list.ToArray();
        }
        // 优先获取IPV4的地址
        public static IPAddress GetHostAddress(string hostName) {
            IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName); // 通过。NET 网络底层方法，拿到地址
            IPAddress returnIpAddress = null;
            foreach (IPAddress ipAddress in ipAddresses) {
                returnIpAddress = ipAddress;
                // 遍历扫描：扫到那个对的，就返回了；扫不到，返回空
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork) { // 底层定义：相对复杂，应该也可以不需要懂
                    return ipAddress;
                }
            }
            return returnIpAddress;
        }
        // 帮助方法：将字符串与端口等，转化成热更域里，可以使用用来拿的小服地址，或是方便建立的与各小服（IPEndPoint）的会话框等
        public static IPEndPoint ToIPEndPoint(string host, int port) {
            return new IPEndPoint(IPAddress.Parse(host), port);
        }
        public static IPEndPoint ToIPEndPoint(string address) {
            int index = address.LastIndexOf(':');
            string host = address.Substring(0, index);
            string p = address.Substring(index + 1);
            int port = int.Parse(p);
            return ToIPEndPoint(host, port);
        }
        public static void SetSioUdpConnReset(Socket socket) {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                return;
            }
            const uint IOC_IN = 0x80000000;
            const uint IOC_VENDOR = 0x18000000;
            const int SIO_UDP_CONNRESET = unchecked((int)(IOC_IN | IOC_VENDOR | 12));
            socket.IOControl(SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
        }
    }
}
