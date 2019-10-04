using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RMS.Models;

namespace RMS.Activities
{
    [Activity(Label = "NodeSelect")]
    public class NodeSelect : Activity
    {

        public string Token;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.node_select);


            Token = Intent.Extras.GetString("Token");

            List<Node> nodes = GetNodes();
            var layout = FindViewById<LinearLayout>(Resource.Id.nodeLayout);
            foreach (Node n in nodes)
            {
                Console.WriteLine(n.node_name);
                var button = new Button(this)
                {
                    Text = n.node_name,
                    Id = n.idx
                };

                layout.AddView(button);
            }
        }

        private List<Node> GetNodes()
        {
            using (WebClient wc = new WebClient())
            {
                //wc.Headers.Set("Authorization", "Bearer " + Token);
                wc.Headers[HttpRequestHeader.Authorization] = "Bearer " + Token;
                Console.WriteLine(wc.Headers[HttpRequestHeader.Authorization]);
                string json = wc.DownloadString("http://13.210.251.7/api/nodes");
                JObject data = JObject.Parse(json);
                string nodeJson = data["data"].ToString();
                Console.WriteLine(nodeJson);

                List<Node> nodes = JsonConvert.DeserializeObject<List<Node>>(nodeJson);
                Console.WriteLine("Nodes: " + nodes.Count);
                return nodes;
            }
        }
    }
}