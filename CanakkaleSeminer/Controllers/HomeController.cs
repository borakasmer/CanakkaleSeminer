using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Drawing;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace CanakkaleSeminer.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public string CreateBarcode(string ConnectionID)
        {
            string Code = "http://192.168.43.36:63/ControlPage/" + ConnectionID;
            string width = "150";
            string height = "150";
            var url = string.Format($"http://chart.apis.google.com/chart?cht=qr&chs={width}x{height}&chl={Code}");
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            Image img = Image.FromStream(stream);
            Guid imgName = Guid.NewGuid();
            img.Save(Server.MapPath("/QrBarcode/" + imgName + ".png"));
            return "<img src='/QrBarcode/" + imgName + ".png' width=" + width + " height=" + height + " alt='QrBarcode' class='barcode' />æ" + imgName;
        }

        public void DelImage(string ImageName)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Server.MapPath("/QrBarcode"));
            directoryInfo.Empty(ImageName);
        }
        
        public async Task<ActionResult> ControlPage(string ConnectionID)
        {
            ViewBag.ConnectionID = ConnectionID;
            await MoveRobot(ConnectionID, "connected");
            return View();
        }

        public async Task MoveRobot(string connectionID, string command)
        {
            if (Session["hubProxy"] == null)
            {
                HubConnection hubConnection = new HubConnection("http://192.168.43.36:63");
                IHubProxy hubProxy = hubConnection.CreateHubProxy("Product");
                Session.Add("hubProxy", hubProxy);
                await hubConnection.Start(new LongPollingTransport());
                hubProxy.Invoke("MoveRobot", connectionID, command);
            }
            else
            {
                ((IHubProxy)Session["hubProxy"]).Invoke("MoveRobot", connectionID, command);
            }

        }
    }
    public class Product : Hub
    {
        public override async Task OnConnected()
        {
            await Clients.Caller.getConnectionID(Context.ConnectionId);
        }

        public async Task MoveRobot(string connectionID, string command)
        {
            await Clients.Client(connectionID).moveRobot(command);
        }
    }

    public static class Tool
    {
        public static void Empty(this DirectoryInfo directory, string ImageName)
        {
            foreach (var file in directory.GetFiles())
            {
                if (file.FullName.Contains(ImageName))
                {
                    file.Delete();
                    break;
                }
            }
        }
    }
}