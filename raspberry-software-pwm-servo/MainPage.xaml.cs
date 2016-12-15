using Microsoft.IoT.Lightning.Providers;
using raspberry_software_pwm_servo.Devices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace raspberry_software_pwm_servo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //private Servo2 s;
        private Servo s;

        public MainPage()
        {
            this.InitializeComponent();

            // Register for the unloaded event so we can clean up upon exit
            Unloaded += MainPage_Unloaded;

            // Set Lightning as the default provider
            if (LightningProvider.IsLightningEnabled)
                LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();

            // Initialize the sensors
            InitializeSensors();

            // Setting the DataContext
            this.DataContext = s;
        }

        private async void InitializeSensors()
        {
            try
            {
                //s = new Devices.Servo2(5);
                s = new Devices.Servo(5);

                s_1.Minimum = 0;
                s_1.Maximum = s.MAX_ANGLE;

                await s.InitializeAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Initialization has failed: " + ex);
            }
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            s.Dispose();
            s = null;
        }

        private void bt_1_Click(object sender, RoutedEventArgs e)
        {
            s.MoveServo();
        }
    }
}