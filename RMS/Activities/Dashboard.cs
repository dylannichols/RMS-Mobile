using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    [Activity(Label = "Dashboard")]
    public class Dashboard : Activity
    {
        public string Token;
        public int Node;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.dashboard);
            Token = Intent.Extras.GetString("Token");
            Node = Intent.Extras.GetInt("Node");

            var layout = FindViewById<LinearLayout>(Resource.Id.dashLayout);
            Console.WriteLine("Linear layout width: " + layout.LayoutParameters.Width);
            List<Header> headers = GetDashboard();
            foreach (Header h in headers)
            {
                TableLayout.LayoutParams layoutparams = new TableLayout.LayoutParams(TableLayout.LayoutParams.MatchParent, TableLayout.LayoutParams.WrapContent);

                var table = new TableLayout(this);
                table.LayoutParameters = layoutparams;
     
                var heading = new TableRow(this);
                var title = new TextView(this)
                {
                    Text = h.header,
                    TextSize = 20,
                    Gravity = GravityFlags.CenterHorizontal
                };

                heading.AddView(title);
                table.AddView(heading);

                foreach (DashItem item in h.items)
                {
                    TableRow.LayoutParams rowParams = new TableRow.LayoutParams(TableRow.LayoutParams.MatchParent, TableRow.LayoutParams.WrapContent);
                    var row = new TableRow(this);
                    row.LayoutParameters = rowParams;

                    var label = new TextView(this)
                    {
                        Text = item.display_detail,
                        Gravity = GravityFlags.Left
                    };

                    var value = new TextView(this)
                    {
                        Text = item.display_value,
                        Gravity = GravityFlags.Right
                    };

                    var unit = new TextView(this)
                    {
                        Text = item.display_unit,
                        Gravity = GravityFlags.Right
                    };

                    row.AddView(label);
                    row.AddView(value);
                    row.AddView(unit);

                    table.AddView(row);
                }
                layout.AddView(table);
            }
        }

        private List<Header> GetDashboard()
        {
            using (WebClient wc = new WebClient())
            {
                //wc.Headers.Set("Authorization", "Bearer " + Token);
                wc.Headers[HttpRequestHeader.Authorization] = "Bearer " + Token;
                Console.WriteLine(wc.Headers[HttpRequestHeader.Authorization]);
                string json = wc.DownloadString("http://13.210.251.7/api/dashboard/" + Node);
                JObject data = JObject.Parse(json);
                string headerJSON = data["headers"].ToString();
                string dashJSON = data["dashboard"].ToString();

                List<Header> headers = JsonConvert.DeserializeObject<List<Header>>(headerJSON);
                List<DashItem> items = JsonConvert.DeserializeObject<List<DashItem>>(dashJSON);

                foreach (DashItem item in items)
                {
                    Header header = headers.Find(x => x.header == item.header && x.sub_header == item.sub_header);
                    header.items.Add(item);
                }

                return headers;
            }
        }
    }
}