using System;
using Independentsoft.Sip;
using Independentsoft.Sip.Methods;

namespace SipTester
{
    class Program
    {
        private static Logger logger;
        private static SipClient clientSIP;

        static void Main(string[] args)
        {
            /*
             Use miniSipServer to create the test server
             */

            string sipDomain = "10.0.0.52";
            string sipLogin = "101";
            string sipPassword = "101";
            string localIp = "10.0.0.55";

            clientSIP = new SipClient(sipDomain, sipLogin, sipPassword);

            logger = new Logger();
            logger.WriteLog += new WriteLogEventHandler(OnWriteLog);
            clientSIP.Logger = logger;

            clientSIP.ReceiveRequest += new ReceiveRequestEventHandler(OnReceiveRequest);
            clientSIP.ReceiveResponse += new ReceiveResponseEventHandler(OnReceiveResponse);

            System.Net.IPAddress localAddress = System.Net.IPAddress.Parse(localIp);
            clientSIP.LocalIPEndPoint = new System.Net.IPEndPoint(localAddress, 5060);

            clientSIP.Connect();

            Register register = new Register();
            register.Uri = string.Format("sip:{0}", sipDomain);
            register.From = new ContactInfo(sipLogin, string.Format("sip:{0}@{1}", sipLogin, sipDomain));
            register.To = new ContactInfo(sipLogin, string.Format("sip:{0}@{1}", sipLogin, sipDomain));
            register.Header[StandardHeader.Contact] = string.Format("sip:{0}@", sipLogin) + clientSIP.LocalIPEndPoint.ToString();
            register.Expires = 1200;
            clientSIP.SendRequest(register);

            Console.Write("Press any key to continue...");
            Console.ReadLine();

            clientSIP.Unregister(register.Uri, register.From);
        }

        private static void OnReceiveRequest(object sender, RequestEventArgs e)
        {
            Console.WriteLine("OnReceiveRequest");
        }

        private static void OnReceiveResponse(object sender, ResponseEventArgs e)
        {
            Console.WriteLine("OnReceiveResponse");
        }

        private static void OnWriteLog(object sender, WriteLogEventArgs e)
        {
            Console.WriteLine(e.Log);
        }    
    }
}
