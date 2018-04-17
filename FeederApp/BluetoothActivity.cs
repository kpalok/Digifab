using System;
using Android.Bluetooth;
using Android.App;
using Android.Content;
using Android.Widget;
using Java.IO;
using Java.Util;
using System.Threading;

namespace FeederApp
{
    public class BluetoothActivity : Activity
    {
        static int REQUEST_ENABLE_BT = 1;
        

        public abstract class BaseThread
        {
            private Thread _thread;

            protected BaseThread()
            {
                _thread = new Thread(new ThreadStart(this.RunThread));
            }

            // Thread methods / properties
            public void Start() => _thread.Start();
            public void Join() => _thread.Join();
            public bool IsAlive => _thread.IsAlive;

            // Override in base class
            public abstract void RunThread();
        }



        [BroadcastReceiver(Enabled = true, Exported = false)]
        public class MyReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {  
                string action = intent.Action;
                if (BluetoothDevice.ActionFound.Equals(action))
                {
                    // Discovery has found a device. Get the BluetoothDevice
                    // object and its info from the Intent.
                    Toast.MakeText(context, "Received bluetooth device!", ToastLength.Short).Show();
                    BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                    Toast.MakeText(context, device.Name, ToastLength.Short).Show();
                    BluetoothAdapter.DefaultAdapter.CancelDiscovery();      
                }
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            TextView txtBtLog = FindViewById<TextView>(Resource.Id.txtBtLog);

            if (requestCode == REQUEST_ENABLE_BT)
            {
                if (resultCode == Result.Ok)
                {
                    txtBtLog.Text = "Bluetooth enabled.";
                }
            }
        }


        public class AcceptThread : BaseThread
        {
            private BluetoothServerSocket mmServerSocket;
            private Context context;

            public AcceptThread()
            {
                // Use a temporary object that is later assigned to mmServerSocket
                // because mmServerSocket is final.
                BluetoothServerSocket tmp = null;
                UUID APP_UUID = UUID.FromString("250fcceb-2f35-45d3-865c-b3807bd88960");
                try
                {
                    tmp = BluetoothAdapter.DefaultAdapter.ListenUsingRfcommWithServiceRecord("FeederApp", APP_UUID);
                }
                catch (IOException)
                {
                    Toast.MakeText(context, "Socket's listen() method failed", ToastLength.Long).Show();
                }
                mmServerSocket = tmp;
            }

            public override void RunThread()
            {
                BluetoothSocket socket = null;
                // Keep listening until exception occurs or a socket is returned.
                while (true)
                {
                    try
                    {
                        socket = mmServerSocket.Accept();
                    }
                    catch (IOException)
                    {
                        Toast.MakeText(context, "Socket's accept() method failed", ToastLength.Long).Show();
                        break;
                    }


                    if (socket != null)
                    {
                        FdBluetoothService.ConnectedThread
                        // A connection was accepted. Perform work associated with
                        // the connection in a separate thread.
                        Feeder = new FdBluetoothService.ConnectedThread(socket);
                        Feeder.RunThread();
                        mmServerSocket.Close();
                        break;
                    }
                }
            }

            public void Cancel()
            {
                try
                {
                    mmServerSocket.Close();
                }
                catch (IOException)
                {
                    Toast.MakeText(context, "Could not close the connected socket", ToastLength.Long).Show();
                }
            }


        }

    }

}




