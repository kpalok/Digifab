
/*/
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

        public class AcceptThread
        {
            public BluetoothServerSocket mmServerSocket;
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
/*/



