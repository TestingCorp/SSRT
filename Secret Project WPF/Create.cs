﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Input;
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

    public partial class MainWindow : Window
    {
        /// <summary>
        /// While creating a test when the last tab AddQuestion is selected this method creates and adds a new question to the TabControl
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CreateTabItemAdd_GotFocus(object sender, RoutedEventArgs e)
        {
            //AddQuestion(ref tabControl, ManageStringLines("How longdlkasmdv;alsmd;lcakmds ;lakmdsc lakmds;clkasdlcmaslkm asdc?How lpo nmklmlmlkkd v;alsmd;lcakmds ;lakmdsc lakmds;clkasdlcmaslkm asdc?"), new string[] { "No", "Idea", "What", "To do" });
            g_lQCQuestions.AddQuestionIfNotExist();
            TabItem ti = new TabItem();
            ti.Content = CreateAddQuestion(new string[] { "[отговор 1]", "[отговор 2]", "[отговор 3]", "[отговор 4]" });
            ti.Header = String.Format("Въпрос {0}", (g_lTITabs.Count - 1 <= 1) ? 1 : (g_lTITabs.Count - 1));
            ti.GotFocus += CreateTabItem_GotFocus;
            (ti.Content as Grid).Width = window1.ActualWidth;
            g_lTITabs.Add(ti);
            g_lTITabs.Remove(sender as TabItem);
            g_lTITabs.Add(sender as TabItem);
            ResizeAndAdjust();
            //tabControl.Items.Remove(sender);
            //tabControl.Items.Add(sender);
            tabControl.SelectedIndex = tabControl.Items.Count - 2;

        }
        /// <summary>
        /// Creates a grid with a question with all its parts - textBoxes and Buttons.
        /// </summary>
        /// <param name="tc"></param>
        /// <param name="asAnswers"></param>
        Grid CreateAddQuestion(string[] asAnswers)
        {
            Grid gr = new Grid();
            gr.Width = tabControl.Width;
            //gr.Background = Brushes.Aqua;
            //gr.Height = tabControl.Height-40;
            gr.Margin = new Thickness(0, -5, 0, 0);

            Button btReady = new Button();
            AutomationProperties.SetAutomationId(btReady, "button_ready");
            btReady.HorizontalAlignment = HorizontalAlignment.Left;
            btReady.VerticalAlignment = VerticalAlignment.Top;
            btReady.Width = 87;
            btReady.Height = 25;
            //btReady.Margin = new Thickness(tabControl.Width - 115, tabControl.Height - 70, 0, 0);
            btReady.Content = "Готов съм";
            btReady.Click += CreateButtonReady_Click;
            gr.Children.Add(btReady);

            Button btRemove = new Button();
            AutomationProperties.SetAutomationId(btRemove, "button_remove");
            btRemove.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btRemove.VerticalAlignment = VerticalAlignment.Top;
            btRemove.Width = 87;
            btRemove.Height = 25;
            //btRemove.Margin = new Thickness(tabControl.Width - 115 - 87 - 10, tabControl.Height - 70, 0, 0);
            btRemove.Content = "Изтриване";
            btRemove.Click += btRemove_Click;
            gr.Children.Add(btRemove);

            TextBox tbQuestion = new TextBox();
            AutomationProperties.SetAutomationId(tbQuestion, "textbox_question");
            tbQuestion.HorizontalAlignment = HorizontalAlignment.Left;
            tbQuestion.VerticalAlignment = VerticalAlignment.Top;
            tbQuestion.AcceptsReturn = true;
            tbQuestion.TextWrapping = TextWrapping.Wrap;
            //tbQuestion.Width = tabControl.Width-30-150;
            tbQuestion.Height = 50;
            tbQuestion.Margin = new Thickness(10, 8, 0, 0);
            tbQuestion.Text = "[въпрос]";
            tbQuestion.GotFocus += CreateTextBox_GotFocus;
            tbQuestion.LostFocus += CreateTextBoxQuestion_LostFocus;
            gr.Children.Add(tbQuestion);

            List<TextBox> ltbAnswers = new List<TextBox>();
            g_l2rbAnswers.Add(new List<RadioButton>());
            for (int i = 0; i < Math.Min(asAnswers.Count(), 4); i++)
            {
                ltbAnswers.Add(new TextBox());
                int nLastIndex = ltbAnswers.Count - 1;

                AutomationProperties.SetAutomationId(ltbAnswers[nLastIndex], "textbox_answer");
                ltbAnswers[nLastIndex].Text = asAnswers[i];
                ltbAnswers[nLastIndex].HorizontalAlignment = HorizontalAlignment.Left;
                ltbAnswers[nLastIndex].VerticalAlignment = VerticalAlignment.Top;
                ltbAnswers[nLastIndex].Height = 20;
                //ltbAnswers[nLastIndex].Width = tabControl.Width - 30 -150-13;
                ltbAnswers[nLastIndex].Margin = new Thickness(13 + 10, 70 + i * 25, 0, 0);
                ltbAnswers[nLastIndex].GotFocus += CreateTextBox_GotFocus;
                ltbAnswers[nLastIndex].LostFocus += CreateTextBoxAnswer_LostFocus;
                gr.Children.Add(ltbAnswers[nLastIndex]);

                nLastIndex = g_l2rbAnswers.Count - 1;
                g_l2rbAnswers[nLastIndex].Add(new RadioButton());
                g_l2rbAnswers[nLastIndex][i].Width = 13;
                g_l2rbAnswers[nLastIndex][i].Height = 13;
                g_l2rbAnswers[nLastIndex][i].Margin = new Thickness(10, 73 + i * 25, 0, 0);
                g_l2rbAnswers[nLastIndex][i].HorizontalAlignment = HorizontalAlignment.Left;
                g_l2rbAnswers[nLastIndex][i].VerticalAlignment = VerticalAlignment.Top;
                g_l2rbAnswers[nLastIndex][i].Checked += CreateRadioButtonAnswer_Checked;
                g_l2rbAnswers[nLastIndex][i].Unchecked += CreateRadioButtonAnswer_Unchecked;
                gr.Children.Add(g_l2rbAnswers[nLastIndex][i]);

                if (i == Math.Min(asAnswers.Count(), 4) - 1)
                {
                    TextBox tbPoints = new TextBox();
                    AutomationProperties.SetAutomationId(tbPoints, "textbox_points");
                    tbPoints.Width = 87;
                    tbPoints.Height = 25;
                    tbPoints.HorizontalAlignment = HorizontalAlignment.Left;
                    tbPoints.VerticalAlignment = VerticalAlignment.Top;
                    tbPoints.Text = "[брой точки]";
                    //tbPoints.Margin = new Thickness(10, tabControl.Height - 70, 0, 0);
                    tbPoints.GotFocus += CreateTextBox_GotFocus;
                    tbPoints.LostFocus += CreateTextBoxPoints_LostFocus;
                    gr.Children.Add(tbPoints);
                }
            }

            Button browse = new Button();
            AutomationProperties.SetAutomationId(browse, "button_browse");
            browse.HorizontalAlignment = HorizontalAlignment.Left;
            browse.VerticalAlignment = VerticalAlignment.Top;
            browse.Width = 95;
            browse.Height = 25;
            browse.Content = "+Изображение";
            browse.Click += new RoutedEventHandler(browse_Click);
            gr.Children.Add(browse);

            g_lICImages.Add(new ImageClass());
            g_lICImages[g_nCurrQuestion].picBox.MouseDown += picBox_MouseDown;
            g_lICImages[g_nCurrQuestion].picBox.MouseUp += picBox_MouseUp;
            //WindowsFormsHost wfh = new WindowsFormsHost();

            /*wfh.Width = tabControl.ActualWidth - 350;
            wfh.Height = tabControl.ActualHeight - 95;*/

            //wfh.Margin = new Thickness(tabControl.Width - wfh.Width - 15, 9, 0, 0);
            //wfh.Child = g_lICImages[g_lICImages.Count - 1].picBox;
            gr.Children.Add(g_lICImages[g_lICImages.Count - 1].wfh);
            return gr;
        }

        void browse_Click(object sender, RoutedEventArgs e)
        {
            var browseBut = sender as Button;
            if (browseBut.Content.Equals("-Изображение"))
            {
                if (MessageBox.Show(String.Format("В момента имате добавено изображение! Сигурни ли сте, че желаете да го изтриете?"), "Внимание!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    //g_lICImages[g_nCurrQuestion].img.Source = null;
                    g_lICImages[g_nCurrQuestion].picBox.Image.Dispose();
                    g_lICImages[g_nCurrQuestion].picBox.Image = null;
                    g_lICImages[g_nCurrQuestion].SetSize(0, 0);
                    ResizeAndAdjust();
                    browseBut.Content = "+Изображение";
                }
                return;
            }

            //Image img = g_lICImages[g_nCurrQuestion].img;

            OpenFileDialog imageDialog = new OpenFileDialog();
            imageDialog.Multiselect = false;
            imageDialog.Filter = "Image File (.jpg, .jpeg, .bmp, .png)|*.jpg; *.jpeg; *.bmp; *.png";
            imageDialog.FilterIndex = 1;
            bool? nbClickedOK = imageDialog.ShowDialog();
            if (nbClickedOK == false) return;
            g_lICImages[g_nCurrQuestion].filePath = imageDialog.FileName;
            browseBut.Content = "-Изображение";
            /*
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(g_lICImages[g_nCurrQuestion].filePath, UriKind.Absolute);
            bitmap.EndInit();
            img.Stretch = Stretch.Fill;
            img.Source = bitmap;
            img.Width = tabControl.ActualWidth - 350;
            img.Height = tabControl.ActualHeight - 95;
            img.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            img.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            img.MouseDown += img_MouseDown;
            img.MouseUp += img_MouseUp;
            */
            System.Drawing.Image img = System.Drawing.Image.FromFile(g_lICImages[g_nCurrQuestion].filePath);
            g_lICImages[g_nCurrQuestion].SetSize(
                (int)((double)(img.Width) / (img.Height) * (int)(tabControl.ActualHeight - 95)),
                (int)(tabControl.ActualHeight - 95)
                );
            g_lICImages[g_nCurrQuestion].Show();
            g_lICImages[g_nCurrQuestion].picBox.Image = img;
            //g_lICImages[g_nCurrQuestion].SetMargin(new Thickness(tabControl.Width - g_lICImages[g_nCurrQuestion].picBox.Width - 15, 9, 0, 0));
            g_lICImages[g_nCurrQuestion].picBox.MouseDown += picBox_MouseDown;
            g_lICImages[g_nCurrQuestion].picBox.MouseUp += picBox_MouseUp;

            ResizeAndAdjust();
        }

        void picBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                this.downTime = DateTime.Now;
        }

        void picBox_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {

                TimeSpan timeSinceDown = DateTime.Now - this.downTime;
                if (timeSinceDown.TotalMilliseconds < 300)
                {
                    Window imageWindow = g_wImageWindow;
                    //imageWindow = new Window();

                    /*BitmapFrame bitmapFrame = BitmapFrame.Create(new Uri(g_lICImages[g_nCurrQuestion].filePath), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                    int width = bitmapFrame.PixelWidth;
                    int height = bitmapFrame.PixelHeight;
                    */
                    /* Image img = new Image();
                     BitmapImage bitmap = new BitmapImage();
                     bitmap.BeginInit();
                     bitmap.UriSource = new Uri(g_lICImages[g_nCurrQuestion].filePath, UriKind.Absolute);
                     bitmap.EndInit();
                     img.Stretch = Stretch.Uniform;
                     img.Source = bitmap;
                     //img.Width = tabControl.ActualWidth - 350;
                     img.Height = 500;
                     * 
                     img.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                     img.VerticalAlignment = System.Windows.VerticalAlignment.Top;

                     img.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                     img.Arrange(new Rect(new Point(0, 0), img.DesiredSize));
                     Size size = img.DesiredSize;
                     */
                    WindowsFormsHost wfh = new WindowsFormsHost();
                    System.Windows.Forms.PictureBox picBox = new System.Windows.Forms.PictureBox();
                    System.Drawing.Image img = System.Drawing.Image.FromFile(g_lICImages[g_nCurrQuestion].filePath);
                    picBox.Image = img;
                    wfh.Child = picBox;
                    //wfh.Height = 500;
                    imageWindow.Height = 500;
                    picBox.Height = 500;
                    //wfh.Width = (int)((double)(img.Width) * picBox.Height / img.Height);
                    picBox.Width = (int)((double)(img.Width) * picBox.Height / img.Height);
                    picBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
                    imageWindow.Width = picBox.Width;
                    imageWindow.Content = wfh;
                    //imageWindow.Width = size.Width;
                    imageWindow.ResizeMode = System.Windows.ResizeMode.NoResize;
                    //imageWindow.Icon = Properties.Resources.icon;
                    var iconHandle = Properties.Resources.icon;
                    System.Drawing.Bitmap bitmap = iconHandle.ToBitmap();
                    IntPtr hBitmap = bitmap.GetHbitmap();

                    ImageSource wpfBitmap =
                         System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                              hBitmap, IntPtr.Zero, Int32Rect.Empty,
                              BitmapSizeOptions.FromEmptyOptions());
                    imageWindow.Icon = wpfBitmap;
                    imageWindow.Closing += imageWindow_Closing;
                    imageWindow.Show();
                }
            }
        }

        void imageWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            (sender as Window).Visibility = System.Windows.Visibility.Hidden;
        }

        private DateTime downTime;

        void CreateTabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            g_nCurrQuestion = tabControl.SelectedIndex - 1;
        }

        /// <summary>
        /// If the remove button is clicked this method asks if the user is sure about deleting the question
        /// and if yes - does just that and moves to the next question (if any)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btRemove_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Сигурни ли сте, че искате да изтриете този въпрос?", "Внимание!",
                MessageBoxButton.YesNo) == MessageBoxResult.No) return;
            g_lQCQuestions.RemoveAt(g_nCurrQuestion);
            g_l2rbAnswers.RemoveAt(g_nCurrQuestion);
            tabControl.SelectedIndex = 0;
            g_lTITabs.RemoveAt(g_nCurrQuestion + 1);
            if (g_lTITabs.Count == 2)
                tabControl.SelectedIndex = g_lTITabs.Count - 1;
            else if (g_nCurrQuestion > 0)
                g_nCurrQuestion--;
            tabControl.SelectedIndex = g_nCurrQuestion + 1;
            for (int i = g_nCurrQuestion + 1; i < g_lTITabs.Count - 1; i++)
                g_lTITabs[i].Header = String.Format("Въпрос {0}", i);
        }

        /// <summary>
        /// When a radio button of an answer is checked updates the status of the QuestionClass object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CreateRadioButtonAnswer_Checked(object sender, RoutedEventArgs e)
        {
            int nAnswerNum = -1, nQuestionNum = -1;
            for (nQuestionNum = 0; nQuestionNum < g_l2rbAnswers.Count; nQuestionNum++)
            {
                for (nAnswerNum = 0; nAnswerNum < g_l2rbAnswers[nQuestionNum].Count; nAnswerNum++)
                {
                    if (Object.ReferenceEquals(sender, g_l2rbAnswers[nQuestionNum][nAnswerNum]))
                    {
                        goto next;
                    }
                }
            }
        next:
            //GetAnswerNumAndQuestionNumFromRadioButtonPosition(sender as RadioButton, out nAnswerNum, out nQuestionNum);
            g_lQCQuestions[nQuestionNum].UpdateStatus(null, null, nAnswerNum, null, true);
        }

        /// <summary>
        /// When a radio button of an answer is unchecked updates the status of the QuestionClass object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CreateRadioButtonAnswer_Unchecked(object sender, RoutedEventArgs e)
        {
            int nAnswerNum = -1, nQuestionNum = -1;
            for (nQuestionNum = 0; nQuestionNum < g_l2rbAnswers.Count; nQuestionNum++)
                for (nAnswerNum = 0; nAnswerNum < g_l2rbAnswers[nQuestionNum].Count; nAnswerNum++)
                    if (Object.ReferenceEquals(sender, g_l2rbAnswers[nQuestionNum][nAnswerNum]))
                        goto next;
        next:
            if ((sender as RadioButton).IsChecked == true)
                g_lQCQuestions[nQuestionNum].UpdateStatus(null, null, nAnswerNum, null, false);
        }

        /// <summary>
        /// If the button "Готов съм" is clicked this method checks if there are invalid textboxes or radio buttons.
        /// If there are any - shows an error. If everything is fine - asks for the location of the output file and saves the encoded and encrypted output there.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CreateButtonReady_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < g_lQCQuestions.Count; i++)
            {
                //TODO: out of range exc
                switch (g_lQCQuestions[i].IsSomethingWrong(g_l2rbAnswers[i]))
                {
                    case ISE_ErrorCode.NoAnswers:
                        MessageBox.Show(String.Format("Не сте въвели нито един отговор при въпрос {0}!", i + 1));
                        return;
                    case ISE_ErrorCode.NoQuestion:
                        MessageBox.Show(String.Format("Не сте въвели въпрос при въпрос {0}!", i + 1));
                        return;
                    case ISE_ErrorCode.NoRightAnswer:
                        MessageBox.Show(String.Format("Не сте избрали верен отговор при въпрос {0}!", i + 1));
                        return;
                    case ISE_ErrorCode.NoPoints:
                        MessageBox.Show(String.Format("Не сте въвели брой точки за верен отговор на въпрос {0}!", i + 1));
                        return;
                    case ISE_ErrorCode.AllFine:
                        break;
                }
            }
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "MGTest files (.mgt)|*.mgt";
            saveFileDialog1.FilterIndex = 1;
            bool? nbClickedOK = saveFileDialog1.ShowDialog();
            if (nbClickedOK == true)
            {
                List<string> lsOutput;
                Encode(out lsOutput, g_lQCQuestions);
                List<string> enc = new List<string>();
                for (int i = 0; i < lsOutput.Count; i++)
                {
                    string sOutput = lsOutput[i];
                    enc.Add(Encrypt(ref sOutput, "I like spagetti!"));
                }
                using (Stream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        bw.Write(g_lQCQuestions.Count);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            long bytesCount = -1;
                            for (int i = 0; i < enc.Count; i++)
                            {
                                bw.Write(enc[i]);
                                if (g_lICImages[i].picBox.Image != null)
                                {
                                    g_lICImages[i].picBox.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                    bytesCount = ms.Length;
                                    ms.Position = 0;
                                }
                                bw.Write(bytesCount);
                                if (bytesCount != -1) { ms.CopyTo(fs); ms.Position = 0; }
                            }
                            //bw.Write("hi");
                            //g_lICImages[1].picBox.Image.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                            //long a = fs.Length;

                            /*System.Drawing.Bitmap dImg = (System.Drawing.Bitmap)g_lICImages[0].picBox.Image;
                            MemoryStream ms = new MemoryStream();
                            dImg.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                            System.Windows.Media.Imaging.BitmapImage bImg = new System.Windows.Media.Imaging.BitmapImage();
                            bImg.BeginInit();
                            bImg.StreamSource = new MemoryStream(ms.ToArray());
                            bImg.EndInit();
                            byte[] bytess = BufferFromImage(bImg);
                            bw.Write(bytess);*/


                            //g_lICImages[1].picBox.Image.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
                /*using (StreamWriter stream = new StreamWriter(saveFileDialog1.FileName))
                    stream.WriteLine(enc);*/
                while (g_lTITabs.Count > 1)
                    g_lTITabs.RemoveAt(g_lTITabs.Count - 1);
                g_lQCQuestions = null;
                g_l2rbAnswers = null;
                //btCreate.IsEnabled = true;
                state = TestState.DoingNothing;
                /*StreamReader streamR = new StreamReader("output.mgt");
                string dec = Decrypt(streamR.ReadLine(), "I like spagetti!");
                streamR.Close();*/
            }
        }

        public byte[] BufferFromImage(BitmapImage imageSource)
        {
            Stream stream = imageSource.StreamSource;
            byte[] buffer = null;

            if (stream != null && stream.Length > 0)
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    buffer = br.ReadBytes((Int32)stream.Length);
                }
            }

            return buffer;
        }

        /// <summary>
        /// Clears any textbox starting with "[отговор " or being "[въпрос]" or "[брой точки]" and gets focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CreateTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            string sText = (sender as TextBox).Text;
            if (sText == "[въпрос]" ||
                sText == "[брой точки]" ||
                sText.Length > 8 &&
                sText.Substring(0, 9) == "[отговор ")
                (sender as TextBox).Text = "";
        }

        /// <summary>
        /// Whenever an answer textbox loses focus if it's empty this method makes its content [отговор #]. If not
        /// it appends its content to the output stream.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CreateTextBoxAnswer_LostFocus(object sender, RoutedEventArgs e)
        {
            int i = 0;
            for (; i < g_lTITabs.Count - 2; i++)
            {
                if ((g_lTITabs[i].Content as Grid).Children.Contains(sender as TextBox)) break;
            }
            int nAnswerNum = (int)((sender as TextBox).Margin.Top - 70) / 25 + 1;
            if ((sender as TextBox).Text == "")
            {
                g_lQCQuestions[i - 1].UpdateStatus(null, null, nAnswerNum - 1, "", null);
                (sender as TextBox).Text = String.Format("[отговор {0}]", ((sender as TextBox).Margin.Top - 70) / 25 + 1);
            }
            else
                g_lQCQuestions[i - 1].UpdateStatus(null, null, nAnswerNum - 1, (sender as TextBox).Text, null);
        }

        /// <summary>
        /// Whenever a question textbox loses focus if it's empty this method makes its content [въпрос #]. If not
        /// it appends its content to the output stream.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CreateTextBoxQuestion_LostFocus(object sender, RoutedEventArgs e)
        {
            int i = 0;
            for (; i < g_lTITabs.Count - 2; i++)
            {
                if ((g_lTITabs[i].Content as Grid).Children.Contains(sender as TextBox)) break;
            }
            if ((sender as TextBox).Text == "")
            {
                g_lQCQuestions[i - 1].UpdateStatus("", null, -1, null, null);
                (sender as TextBox).Text = String.Format("[въпрос]");
            }
            else
            {
                //(sender as TextBox).Text = ManageStringLines((sender as TextBox).Text);
                g_lQCQuestions[i - 1].UpdateStatus((sender as TextBox).Text, null, -1, null, null);
            }
        }

        /// <summary>
        /// Whenever a points textbox loses focus if it's empty or contains non-numerical symbols this method makes its content [брой точки].
        /// If its content is a number, saves the number of points to the QuestionClass object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CreateTextBoxPoints_LostFocus(object sender, RoutedEventArgs e)
        {
            string sText = (sender as TextBox).Text;
            int nRes = -1;
            if (!Int32.TryParse(sText, out nRes) || nRes <= 0)
                (sender as TextBox).Text = "[брой точки]";
            g_lQCQuestions[g_nCurrQuestion].UpdateStatus(null, nRes, -1, null, null);
        }
    }
}