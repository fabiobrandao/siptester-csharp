using Independentsoft.Sip;
using Independentsoft.Sip.Methods;
using System;
using System.Configuration;

namespace SipTester
{
    class Program
    {
        private static Logger logger;
        private static SipClient client;
        private static Register register;
        private static string sipDomain = ConfigurationManager.AppSettings["SipDomain"];
        private static string sipLogin = ConfigurationManager.AppSettings["SipLogin"];
        private static string sipPassword = ConfigurationManager.AppSettings["SipPassword"];
        private static string sipProtocolType = ConfigurationManager.AppSettings["SipProtocolType"];

        private static void Main(string[] args)
        {
            register = new Register();
            Connect();
        }

        private static void Connect()
        {
            try
            {
                // Configure client
                if (sipProtocolType == "tcp")
                    client = new SipClient(sipDomain, ProtocolType.Tcp, sipLogin, sipPassword);
                else if (sipProtocolType == "upd")
                    client = new SipClient(sipDomain, ProtocolType.Udp, sipLogin, sipPassword);
                else
                    client = new SipClient(sipDomain, sipLogin, sipPassword);
                client.Timeout = 5000;

                // Log Register
                if (logger == null)
                {
                    logger = new Logger();
                    logger.WriteLog += new WriteLogEventHandler(OnWriteLog);
                }

                client.Logger = logger;

                // Events Register                
                client.ReceiveRequest += new ReceiveRequestEventHandler(OnReceiveRequest);
                client.ReceiveResponse += new ReceiveResponseEventHandler(OnReceiveResponse);

                Console.WriteLine(">> SIP Tester");
                Console.WriteLine("");
                Console.WriteLine("Domain...: " + sipDomain);
                Console.WriteLine("Login....: " + sipLogin);
                Console.WriteLine("Password.: " + sipPassword);
                Console.WriteLine("Protocol.: " + sipProtocolType);
                Console.WriteLine("");

                // Client Connect
                client.Connect();

                register.Uri = string.Format("sip:{0}", sipDomain);
                register.From = new ContactInfo(sipLogin, string.Format("sip:{0}@{1}", sipLogin, sipDomain));
                register.To = new ContactInfo(sipLogin, string.Format("sip:{0}@{1}", sipLogin, sipDomain));
                register.Header[StandardHeader.Contact] = string.Format("sip:{0}@{1}", sipLogin, client.LocalIPEndPoint.ToString());
                register.Expires = 300;

                client.SendRequest(register);

                Console.WriteLine("");
                Console.WriteLine("Press any key to unregister, disconnect and reconnect! Press CTRL + C to abort...");
                Console.ReadLine();

                client.Unregister(register.Uri, register.From);
                client.Disconnect();

                Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("");
                Console.WriteLine("Press any key to exit...");
                Console.ReadLine();
            }
        }

        private static void OnReceiveRequest(object sender, RequestEventArgs e)
        {
            client.AcceptRequest(e.Request);

            Console.WriteLine(string.Format("ReceiveRequest From {0} CallID {1}", e.Request.From, e.Request.CallID));
        }

        private static void OnReceiveResponse(object sender, ResponseEventArgs e)
        {            
            Console.WriteLine(string.Format("Receive PABX message, StatusCode [{0}], operation [{1}] Fom {2}", e.Response.StatusCode , e.Response.CSeq.ToUpper(), e.Response.From.Name));

            if (e.Response.StatusCode == 200)
            {
                Console.Beep();
                Console.WriteLine(string.Format("{0} - NUMBER REGISTERED!", e.Response.From.Name));
            }
        }

        private static void OnWriteLog(object sender, WriteLogEventArgs e)
        {
            Console.WriteLine(e.Log);
        }
    }
}