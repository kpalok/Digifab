using System.IO;
using System.Runtime.CompilerServices;
using Android.Bluetooth;
using Android.OS;
using Android.Util;
using Java.Lang;
using Java.Util;

namespace FeederApp
{
    class BluetoothFeedService
    {
        // Class has exessive logging for debugging with physical device.
        const string TAG = "BluetoothFeedService";
        ConnectThread connectThread;
        ConnectedThread connectedThread;
        int state;

        public const int STATE_NONE = 0;
        public const int STATE_CONNECTING = 1;
        public const int STATE_CONNECTED = 2;

        public BluetoothFeedService()
        {
            state = STATE_NONE;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int GetState()
        {
            return state;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Stop()
        {
            if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }
            state = STATE_NONE;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Connect(BluetoothDevice device)
        {
            if (state == STATE_CONNECTING)
            {
                if (connectThread != null)
                {
                    connectThread.Cancel();
                    connectThread = null;
                }
            }

            // Cancel any thread currently running a connection
            if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            // Start the thread to connect with the given device
            connectThread = new ConnectThread(device, this);
            connectThread.Run();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Connected(BluetoothSocket socket, BluetoothDevice device)
        {
            // Cancel the thread that completed the connection
            if (connectThread != null)
            {
                connectThread.Cancel();
                connectThread = null;
            }

            connectedThread = new ConnectedThread(socket, this);
            connectedThread.Run();
        }

        public void Write(byte[] @out)
        {
            // Create temporary object
            ConnectedThread r;
            // Synchronize a copy of the ConnectedThread
            lock (this)
            {
                if (state != STATE_CONNECTED)
                {
                    return;
                }
                r = connectedThread;
            }
            // Perform the write unsynchronized
            r.Write(@out);
        }

        protected class ConnectThread : Thread
        {
            BluetoothSocket socket;
            BluetoothDevice device;
            BluetoothFeedService service;
            
            public ConnectThread(BluetoothDevice device, BluetoothFeedService service)
            {
                this.device = device;
                this.service = service;
                BluetoothSocket tmp = null;
                ParcelUuid[] supportedUuids = device.GetUuids();
                try
                {   if (supportedUuids.Length > 0)
                    {
                        tmp = device.CreateInsecureRfcommSocketToServiceRecord(supportedUuids[0].Uuid);
                        Log.Info(TAG, "create() succeeded");
                    }
                    else
                    {
                        tmp = device.CreateInsecureRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805F9B34FB"));
                        Log.Info(TAG, "create() succeeded");
                    }
                }
                catch (Java.IO.IOException e)
                {
                    Log.Error(TAG, "create() failed", e);
                }
                socket = tmp;
                service.state = STATE_CONNECTING;
            }

            public override void Run()
            {
                base.Run();
                try
                {
                    socket.Connect();
                    Log.Info(TAG, "Connect() succeeded");
                }
                catch (Java.IO.IOException)
                {
                    // Close the socket
                    try
                    {
                        socket.Close();
                        Log.Info(TAG, "Socket closed in connect.run()!");
                    }
                    catch (Java.IO.IOException e2)
                    {
                        Log.Error(TAG, "unable to close() socket during connection failure.", e2);
                    }
                    return;
                }

                lock (this)
                {
                    service.connectThread = null;
                }
                service.Connected(socket, device);
            }

            public void Cancel()
            {
                try
                {
                    socket.Close();
                    Log.Info(TAG, "Socket closed");
                }
                catch (Java.IO.IOException e)
                {
                    Log.Error(TAG, "close() of connect socket failed", e);
                }
            }
        }

        class ConnectedThread : Thread
        {
            BluetoothSocket socket;
            Stream inStream;
            Stream outStream;
            BluetoothFeedService service;

            public ConnectedThread(BluetoothSocket socket, BluetoothFeedService service)
            {
                this.socket = socket;
                this.service = service;
                Stream tmpIn = null;
                Stream tmpOut = null;

                // Get the BluetoothSocket input and output streams
                try
                {
                    tmpIn = socket.InputStream;
                    Log.Info(TAG, "tmpIn created");
                }
                catch (Java.IO.IOException e)
                {
                    Log.Error(TAG, "temp sockets not created", e);
                }
                try
                {
                    tmpOut = socket.OutputStream;
                    Log.Info(TAG, "tmpOut created");
                }
                catch (Java.IO.IOException e)
                {
                    Log.Error(TAG, "temp sockets not created", e);
                }
                inStream = tmpIn;
                outStream = tmpOut;
                service.state = STATE_CONNECTED;
            }

            public override void Run()
            {
                // do nothing with instream
            }

            public void Write(byte[] buffer)
            {
                try
                {
                    outStream.Write(buffer, 0, buffer.Length);
                    Log.Info(TAG, "Write() succeeded");
                }
                catch (Java.IO.IOException e)
                {
                    Log.Error(TAG, "Exception during write", e);
                }
            }

            public void Cancel()
            {
                try
                {
                    socket.Close();
                    Log.Info(TAG, "Socket closed");
                }
                catch (Java.IO.IOException e)
                {
                    Log.Error(TAG, "close() of connect socket failed", e);
                }
            }
        }
    }
}