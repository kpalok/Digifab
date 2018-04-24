using Android.App;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using System;
using Android.Content;
using Android.Runtime;
using Java.Util;
using System.Threading;
using Java.IO;

namespace FeederApp
{   


    [Activity(Label = "FeederApp", MainLauncher = true)]
    public class MainActivity : Activity
    {
        public Context context = null;
        public BluetoothSocket socket = null;
        static int REQUEST_ENABLE_BT = 1;
        byte[] FEED = new byte[] { byte.MaxValue  };
        byte[] CLEAR = new byte[] { 0b0 };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);
            TextView txtBtLog = FindViewById<TextView>(Resource.Id.txtBtLog);
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;

            if (adapter == null)
            {

                txtBtLog.Text = "Device doesn't support Bluetooth!";
            }
            else if (!adapter.IsEnabled)
            {
                Intent enableBtIntetent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableBtIntetent, REQUEST_ENABLE_BT);
            }
        }

        protected override void OnStart()
        {
            base.OnStart();

            TextView txtFeedLog = FindViewById<TextView>(Resource.Id.txtFeedLog);
            Button button = FindViewById<Button>(Resource.Id.btnMain);
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;

            // Register for broadcasts when a device is discovered.
            IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
            RegisterReceiver(BtReceiver, filter);

            button.Click += (o, e) =>
            txtFeedLog.Text = "Last fed: " + DateTime.Now.ToString("dd/MM hh:mm");

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            TextView txtBtLog = FindViewById<TextView>(Resource.Id.txtBtLog);

            if (requestCode == REQUEST_ENABLE_BT)
            {
                if (resultCode == Result.Ok)
                    txtBtLog.Text = "Bluetooth enabled.";

                else if (resultCode == Result.Canceled)
                    txtBtLog.Text = "Bluetooth disabled.";
            }
        }

        protected override void OnPause()
        {
            
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();

            // UnregisterReceiver(BtReceiver);

        }
        
    }

}

