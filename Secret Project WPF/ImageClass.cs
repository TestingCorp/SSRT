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
        public WindowsFormsHost wfh { get; set; }
        public System.Windows.Forms.PictureBox picBox { get; set; }
        public String filePath { get; set; }
        //public bool ImageOpened { get; set; }
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
        public void SetSize(int widht, int height)
        {
            wfh.Width = widht;
            picBox.Width = widht;
            wfh.Height = height;
            picBox.Height = height;
        }
        public void Show()
        {
            wfh.Visibility = Visibility.Visible;

        }
    }
}
