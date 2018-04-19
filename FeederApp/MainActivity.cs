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

        BroadcastReceiver BtReceiver = new BluetoothActivity.MyReceiver();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);
            TextView txtFeedLog = FindViewById<TextView>(Resource.Id.txtFeedLog);
            TextView txtBtLog = FindViewById<TextView>(Resource.Id.txtBtLog);
            Button button = FindViewById<Button>(Resource.Id.btnMain);
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;

            if (adapter == null)
            {

                txtBtLog.Text = "Device doesn't support Bluetooth!";
            }
            else if (!adapter.IsEnabled)
            {
                txtBtLog.Text = "Bluetooth disabled";
                Intent enableBtIntetent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableBtIntetent, REQUEST_ENABLE_BT);
            }
            else
            {
                adapter.StartDiscovery();

                // Register for broadcasts when a device is discovered.
                IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
                RegisterReceiver(BtReceiver, filter);
                BluetoothActivity.AcceptThread accept = new BluetoothActivity.AcceptThread();

                while (true)
                {
                    try
                    {
                        socket = accept.mmServerSocket.Accept();
                    }
                    catch (IOException)
                    {
                        Toast.MakeText(context, "Socket's accept() method failed", ToastLength.Long).Show();
                        break;
                    }
                    if (socket != null)
                    {
                        break;
                    }
                }

                ConnectedThread connected = new ConnectedThread(socket);

                button.Click += (o, e) =>
                connected.Write(FEED);
                Thread.Sleep(TimeSpan.FromSeconds(5));
                connected.Write(CLEAR);
                txtFeedLog.Text = "Last fed: " + DateTime.Now.ToString("dd/MM hh:mm");     
            }
        }

        public class ConnectedThread
        {
            private BluetoothSocket mmSocket;
            private System.IO.Stream mmInStream;
            private System.IO.Stream mmOutStream;
            private Context context;

            public ConnectedThread(BluetoothSocket socket)
            {
                mmSocket = socket;
                System.IO.Stream tmpIn = null;
                System.IO.Stream tmpOut = null;

                try
                {
                    tmpIn = socket.InputStream;
                }
                catch (IOException)
                {
                    Toast.MakeText(context, "Error occured when creating input stream", ToastLength.Long).Show();
                }
                try
                {
                    tmpOut = socket.OutputStream;
                }
                catch (IOException)
                {
                    Toast.MakeText(context, "Error occured when creating output stream", ToastLength.Long).Show();
                }

                mmInStream = tmpIn;
                mmOutStream = tmpOut;
            }

            public void Write(byte[] bytes)
            {
                try
                {
                    mmOutStream.Write(bytes, 0, 1);
                }
                catch (IOException)
                {
                    Toast.MakeText(context, "Error occured when sending data", ToastLength.Long).Show();
                }
            }

            public void Cancel()
            {
                try
                {
                    mmSocket.Close();
                }
                catch (IOException)
                {
                    Toast.MakeText(context, "Could not close the connect socket", ToastLength.Long).Show();
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnregisterReceiver(BtReceiver);
        }  
    }
}
