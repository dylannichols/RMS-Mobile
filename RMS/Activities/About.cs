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

namespace RMS.Activities
{
    [Activity(Label = "About Us")]
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

            Auth = Intent.Extras.GetBoolean("Auth");

            if (Auth)
            {
                var menu = navigationView.Menu;

                var userMenu = menu.FindItem(Resource.Id.userMenu);
                userMenu.SetVisible(true);
            }

            ContentMain = FindViewById<LinearLayout>(Resource.Id.contentMain);


            LayoutInflater inflater = (LayoutInflater)GetSystemService(LayoutInflaterService);

            View about = inflater.Inflate(Resource.Layout.about, null, true);
            ContentMain.AddView(about);
            AboutScroll = FindViewById<ScrollView>(Resource.Id.aboutScroll);
        }

        public override bool OnNavigationItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.logoutNav)
            {
                Intent intent = new Intent(this, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
            } else { 

            }

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }
    }
}