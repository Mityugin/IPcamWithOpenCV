using System;
using System.Windows;
using System.ServiceModel.Discovery;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

namespace WebcamWithOpenCV
{
    public partial class MainWindow : Window
    {
        private IPcamStreaming _webcamStreaming;

        public MainWindow()
        {
            InitializeComponent();
            
            IPCameraList();
            cmbCameraDevices.SelectedIndex = 0;
            cameraLoading.Visibility = Visibility.Collapsed;
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            cameraLoading.Visibility = Visibility.Visible;
            webcamContainer.Visibility = Visibility.Collapsed;
            btnStop.IsEnabled = false;
            btnStart.IsEnabled = false;

            var selectedCameraDeviceId = cmbCameraDevices.SelectedItem.ToString ();
            if (_webcamStreaming == null || _webcamStreaming.CameraDeviceId != selectedCameraDeviceId)

            {
                _webcamStreaming?.Dispose();
                _webcamStreaming = new IPcamStreaming(
                    imageControlForRendering: webcamPreview,
                    frameWidth: 792,
                    frameHeight: 373,
                    cameraDeviceId: "rtsp://"+selectedCameraDeviceId+":554/user=admin_password=tlJwpbo6_channel=0_stream=0.sdp?real_stream");
            }
            
            try
            {
                await _webcamStreaming.Start();
                btnStop.IsEnabled = true;
                btnStart.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                btnStop.IsEnabled = false;
                btnStart.IsEnabled = true;
            }

            cameraLoading.Visibility = Visibility.Collapsed;
            webcamContainer.Visibility = Visibility.Visible;
        }

        private async void btnStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _webcamStreaming.Stop();
                btnStop.IsEnabled = false;
                btnStart.IsEnabled = true;

                // To save the screenshot
                // var screenshot = _webcamStreaming.LastPngFrame;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void IPCameraList()
        {
            var endPoint = new UdpDiscoveryEndpoint(DiscoveryVersion.WSDiscoveryApril2005);
            var discoveryClient = new DiscoveryClient(endPoint);

            var findCriteria = new FindCriteria
            {
                Duration = TimeSpan.FromSeconds(3),
                MaxResults = int.MaxValue
            };

            findCriteria.ContractTypeNames.Add(new XmlQualifiedName("NetworkVideoTransmitter", @"http://www.onvif.org/ver10/network/wsdl"));

            var response = discoveryClient.Find(findCriteria);
            List<string> lst = new List<string>();

            foreach (var e in response.Endpoints)
            {

                foreach (var item in e.ListenUris)
                {
                    string uri = item.OriginalString;
                    string host = item.Host;
                    lst.Add(host);

                }
            }
            lst = lst.Distinct().ToList();
            lst.ForEach(x => cmbCameraDevices.Items.Add(x));

        }

        public void Dispose()
        {
            _webcamStreaming?.Dispose();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispose();
        }
    }
}
