using Android.App;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using System;
using Android.Content;
using Android.Runtime;
using Java.Util;
using System.Threading;

namespace FeederApp
{   


    [Activity(Label = "FeederApp", MainLauncher = true)]
    public class MainActivity : Activity
    {
        static int REQUEST_ENABLE_BT = 1;
        byte[] WriteStream = new byte[] { 0b0 , byte.MaxValue  };

        BroadcastReceiver BtReceiver = new BluetoothActivity.MyReceiver();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            TextView txtFeedLog = FindViewById<TextView>(Resource.Id.txtFeedLog);
            TextView txtBtLog = FindViewById<TextView>(Resource.Id.txtBtLog);
            Button ButtonMain = FindViewById<Button>(Resource.Id.btnMain);

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

            else
            {
                adapter.StartDiscovery();

                // Register for broadcasts when a device is discovered.
                IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
                RegisterReceiver(BtReceiver, filter);

                ButtonMain.Click += (o, e) =>
                txtFeedLog.Text = "Last fed: " + DateTime.Now.ToString("dd/MM hh:mm");

            }
        }   
      
        protected override void OnDestroy()
        {
            base.OnDestroy();

            UnregisterReceiver(BtReceiver);

        }
        
    }

}

