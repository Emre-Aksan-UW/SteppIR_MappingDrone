﻿//CREDIT: desktop link code is heavily reused from Stefan Wick's UWP with Desktop Extension tutorial
///https://stefanwick.com/2018/04/16/uwp-with-desktop-extension-part-3/

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace FullTrust
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppServiceConnection connection = null;
        bool sessionFlag = false;
        public MainWindow()
        {
            InitializeComponent();
            InitializeAppServiceConnection();
        }

        /// <summary>
        /// Open connection to UWP app service
        /// </summary>
        private async void InitializeAppServiceConnection()
        {
            connection = new AppServiceConnection();
            connection.AppServiceName = "SampleInteropService";
            connection.PackageFamilyName = Package.Current.Id.FamilyName;
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;

            AppServiceConnectionStatus status = await connection.OpenAsync();
            if (status != AppServiceConnectionStatus.Success)
            {
                // something went wrong ...
                MessageBox.Show(status.ToString());
                this.IsEnabled = false;
            }
        }

        /// <summary>
        /// Handles the event when the app service connection is closed
        /// </summary>
        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            // connection to the UWP lost, so we shut down the desktop process
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                Application.Current.Shutdown();
            })); 
        }

        /// <summary>
        /// Handles the event when the desktop process receives a request from the UWP app
        /// </summary>
        private async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            string data = "TEST DATA\n";
            ValueSet response = new ValueSet();

            try
            {
                ////Start session with spec analyzer
                var session = (Ivi.Visa.IMessageBasedSession)
                Ivi.Visa.GlobalResourceManager.Open("USB0::0x1AB1::0x0960::DSA8A221700409::INSTR");

                //Ask for value at marker
                session.FormattedIO.WriteLine("CALC:MARK:Y?");
                //Get Response
                data = session.FormattedIO.ReadLine();
                session.Dispose();
            }
            catch
            {
                data = "BAD DATA\n";
            }

            //send to UWP app
            response.Add("Magnitude", data);
            await args.Request.SendResponseAsync(response);
        }
    }
}
