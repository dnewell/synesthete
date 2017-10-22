using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace Synesthete
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region initializations

        private KinectSensor kinect = null;
        private ColorFrameReader colorFrameReader = null;
        private WriteableBitmap colorBitmap = null;
        private string statusText = null;


        #endregion

        public MainWindow()
        {
            this.kinect = KinectSensor.GetDefault();
            this.kinect.IsAvailableChanged += this.Sensor_IsAvailableChanged;
            this.colorFrameReader = this.kinect.ColorFrameSource.OpenReader();

            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

            FrameDescription colorFrameDescription = this.kinect.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            this.DataContext = this;
            InitializeComponent();

            this.kinect.Open();
        }

        public ImageSource ImageSource => this.colorBitmap;

        public event PropertyChangedEventHandler PropertyChanged;
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            
        }
        public string StatusText
        {
            get => this.statusText;
            set
            {
                if (this.statusText == value) return;
                this.statusText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatusText"));
            }
        }


        /// <summary>
        /// Update WritableBitmap with new frame data 15-30x per second.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // block to limit frame lifetime / handle disposing
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                // Kinect may return a null on invalid frame aquisition
                if (colorFrame == null) return;

                FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                // lock colorFrame image data buffer to prevent Kinect from sending more data
                using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                {
                    // lock and reserve WritableBitmap data buffer and prevent unwanted UI from updates
                    this.colorBitmap.Lock();
                    // check data integrity via comparison to FrameDescription property
                    if (colorFrameDescription.Width == this.colorBitmap.PixelWidth && colorFrameDescription.Height == this.colorBitmap.PixelHeight)
                    {
                        // BGRA32 is 8 bits, per-channel per-pixel (4 bytes per pixel total), so buffer size in bytes is 4 x number of pixels 
                        var numberOfPixels = (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4);

                        // copy ColorFrame data to WritableBitmap
                        colorFrame.CopyConvertedFrameDataToIntPtr(this.colorBitmap.BackBuffer, numberOfPixels, ColorImageFormat.Bgra);

                        // specify segment of WritableBitmap to change (entire area, in this case)
                        this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                    }
                    this.colorBitmap.Unlock();
                }
            }

        }


        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.colorFrameReader != null)
            {
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }
            if (this.kinect == null) return;

            this.kinect.Close();
            this.kinect = null;
        }

    }
}
