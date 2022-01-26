using System;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;

namespace VerIP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            IPHostEntry host;
            string localIP = "";
            string mascara = "";
            String mac = "";
            String tipointerfaz = "";
            String adaptador = "";

            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();

                    foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        //Obtener mascara
                        foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                        {
                            if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                if (localIP.Equals(unicastIPAddressInformation.Address.ToString()))
                                {
                                    adaptador = adapter.Description.ToString();
                                    tipointerfaz = adapter.NetworkInterfaceType.ToString();
                                    mac = adapter.GetPhysicalAddress().ToString();

                                    string macAddStrNew = mac;
                                    int insertedCount = 0;
                                    for (int i = 2; i < mac.Length; i = i + 2)
                                    {
                                        macAddStrNew = macAddStrNew.Insert(i + insertedCount++, ":");
                                    }
                                    mac = macAddStrNew;
                                    //Console.WriteLine("MAC: " + macAddStrNew);
                                    mascara = unicastIPAddressInformation.IPv4Mask.ToString();
                                    break;
                                }
                            }
                        }
                    }
                    break;
                }
            }
            iptxt.Text = localIP;
            mask.Text = mascara;
            puerta.Text = GetGateway().ToString();
            mactxt.Text = mac;
            adaptadortxt.Text = adaptador;
            interfaztxt.Text = tipointerfaz;
            redtxt.Text = red_conectada();
        }

        public static IPAddress GetGateway()
        {
            IPAddress result = null;
            var cards = NetworkInterface.GetAllNetworkInterfaces().ToList();
            if (cards.Any())
            {
                foreach (var card in cards)
                {
                    var props = card.GetIPProperties();
                    if (props == null)
                        continue;

                    var gateways = props.GatewayAddresses;
                    if (!gateways.Any())
                        continue;

                    var gateway =
                        gateways.FirstOrDefault(g => g.Address.AddressFamily.ToString() == "InterNetwork");
                    if (gateway == null)
                        continue;

                    result = gateway.Address;
                    break;
                };
            }
            if (result == null)
            {
                return System.Net.IPAddress.Parse("0.0.0.0");

            }
            else
            {
                return result;
            }
        }

        public static String red_conectada()
        {
            ConnectionOptions options = new ConnectionOptions();
            options.Impersonation = System.Management.ImpersonationLevel.Impersonate;


            ManagementScope scope = new ManagementScope("\\\\" + Environment.MachineName.ToString() + "\\root\\StandardCimv2", options);
            scope.Connect();

            //Query system for Operating System information
            ObjectQuery query = new ObjectQuery("SELECT * FROM MSFT_NetConnectionProfile Where IPv4Connectivity = '4'");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

            ManagementObjectCollection queryCollection = searcher.Get();
            foreach (ManagementObject m in queryCollection)
            {
                return m["Name"].ToString();
            }
            return null;
        }

    }
}
