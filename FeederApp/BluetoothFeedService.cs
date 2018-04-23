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
        const string TAG = "BluetoothReedService";

        static UUID MY_UUID = UUID.FromString("250fcceb-2f35-45d3-865c-b3807bd88960");

        BluetoothAdapter btAdapter;
        Handler handler;
        AcceptThread insecureAcceptThread;
        ConnectThread connectThread;
        ConnectedThread connectedThread;
        int state;
        int newState;

        public const int STATE_NONE = 0;
        public const int STATE_LISTEN = 1;
        public const int STATE_CONNECTING = 2;
        public const int STATE_CONNECTED = 3;

        public BluetoothFeedService(Handler handler)
        {
            btAdapter = BluetoothAdapter.DefaultAdapter;
            state = STATE_NONE;
            newState = state;
            this.handler = handler;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int GetState()
        {
            return state;
        }

        public void Start()
        {
            if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            if (insecureAcceptThread == null)
            {
                insecureAcceptThread = new AcceptThread(this);
                insecureAcceptThread.Start();
            }
        }

        public void Connect(BluetoothDevice device)
        {
            if (state == STATE_CONNECTING)
            {
                if (connectedThread != null)
                {
                    connectThread.Cancel();
                    connectThread = null;
                }
            }

            if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            connectThread = new ConnectThread(device, this);
            connectThread.Start();
        }

        public void Connected(BluetoothSocket socket, BluetoothDevice device)
        {
            // Cancel the thread that completed the connection
            if (connectThread != null)
            {
                connectThread.Cancel();
                connectThread = null;
            }

            // Cancel any thread currently running a connection
            if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            if (insecureAcceptThread != null)
            {
                insecureAcceptThread.Cancel();
                insecureAcceptThread = null;
            }

            // Start the thread to manage the connection and perform transmissions
            connectedThread = new ConnectedThread(socket, this);
            connectedThread.Start();
        }

        public void Stop()
        {
            if (connectThread != null)
            {
                connectThread.Cancel();
                connectThread = null;
            }

            if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            if (insecureAcceptThread != null)
            {
                insecureAcceptThread.Cancel();
                insecureAcceptThread = null;
            }

            state = STATE_NONE;
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

        void ConnectionFailed()
        {
            state = STATE_LISTEN;
            Start();
        }

        public void ConnectionLost()
        {
            state = STATE_NONE;
            this.Start();
        }

        class AcceptThread : Thread
        {
            BluetoothServerSocket serverSocket;
            BluetoothFeedService service;

            public AcceptThread(BluetoothFeedService service)
            {
                BluetoothServerSocket tmp = null;
                this.service = service;

                try
                {
                    tmp = service.btAdapter.ListenUsingInsecureRfcommWithServiceRecord("FeederApp", MY_UUID);
                }
                catch (Java.IO.IOException e)
                {
                    Log.Error(TAG, "listen() failed", e);
                }
                serverSocket = tmp;
                service.state = STATE_LISTEN;
            }

            public override void Run()
            {
                BluetoothSocket socket = null;

                while (service.GetState() != STATE_CONNECTED)
                {
                    try
                    {
                        socket = serverSocket.Accept();
                    }
                    catch (Java.IO.IOException e)
                    {
                        Log.Error(TAG, "accept() failed", e);
                        break;
                    }
                }

                if (socket != null)
                {
                    lock (this)
                    {
                        switch (service.GetState())
                        {
                            case STATE_LISTEN:
                            case STATE_CONNECTING:
                                // Situation normal. Start the connected thread.
                                service.Connected(socket, socket.RemoteDevice);
                                break;
                            case STATE_NONE:
                            case STATE_CONNECTED:
                                try
                                {
                                    socket.Close();
                                }
                                catch (Java.IO.IOException e)
                                {
                                    Log.Error(TAG, "Could not close unwanted socket", e);
                                }
                                break;
                        }
                    }
                }
            }

            public void Cancel()
            {
                try
                {
                    serverSocket.Close();
                }
                catch (Java.IO.IOException e)
                {
                    Log.Error(TAG, "close() of server failed", e);
                }
            }
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

                try
                {
                    tmp = device.CreateInsecureRfcommSocketToServiceRecord(MY_UUID); 
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
                // Always cancel discovery because it will slow down connection
                service.btAdapter.CancelDiscovery();

                // Make a connection to the BluetoothSocket
                try
                {
                    socket.Connect();
                }
                catch (Java.IO.IOException e)
                {
                    // Close the socket
                    try
                    {
                        socket.Close();
                    }
                    catch (Java.IO.IOException e2)
                    {
                        Log.Error(TAG, "unable to close()socket during connection failure.", e2);
                    }

                    // Start the service over to restart listening mode
                    service.ConnectionFailed();
                    return;
                }

                // Reset the ConnectThread because we're done
                lock (this)
                {
                    service.connectThread = null;
                }

                // Start the connected thread
                service.Connected(socket, device);
            }

            public void Cancel()
            {
                try
                {
                    socket.Close();
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
                    tmpOut = socket.OutputStream;
                }
                catch (Java.IO.IOException e)
                {
                    Log.Error(TAG, "temp sockets not created", e);
                }

                outStream = tmpOut;
                service.state = STATE_CONNECTED;
            }

            public override void Run()
            {
                byte[] buffer = new byte[1];
                int bytes;

                // Keep listening to the InputStream while connected
                while (service.GetState() == STATE_CONNECTED)
                {
                    try
                    {
                        bytes = inStream.Read(buffer, 0, buffer.Length);
                    }
                    catch (Java.IO.IOException e)
                    {
                        Log.Error(TAG, "disconnected", e);
                        service.ConnectionLost();
                        break;
                    }
                }
            }

            public void Write(byte[] buffer)
            {
                try
                {
                    outStream.Write(buffer, 0, buffer.Length);
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
                }
                catch (Java.IO.IOException e)
                {
                    Log.Error(TAG, "close() of connect socket failed", e);
                }
            }
        }
    }
}