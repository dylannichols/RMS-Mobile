using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;

using Android.Views;
using Android.Widget;
using Xamarin.Essentials;

namespace RMS.Activities
{
    [Activity(Label = "About the App")]
    public class About : MainActivity
    {
        public bool Auth;
        public ScrollView AboutScroll;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            // General set up
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);


            ContentMain = FindViewById<LinearLayout>(Resource.Id.contentMain);


            LayoutInflater inflater = (LayoutInflater)GetSystemService(LayoutInflaterService);

            View about = inflater.Inflate(Resource.Layout.about, null, true);
            ContentMain.AddView(about);
            AboutScroll = FindViewById<ScrollView>(Resource.Id.aboutScroll);

            Auth = Intent.Extras.GetBoolean("Auth");

            if (Auth == true)
            {
                var menu = navigationView.Menu;

                var userMenu = menu.FindItem(Resource.Id.userMenu);
                userMenu.SetVisible(true);
            }

            Button appLink = FindViewById<Button>(Resource.Id.appLink);
            appLink.Click += async (s, arg) =>
            {
                var uri = "http://13.210.251.7";
                await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            };

            Button webLink = FindViewById<Button>(Resource.Id.webLink);
            webLink.Click += async (s, arg) =>
            {
                var uri = "https://www.concar.co.nz/";
                await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            };
        }

        public override bool OnNavigationItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.logoutNav)
            {
                Intent intent = new Intent(this, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
            }
            else
            {
                AboutScroll.ScrollTo(0, 0);
            }

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.topMenu)
            {
                AboutScroll.ScrollTo(0, 0);
            }
            else if (id == Resource.Id.bottomMenu)
            {
                AboutScroll.ScrollTo(0, AboutScroll.Bottom);
            }
            else if (id == Resource.Id.logoutMenu)
            {
                Intent intent = new Intent(this, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}