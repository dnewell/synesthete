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

        private KinectSensor _kinect = null;
        private ColorFrameReader _colorFrameReader = null;
        private WriteableBitmap _colorBitmap = null;
        private string _statusText = null;

        #endregion

        public MainWindow()
        {
            this._kinect = KinectSensor.GetDefault();
            this._kinect.IsAvailableChanged += this.Sensor_IsAvailableChanged;
            this._colorFrameReader = this._kinect.ColorFrameSource.OpenReader();

            this._colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

            FrameDescription colorFrameDescription = this._kinect.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            this._colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            this.DataContext = this;
            InitializeComponent();

            this._kinect.Open();
        }

        public ImageSource ImageSource => this._colorBitmap;

        public event PropertyChangedEventHandler PropertyChanged;
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            
        }
        public string StatusText
        {
            get => this._statusText;
            set
            {
                if (this._statusText == value) return;
                this._statusText = value;
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
                    this._colorBitmap.Lock();
                    // check data integrity via comparison to FrameDescription property
                    if (colorFrameDescription.Width == this._colorBitmap.PixelWidth && colorFrameDescription.Height == this._colorBitmap.PixelHeight)
                    {
                        // BGRA32 is 8 bits, per-channel per-pixel (4 bytes per pixel total), so buffer size in bytes is 4 x number of pixels 
                        var numberOfPixels = (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4);

                        // copy ColorFrame data to WritableBitmap
                        colorFrame.CopyConvertedFrameDataToIntPtr(this._colorBitmap.BackBuffer, numberOfPixels, ColorImageFormat.Bgra);

                        // specify segment of WritableBitmap to change (entire area, in this case)
                        this._colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this._colorBitmap.PixelWidth, this._colorBitmap.PixelHeight));
                    }
                    this._colorBitmap.Unlock();
                }
            }

        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this._colorFrameReader != null)
            {
                this._colorFrameReader.Dispose();
                this._colorFrameReader = null;
            }
            if (this._kinect == null) return;

            this._kinect.Close();
            this._kinect = null;
        }

    }
}
