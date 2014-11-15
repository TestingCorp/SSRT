using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;

namespace Secret_Project_WPF
{
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
        /// A path to the image as a file
        /// </summary>
        public String filePath { get; set; }

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

        /// <summary>
        /// Sets the size of the PictureBox, as well as the WindowsFormsHost objects
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
    }
}
