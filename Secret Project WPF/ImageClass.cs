using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Secret_Project_WPF
{
    /// <summary>
    /// Stores the pictures on each question
    /// </summary>
    public class ImageClass
    {
        /// <summary>
        /// A Control to store the Windows.Forms PictureBox object
        /// </summary>
        public WindowsFormsHost wfh { get; set; }

        /// <summary>
        /// Windows.Forms PictureBox object containing the image itself
        /// </summary>
        public System.Windows.Forms.PictureBox picBox { get; set; }

        /// <summary>
        /// A constructor which initializes the objects
        /// and sets alignment, size options and some other properties
        /// </summary>
        public ImageClass()
        {
            wfh = new WindowsFormsHost();
            wfh.HorizontalAlignment = HorizontalAlignment.Left;
            wfh.VerticalAlignment = VerticalAlignment.Top;

            picBox = new System.Windows.Forms.PictureBox();
            wfh.Child = picBox;

            SetSize(0, 0);
            wfh.Visibility = Visibility.Hidden;
            picBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
        }

        public static void InitImageWindow()
        {
            imageWindow = new Window();
            imageWindow.ResizeMode = System.Windows.ResizeMode.NoResize;
        }

        /// <summary>
        /// Sets the size of the PictureBox, as well as the WindowsFormsHost objects
        /// to the given width and height integers
        /// </summary>
        /// <param name="widht"></param>
        /// <param name="height"></param>
        public void SetSize(int widht, int height)
        {
            wfh.Width = widht;
            picBox.Width = widht;
            wfh.Height = height;
            picBox.Height = height;
        }

        /// <summary>
        /// Sets Visibility to true
        /// </summary>
        public void Show()
        {
            wfh.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Sets Visibility to false
        /// </summary>
        public void Hide()
        {
            wfh.Visibility = Visibility.Hidden;
        }

        public delegate void MethodsToExecute();
        public static MethodsToExecute pictureOpenMethods = null;
        public static MethodsToExecute pictureCloseMethods = null;

        /// <summary>
        /// The window that opens when you click on an image
        /// </summary>
        private static Window imageWindow;

        /// <summary>
        /// Used with the handles for MouseDown and MouseUp
        /// </summary>
        private static DateTime downTime;

        /// <summary>
        /// Sets the handle for the imageWindow's Closing event
        /// </summary>
        /// <param name="handle"></param>
        public static void SetWinClosingHandle(System.ComponentModel.CancelEventHandler handle)
        {
            imageWindow.Closing += handle;
        }

        /// <summary>
        /// Calls the imageWindow's Close() method
        /// </summary>
        /// <param name="handle"></param>
        public static void CloseWindow()
        {
            if(imageWindow != null) imageWindow.Close();
        }

        /// <summary>
        /// Handler for mouseDown event of the picture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void picBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                downTime = DateTime.Now;
        }

        /// <summary>
        /// Handler for mouseUp event of the picture in each question
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void picBox_MouseUp(object sender,
                                   System.Windows.Forms.MouseEventArgs e)
        {
            TimeSpan timeSinceDown = DateTime.Now - downTime;
            if (timeSinceDown.TotalMilliseconds >= 500 ||
                e.Button != System.Windows.Forms.MouseButtons.Left ||
                this.picBox.Image == null)
            {
                return;
            }
            this.Hide();

            System.Drawing.Image img = this.picBox.Image;
            this.picBox.Image = null;

            System.Windows.Forms.PictureBox l_picBox = new System.Windows.Forms.PictureBox();
            l_picBox.Image = img;
            l_picBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;

            imageWindow.Height = 500D;
            double imageRatio = (double)(img.Width) / img.Height;
            l_picBox.Height = (int)imageWindow.Height;
            l_picBox.Width = (int)(imageRatio * l_picBox.Height);
            imageWindow.Width = l_picBox.Width;

            WindowsFormsHost wfh = new WindowsFormsHost();
            wfh.Child = l_picBox;
            imageWindow.Content = wfh;


            //Set the image window's icon
            System.Drawing.Icon iconHandle = Properties.Resources.icon;
            System.Drawing.Bitmap bitmap = iconHandle.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap =
                 System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                      hBitmap, IntPtr.Zero, Int32Rect.Empty,
                      BitmapSizeOptions.FromEmptyOptions());
            imageWindow.Icon = wpfBitmap;

            imageWindow.Closing += imageWindow_Closing;
            //Show the image window
            imageWindow.Show();
            if(pictureOpenMethods != null) pictureOpenMethods();
            pictureOpenMethods = null;
        }

        public void imageWindow_Closing(object sender,
                                        System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            (sender as Window).Closing -= imageWindow_Closing;
            (sender as Window).Hide();

            System.Windows.Forms.PictureBox picBox = (((sender as Window).Content as WindowsFormsHost).Child as System.Windows.Forms.PictureBox);
            System.Drawing.Image img = picBox.Image;
            picBox.Image = null;
            this.picBox.Image = img;
            this.Show();
            if (pictureCloseMethods != null) pictureCloseMethods();
            pictureCloseMethods = null;
        }
    }
}
