﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Tizen;
using Tizen.Applications;
using Tizen.Multimedia;
using Tizen.Network.Bluetooth;
using Tizen.Network.WiFi;
using Tizen.System;
using Tizen.Uix.Tts;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;
using Xamarin.Forms.Xaml;

namespace GreentegCoreApp1
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CoreView : ContentPage
    {
        Settings_t set = new Settings_t();
        bool started = false;
        double offset = 0.08661417322834646;
        BluetoothLeDevice device = null;
        BluetoothGattClient client = null;
        Vibrator vibr = null;
        bool sport_enabled = false;
        BluetoothGattCharacteristic characteristic = null;
        int servicesl = 0;
        bool farenheit = false;
        List<BluetoothGattService> services = new List<BluetoothGattService>();
        Timer tmr = null;
        async void BlinkAct()
        {
            DebugBlinkerLED.BackgroundColor = Color.Green;
            Task.Delay(1000);
            DebugBlinkerLED.BackgroundColor = Color.Red;
        }
        void TMR_INTV(object sender,EventArgs e)
        {
            tmr.Interval = 1000;
            tmr.Start();
            this.TimeLBL.Text = DateTime.Now.ToString("HH:mm");
        }
        async void WaiterWatcher()
        {
            int time = 30;
            while (!started)
            {
                Task.Delay(1000);
                time--;
                if(time < 0)
                {
                        if
                        (!started) {
                        WaiterReaderConnecter();
                        break;
                    }
                }
            }
        }
        public CoreView(BluetoothLeDevice device)
        {
            
            InitializeComponent();
            
            if (Variables.ServerMode)
            {
                byte[] data = { 50 };
                WebRequest request = WebRequest.Create(Variables.URL);
                request.Method = "POST";
                request.GetRequestStream().Write(data, 0, data.Length);
                request.GetRequestStream().Close();
                GC.SuppressFinalize(request);
                GC.Collect();
            }
            if (device != null)
            {
                //4WaiterX60(   ;
                LookWhatYouMadeMeDo(device);
            }
            else
            {
                
            }
        }
        async void rec_mode()
        {
            await WaitFlag();
            ComputeTemp(190);
        }
        void LookWhatYouMadeMeDo(BluetoothLeDevice device)
        {
            tmr = new Timer(1000);
            tmr.Elapsed += TMR_INTV;
            tmr.Start();
            Sport_modebtn.Clicked += Sport_modebtn_Clicked;
            try
            {
                vibr = Vibrator.Vibrators.FirstOrDefault();
            }
            catch { }

            if (device != null)
            {
                this.device = device;
                client = device.GattConnect(false);
                //Connect.Clicked += Connect_Clicked;
                WaiterReaderConnecter();
            }
            try
            {
                if (Variables.debug_mode)
                {
                    ComputeTemp(250);
                }
            }
            catch { }
        }

        private void Sport_modebtn_Clicked(object sender, EventArgs e)
        {
            sport_enabled = !sport_enabled;
            if (!sport_enabled)
            {
                Sport_modebtn.BackgroundColor = Color.Green;
                
            }
            else
            {
                Sport_modebtn.BackgroundColor = Color.LightGreen;
                
            }
        }

        private void Connect_Clicked(object sender, EventArgs e)
        {
            GC.Collect();
            Reader();
        }
        void Panic()
        {
            VIBRATEMOTOR();
            WavPlayer.StartAsync(ResourcePath.GetPath("your_temperature_is_high.wav"), new AudioStreamPolicy(AudioStreamType.Alarm));

        }

        private void Client_StateChanged(object sender, Tizen.Uix.Tts.StateChangedEventArgs e)
        {
            if(e.Current == State.Ready)
            {
                
            }
        }

        async void VIBRATEMOTOR()
        {
            if (vibr != null) vibr.Vibrate(1000, 100);
        }
        async void WaiterReaderConnecter()
        {
            await WaitFlag();
            //WaiterWatcher();
            try
            {
                foreach (BluetoothGattService serv in client.GetServices())
            {
                /*if(serv.Uuid != 0x1809)
                {

                }*/
                services.Add(serv);
                if (/*serv.Uuid.StartsWith(Variables.serviceuuid_start)*/true)
                {
                    foreach (BluetoothGattCharacteristic chara in serv.GetCharacteristics())
                    {
                            Tizen.Log.Info("COREUUID", "COREUUID=" + chara.Uuid);
                        if (chara.Uuid.ToLower().StartsWith(Variables.characteristicuuid_start))
                        {
                                DataLBL.FontSize = 10;
                            DataLBL.Text = "Waiting for response";
                            characteristic = chara;
                            characteristic.ValueChanged += Characteristic_ValueChanged;


                                //DisplayAlert("OK", "OK", "OK");
                        }
                            GC.Collect();
                   
                    }
                }
                
                
               // DataLBL.Text = serv.Uuid;
            }
           
               
            }
            catch {if(characteristic != null) DisplayAlert("Error", "Error", "Error"); else DisplayAlert("Error", "Is Null ?", "Retry");WaiterReaderConnecter(); }
            //DataLBL.Text = "Done";
        }
        async void KeeperUpper(BluetoothGattService srv)
        {
            while (true)
            {
                     }
        }
        async void WaiterX60()
        {
            await Task.Delay(20000);
            try
            {
                if (!started)
                {
                    device.GattDisconnect();
                    LookWhatYouMadeMeDo(device);
                }
            }
            catch(Exception ex) { DisplayAlert("Error", ex.ToString(),"OK"); }
        }
        private void Characteristic_ValueChanged(object sender, Tizen.Network.Bluetooth.ValueChangedEventArgs e)
        {
            GC.Collect();
            DataLBL.FontSize = 40;
            BlinkAct();
            started = true;
            //filesystemX.Write_Temp(e.Value[1]);
            Tizen.Log.Info("CORE", "Core_Val=" + Bytepacker.pack(e.Value));
            //DisplayAlert("OKKKKKK", "OKKKKKKKKKK", "OK");
            ComputeTemp(e.Value[1]);
            if (Variables.ServerMode)
            {
                byte[] data = { e.Value[1] };
                WebRequest request = WebRequest.Create(Variables.URL);
                request.Method = "POST";
                request.GetRequestStream().Write(data, 0, data.Length);
                request.GetRequestStream().Close();
                GC.SuppressFinalize(request);
                GC.Collect();
            }
        }
        void ComputeTemp(byte temp)
        {
            double calc = 0;
            if (Variables.inverse_temp)
            {
                calc = offset * (Variables.max_temp_val - temp);
            }
            else
            {
                calc = offset * temp;
            }

            float temp_calc = MathF.Round((float)(calc + 20), 1);
            //handler.WriteTemp((double)temp_calc);
            this.DataLBL.Text = temp_calc.ToString() + (set.f ? "F°" : "C°");
            if (!sport_enabled && temp_calc > 38)
            {
                this.DataLBL.TextColor = Color.Red;
                Panic();
            }
            else if (sport_enabled && temp_calc > 40)
            {
                this.DataLBL.TextColor = Color.Red;
                Panic();
            }
            else
            {
                this.DataLBL.TextColor = Color.Green;
            }
            
            GC.Collect();
        }
        async void Reader()
        {
        }
        async Task WaitFlag()
        {
            int sec = 10;
            while (true)
            {
                await Task.Delay(1000);
                sec--;
                this.DataLBL.FontSize = 10;
                this.DataLBL.Text = "Waiting "+sec.ToString();
                if (sec == 0)
                {
                    
                    break;
                }
                
            }
        }
        private void Disconnect_Clicked(object sender, EventArgs e)
        {
            GC.Collect();
            device.GattDisconnect();
            device = null;
        }

        private void SETTTBTN_Clicked(object sender, EventArgs e)
        {
            GC.Collect();
            Navigation.PushModalAsync(new SettingsPage(device,ref set));
        }
    }
}