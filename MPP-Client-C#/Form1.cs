﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MPP_Client_C_
{
    public partial class client : Form
    {
        public Client cl = new Client(new Uri("wss://mppclone.com"), "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIwZjExMTVlNWRiODQxZjg0ZWNkMzk3YzgiLCJpYXQiOjE3MDI2NjE0MzYsImlzcyI6ImFkbWluQG1wcGNsb25lLmNvbSJ9.BAGZ3Frcp8lLsDPkUlmWviTNaPp4JK3C9e8c4kuKi3A");
        public client()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cl.Stop();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cl.Start();
            cl.OnDynamic("hi", (msg) =>
            {
                cl.setName("Csharp Client");
                cl.setChannel("The Roleplay Room");
            });
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = (cl.IsConnected()).ToString();
        }
    }
}
