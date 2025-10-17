using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
namespace Server
{
    internal class Program
    {
        static void Main()
        {

            /*
            TODO:
            Be able to handle disconnects from PCS GUI
            */

            #region "Global" Variables
            // Can be modified to guarantee a maximum sampling time of once per <value>.
            // Real sampling time would be slightly longer due to computation.
            int limitSamplingRate = 3000; 
            // Button controllers
            GpioController controllerBtn1;
            GpioController controllerBtn2;
            GpioController controllerBtn3;
            GpioController controllerBtn4;

            // Pins, GPIO 25, 8, 7, 1
            int buttonPin1 = 22;
            int buttonPin2 = 24;
            int buttonPin3 = 26;
            int buttonPin4 = 28;

            // Connection
            IPAddress ipAddress;
            IPEndPoint localEndPoint;
            Socket? listener = null;
            Socket? handler = null;
            bool connected = false;
            #endregion

            #region Initialize Pins
            // Commented out for testing
            /**/
            controllerBtn1 = new GpioController(PinNumberingScheme.Board);
            controllerBtn2 = new GpioController(PinNumberingScheme.Board);
            controllerBtn3 = new GpioController(PinNumberingScheme.Board);
            controllerBtn4 = new GpioController(PinNumberingScheme.Board);

            controllerBtn1.OpenPin(buttonPin1, PinMode.Input);
            controllerBtn2.OpenPin(buttonPin2, PinMode.Input);
            controllerBtn3.OpenPin(buttonPin3, PinMode.Input);
            controllerBtn4.OpenPin(buttonPin4, PinMode.Input);
            /**/
            Console.WriteLine("Initalization done");
            #endregion

            #region Start Server
            // Get Host IP Address that is used to establish a connection
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1
            // If a host has multiple addresses, you will get a list of addresses
            //IPHostEntry host = Dns.GetHostEntry("localhost");
            //IPAddress ipAddress = host.AddressList[0];

            // Real IP for raspberry pi
            ipAddress = IPAddress.Parse("169.254.88.165");
            
            // Testing locally
            //ipAddress = IPAddress.Parse("127.0.0.1");

            localEndPoint = new IPEndPoint(ipAddress, 11800);
            try
            {
                // Create a Socket that will use Tcp protocol
                listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // A Socket must be associated with an endpoint using the Bind method
                listener.Bind(localEndPoint);

                // This server will only need to connect to our individual computer.
                listener.Listen(1);
                Console.WriteLine("Waiting for a connection...");
                handler = listener.Accept();
                connected = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            #endregion

            while (true)
            {
                try
                {

                    #region Read Measurements, Raise Alarm and Send Measurements
                    // Figure out values to return
                    string sendString;

                    // Comment to test code
                    if (controllerBtn1.Read(buttonPin1) == PinValue.High)
                    
                    // Testing line
                    //if(true)
                    {
                        // Rest of RaiseAlarm and SendMeasurements in this if statment.
                        // Cause if it returns a low pin value, the alarms are off anyways.
                        bool alarm = false;
                        sendString = "1";
                        
                        // Comment to test code
                        /**/
                        if (controllerBtn2.Read(buttonPin2) == PinValue.High) { sendString += "1"; alarm = true; }
                        else { sendString += "0"; }
                        if (controllerBtn3.Read(buttonPin3) == PinValue.High) { sendString += "1"; alarm = true; }
                        else { sendString += "0"; }
                        if (controllerBtn4.Read(buttonPin4) == PinValue.High) { sendString += "1"; alarm = true; }
                        else { sendString += "0"; }
                        /**/
                        sendString += "101";
                        Console.WriteLine("Sending this message: " + sendString);

                        if (alarm)
                        {
                            Console.WriteLine("raisin' an alarm!!1!");
                        }


                        // Get command to read and send values
                        string data = null;
                        byte[] bytes = new byte[1024];

                        Console.WriteLine("Waiting to receive");
                        int bytesRec = handler.Receive(bytes);

                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("True") == 0)
                        {
                            Console.WriteLine("test");
                            // Send values
                            byte[] msg = Encoding.ASCII.GetBytes(sendString);
                            Console.WriteLine("Sending message " + sendString);
                            if (handler != null)
                            {
                                handler.Send(msg);
                                Thread.Sleep(limitSamplingRate);
                            }
                            else
                            {
                                Thread.Sleep(limitSamplingRate);
                            }
                        }
                        else { Console.WriteLine("message not received i guess"); }
                    }
                    else
                    {
                        Console.WriteLine("Alarms are turned off, no data being sent");
                        Thread.Sleep(limitSamplingRate);
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString() + Environment.NewLine);
                    break;
                }
            }
            // Connection has been broken
            handler.Close();
            listener.Close();
            // Restart program, try to establish a new connection.
            Main();

        }
    }
}