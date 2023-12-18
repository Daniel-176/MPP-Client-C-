using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using WebSocketSharp;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Security.Policy;
using System.Net.Configuration;
using System.Runtime.Remoting.Messaging;
using Newtonsoft.Json;
using System.Net;
using System.Collections;
using System.Threading;
using System.Runtime.Remoting;

namespace MPP_Client_C_
{
    public class Client : EventEmitter
    {
        public WebSocket _ws = null;
        private readonly System.Timers.Timer _pingInterval;
        public dynamic user;
        private readonly DateTime _st = new DateTime(1970, 1, 1);
        public object Users;
        public Uri _uri;
        public string _useragent;
        public dynamic Channel;
        public string _token;
        public long ServerTimeOffset;
        private int _connectionAttempts;
        private bool _connected = false;
        public event EventHandler Disconnected = (_param1, _param2) => { };

        public Client(Uri uri, string token = null)
        {
            this._token = token;
            _uri = uri;
            _useragent = "";
            this._pingInterval = new System.Timers.Timer()
            {
                Interval = 20000
            };
            this._pingInterval.Elapsed += new ElapsedEventHandler((object o, ElapsedEventArgs eventArgs) => this.Send(string.Format("[{{\"m\": \"t\", \"e\":\"{0}\"}}]", this.GetTime())));
            ReinitWs();
            BindEventListeners();
        }

        public long GetTime()
        {
            TimeSpan universalTime = DateTime.Now.ToUniversalTime() - this._st;
            return (long)(universalTime.TotalMilliseconds + 0.5);
        }

        public long GetSTime()
        {
            return this.GetTime() + this.ServerTimeOffset;
        }

        private void ReceiveServerTime(long t)
        {
            this.ServerTimeOffset = t - this.GetTime();
        }

        public bool IsConnected() => this._ws != null && this._ws.IsAlive;

        public bool IsConnecting() => this._ws != null && this._ws.ReadyState == WebSocketState.Connecting;

        public void ReinitWs()
        {
            _ws = new WebSocket(_uri.ToString());
            _ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            _ws.OnClose += (sender, e) => OnWsClosed(sender, e);
            _ws.OnOpen += (sender, e) => OnWsOpened(sender, e);
            _ws.OnMessage += (sender, e) => OnWsMessageReceived(sender, e);
        }

        public void Start()
        {
            ReinitWs();
            this.Connect();
        }

        public void Stop()
        {
            if (!IsConnected())
                return;
            _ws.CloseAsync();
        }

        public void Connect()
        {
            ReinitWs();
            this._ws.ConnectAsync();
        }


        public void OnWsClosed(object sender, CloseEventArgs e)
        {
            lock (this._ws)
            {
                this._pingInterval.Stop();
                this.Disconnected((object)this, new EventArgs());
                ++this._connectionAttempts;
                this.Emit("rawmsg", "CONNECTION CLOSED.  REASON: " + e.Reason);
                Task.Delay(5000).ContinueWith(_ => Connect());
            }
        }

        public void OnWsOpened(object sender, EventArgs e)
        {
            DateTime _connectionTime = new DateTime();
            if(this._token != null)
            {
                Send("[{\"m\": \"hi\",\"token\": \"" + this._token + "\"}]");
            } else
            {
                Send("[{\"m\": \"hi\"}]");
            }
            Send("[{\"m\":\"devices\",\"list\":[]}]");
            this._pingInterval.Start();
        }
        public void setChannel(string room)
        {
            if (!IsConnected())
                return;
            Send("[{\"m\": \"ch\", \"_id\":\""+room+"\"}]");
        }
        public void setName(string name)
        {
            if (!IsConnected())
                return;
            Send("[{\"m\": \"userset\", \"set\":{\"name\":\""+name+"\"}}]");
        }


        public void OnWsMessageReceived(object sender, MessageEventArgs e)
        {
            var jsonArray = JArray.Parse(e.Data);

            foreach (var msg in jsonArray)
            {
                if (msg["m"] != null)
                {
                    string messageType = msg["m"].Value<string>();
                    this.Emit(messageType, msg);
                    this.Emit("rawmsg", e.Data);
                }
            }
        }



        public void OnDynamic(string evnt, Action<dynamic> callback)
        {
            base.On(evnt, (object[] objects) => {
                if (objects.Length > 0)
                {
                    dynamic dynamicArg = objects[0] as dynamic;
                    if (dynamicArg != null)
                    {
                        callback(dynamicArg);
                    }
                }
            });
        }



        /*public void OnWsMessageReceived(object sender, MessageEventArgs e)
        {
            foreach (dynamic obj in (IEnumerable)JArray.Parse(e.Data))
            {
                dynamic obj1 = obj != (dynamic)null;
                if ((!obj1 ? obj1 == null : (obj1 & obj.m != (dynamic)null) == 0))
                {
                    continue;
                }
                this.Emit((string)obj.m, obj);
            }
        }*/

        public void Send(string raw)
        {
            if (!IsConnected())
                return;
            _ws.Send(raw);
            this.Emit("rawmsg", "CLIENT: send -> "+raw);
        }

        public void SendArray(JArray jArray)
        {
            this.Send(JsonConvert.SerializeObject(jArray));
        }

        public void say(string chat)
        {
            this.Send("[{\"m\":\"a\", \"message\":\""+chat+"\"}]");
        }
        public void BindEventListeners()
        {
            this.OnDynamic("hi", (msg) => {
                if (msg != null && msg.t != null)
                {
                    this.user = msg.u;
                    this.setChannel("lobby");
                    this.ReceiveServerTime((long)msg.t);
                }
            });
            this.OnDynamic("ch", (msg) => {
                if (msg != null && msg.ch != null)
                {
                    this.Channel = msg.ch;
                }
            });
        }
    }
}