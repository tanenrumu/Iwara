using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static Iwara.Script.Class.Analyser;
using static Iwara.Script.Class.Base;
using static Iwara.Script.UIManager;

namespace Iwara.Script.Network
{
    class Base
    {
        public static HttpWebRequest GetBaseRequest(CustomUrl customUrl)
        {
            string siteDomain = customUrl.subDomain;
            string cgi = customUrl.cgi;
            string domain = siteDomain + ".iwara.tv";

            if (MainWindow.Settings.EnableDoH && MainWindow.Settings.HostsList.ContainsKey(siteDomain))
            {
                domain = MainWindow.Settings.HostsList[siteDomain];
            }
            Console.WriteLine("https://" + domain + cgi);
            HttpWebRequest request = WebRequest.CreateHttp("https://" + domain + cgi);
            request.Host = siteDomain + ".iwara.tv";

            if (MainWindow.Settings.EnableProxy)
            {
                request.Proxy = new WebProxy(MainWindow.Settings.ProxyServer, Convert.ToInt32(MainWindow.Settings.ProxyPort));
            }

            if (siteDomain == "ecchi")
            {
                CookieContainer cookieContainer = new CookieContainer();
                cookieContainer.Add(new Cookie("show_adult", "1")
                {
                    Domain = siteDomain + ".iwara.tv"
                });
                request.CookieContainer = cookieContainer;
            }
            return request;
        }
        public static string GetTextFromIwara(CustomUrl customUrl)
        {
            string outPut;
            try
            {
                HttpWebResponse response = (HttpWebResponse)GetBaseRequest(customUrl).GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                    outPut = streamReader.ReadToEnd();
                    streamReader.Close();
                    response.Close();
                }
                else
                {
                    outPut = "error: " + response.StatusCode;
                }
            }
            catch (Exception e)
            {
                outPut = "error: " + e.Message;
            }
            return outPut;
        }
        public static void SetHosts(string siteDomain)
        {
            //HttpWebRequest request = WebRequest.CreateHttp("https://myssl.com/api/v1/tools/dns_query?qtype=1&host=" + siteDomain + ".iwara.tv&qmode=-1");
            HttpWebRequest request = WebRequest.CreateHttp("https://1.1.1.1/dns-query?name=" + siteDomain + ".iwara.tv&type=A&ct=application/dns-json");
            request.Method = "GET";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                    string str = streamReader.ReadToEnd();
                    streamReader.Close();
                    response.Close();
                    string host = AnalyseHostByDoH(str);
                    if (host == "error")
                    {
                        if (LogError("Get Hosts Error:\nNo Alive Hosts!") == "Yes") { SetHosts(siteDomain); }
                    }
                    else
                    {
                        if (MainWindow.Settings.HostsList.ContainsKey(siteDomain))
                        {
                            MainWindow.Settings.HostsList[siteDomain] = host;
                        }
                        else
                        {
                            MainWindow.Settings.HostsList.Add(siteDomain, host);
                        }
                    }
                    /*
                    ArrayList hosts = AnalyseHosts(str);
                    bool error = false;
                    foreach (HostsInfo hostsInfo in hosts)
                    {
                        if (hostsInfo.error == false)
                        {
                            // time_consume 最小的 ip
                            if (MainWindow.Settings.HostsList.ContainsKey(siteDomain))
                            {
                                MainWindow.Settings.HostsList[siteDomain] = hostsInfo.ip;
                            }
                            else
                            {
                                MainWindow.Settings.HostsList.Add(siteDomain, hostsInfo.ip);
                            }
                            break;
                        }
                        else
                        {
                            error |= hostsInfo.error;
                        }
                    }
                    if (error)
                    {
                        if (LogError("Get Hosts Error:\nNo Alive Hosts!") == "Yes") { SetHosts(siteDomain); }
                    }*/
                }
                else
                {
                    if (LogError("Get Hosts Error:\n" + response.StatusCode) == "Yes") { SetHosts(siteDomain); }
                }
            }
            catch (Exception e)
            {
                if (LogError("Get Hosts Error:\n" + e.Message) == "Yes") { SetHosts(siteDomain); }
            }
        }
    }
}
