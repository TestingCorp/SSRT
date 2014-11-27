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
using System.Text.RegularExpressions;
using System.Windows.Markup;

namespace Secret_Project_WPF
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// While creating a test when the last tab AddQuestion is selected this method creates and adds a new question to the TabControl
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateTabItemAdd_GotFocus(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 0; // Set current tab to 0

            g_lQCQuestions.AddQuestionIfNotExist(); // Add a question object
            TabItem ti = new TabItem();
            ti.Content = CreateAddQuestion(
                new string[] { "[отговор 1]", "[отговор 2]", "[отговор 3]", "[отговор 4]" }); // Create and add the question
            ti.Header = String.Format("Въпрос {0}", (g_lTITabs.Count - 1 <= 1) ? 1 : (g_lTITabs.Count - 1));
            (ti.Content as Grid).Width = tabControl.Width;
            g_lTITabs.Add(ti); // Add the tab with the grid and question to the list of tabs

            // Remove and add the "AddQuestion" tab in order to make it the last tab
            g_lTITabs.Remove(sender as TabItem);
            g_lTITabs.Add(sender as TabItem);

            ResizeAndAdjust();

            tabControl.SelectedIndex = tabControl.Items.Count - 2; // Set current tab to the last question
        }

        /// <summary>
        /// Creates a grid with a question with all its parts - textBoxes and Buttons.
        /// </summary>
        /// <param name="tc"></param>
        /// <param name="asAnswers"></param>
        private Grid CreateAddQuestion(string[] asAnswers)
        {
            //Create the grid for the tab
            Grid grid = new Grid();
            grid.Width = tabControl.Width;
            grid.Margin = new Thickness(0, -5, 0, 0);

            //Create the TextBox for the Question and add it to the grid
            TextBox tbQuestion = new TextBox();
            AutomationProperties.SetAutomationId(tbQuestion, "textbox_question"); // Add an ID
            tbQuestion.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            tbQuestion.SelectionStart = tbQuestion.Text.Length;
            tbQuestion.HorizontalAlignment = HorizontalAlignment.Left;
            tbQuestion.VerticalAlignment = VerticalAlignment.Top;
            tbQuestion.AcceptsReturn = true;
            tbQuestion.TextWrapping = TextWrapping.Wrap;
            tbQuestion.Height = 70;
            tbQuestion.Margin = new Thickness(10, 8, 0, 0);
            tbQuestion.Text = "[въпрос]";
            tbQuestion.GotFocus += CreateTextBox_GotFocus;
            tbQuestion.LostFocus += CreateTextBox_LostFocus;
            tbQuestion.TabIndex = 1;
            grid.Children.Add(tbQuestion); // Add to grid

            List<TextBox> ltbAnswers = new List<TextBox>(); //Initialize a list of TextBoxes...
            g_l2rbAnswers.Add(new List<RadioButton>()); // and the global 2D list of RadioButtons for the answers
            for (int i = 0; i < Math.Min(asAnswers.Count(), 4); i++)
            {
                g_lQCQuestions[g_lQCQuestions.Count - 1].AddAnswer(""); // Add an answer to the global questions object
                ltbAnswers.Add(new TextBox()); // Add a new TextBox to the list
                int nLastIndex = ltbAnswers.Count - 1; // Create a variable for the last index of the list

                //Add an identifier to the TextBox
                string sID = String.Format("textbox_answer_{0}", i);
                AutomationProperties.SetAutomationId(ltbAnswers[nLastIndex], sID);

                ltbAnswers[nLastIndex].Text = asAnswers[i];
                ltbAnswers[nLastIndex].HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                ltbAnswers[nLastIndex].HorizontalAlignment = HorizontalAlignment.Left;
                ltbAnswers[nLastIndex].VerticalAlignment = VerticalAlignment.Top;
                ltbAnswers[nLastIndex].Height = 25;
                ltbAnswers[nLastIndex].Margin = new Thickness(13 + 10, 90 + i * 30, 0, 0);
                ltbAnswers[nLastIndex].GotFocus += CreateTextBox_GotFocus;
                ltbAnswers[nLastIndex].LostFocus += CreateTextBox_LostFocus;
                ltbAnswers[nLastIndex].TabIndex = 2 + i;
                grid.Children.Add(ltbAnswers[nLastIndex]); // Add to grid

                nLastIndex = g_l2rbAnswers.Count - 1; // variable = last index of the global 2D list
                g_l2rbAnswers[nLastIndex].Add(new RadioButton());
                g_l2rbAnswers[nLastIndex][i].Width = 13;
                g_l2rbAnswers[nLastIndex][i].Height = 13;
                g_l2rbAnswers[nLastIndex][i].Margin = new Thickness(10, 90 + i * 30 + 5, 0, 0);
                g_l2rbAnswers[nLastIndex][i].HorizontalAlignment = HorizontalAlignment.Left;
                g_l2rbAnswers[nLastIndex][i].VerticalAlignment = VerticalAlignment.Top;
                g_l2rbAnswers[nLastIndex][i].Checked += CreateRadioButtonAnswer_CheckedChanged;
                g_l2rbAnswers[nLastIndex][i].Unchecked += CreateRadioButtonAnswer_CheckedChanged;
                grid.Children.Add(g_l2rbAnswers[nLastIndex][i]); // Add to grid
            }

            TextBox tbPoints = new TextBox(); // Create the TextBox for the points
            AutomationProperties.SetAutomationId(tbPoints, "textbox_points"); // Add an ID
            tbPoints.Width = 87;
            tbPoints.Height = 25;
            tbPoints.HorizontalAlignment = HorizontalAlignment.Left;
            tbPoints.VerticalAlignment = VerticalAlignment.Top;
            tbPoints.Text = "[брой точки]";
            tbPoints.Margin = new Thickness(10, tabControl.Height - 70, 0, 0);
            tbPoints.GotFocus += CreateTextBox_GotFocus;
            tbPoints.LostFocus += CreateTextBox_LostFocus;
            tbPoints.TabIndex = 6;
            grid.Children.Add(tbPoints); // Add to the grid

            TextBox tbTime = new TextBox(); // Create the TextBox for the time
            AutomationProperties.SetAutomationId(tbTime, "textbox_timer"); // Add an ID
            tbTime.Width = 100;
            tbTime.Height = 25;
            tbTime.HorizontalAlignment = HorizontalAlignment.Left;
            tbTime.VerticalAlignment = VerticalAlignment.Top;

            string sTimerText = "[време (мин:сек)]"; // Default value
            if (QuestionClass.Time != TimeSpan.Zero) // If current time is set, get it
            {
                sTimerText = String.Format("{0:00}:{1:00}",
                                           QuestionClass.Time.Hours * 60 +
                                           QuestionClass.Time.Minutes,
                                           QuestionClass.Time.Seconds);
            }
            tbTime.Text = sTimerText;
            tbTime.GotFocus += CreateTextBox_GotFocus;
            tbTime.LostFocus += CreateTextBox_LostFocus;
            tbTime.TabIndex = 7;
            grid.Children.Add(tbTime); // Add to grid

            Button btBrowse = new Button(); // Create the Button for the image
            AutomationProperties.SetAutomationId(btBrowse, "button_browse");
            btBrowse.HorizontalAlignment = HorizontalAlignment.Left;
            btBrowse.VerticalAlignment = VerticalAlignment.Top;
            btBrowse.Width = 95;
            btBrowse.Height = 25;
            btBrowse.Content = "+Изображение";
            btBrowse.Click += new RoutedEventHandler(CreateButtonBrowse_Click);
            btBrowse.TabIndex = 8;
            grid.Children.Add(btBrowse); // Add to grid

            Button btRemove = new Button(); // Create the Button for the question removal option
            AutomationProperties.SetAutomationId(btRemove, "button_remove");
            btRemove.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btRemove.VerticalAlignment = VerticalAlignment.Top;
            btRemove.Width = 87;
            btRemove.Height = 25;
            btRemove.Content = "Изтриване";
            btRemove.Click += CreateButtonRemove_Click;
            btRemove.TabIndex = 9;
            grid.Children.Add(btRemove); // Add to grid

            Button btReady = new Button(); // Create the Button for when Ready
            AutomationProperties.SetAutomationId(btReady, "button_ready");
            btReady.HorizontalAlignment = HorizontalAlignment.Left;
            btReady.VerticalAlignment = VerticalAlignment.Top;
            btReady.Width = 87;
            btReady.Height = 25;
            btReady.Content = "Готов съм";
            btReady.Click += CreateButtonReady_Click;
            btReady.TabIndex = 10;
            grid.Children.Add(btReady); // Add to grid

            g_lICImages.Add(new ImageClass()); // Create an ImageClass object for the image
            grid.Children.Add(g_lICImages[g_lICImages.Count - 1].wfh); // Add its WindowsFormsControlHost the the grid
            return grid; // Return the grid
        }

        /// <summary>
        /// Handler for the add/remove Image button. 1. Opens the image file and shows it.
        /// 2. Removes the current image if any
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            var browseBut = sender as Button;
            if (browseBut.Content.Equals("-Изображение")) // Check if image already set
            {
                if (MessageBox.Show(String.Format("В момента имате добавено изображение! " +
                                                  "Сигурни ли сте, че желаете да го изтриете?"),
                                                  "Внимание!",
                                                  MessageBoxButton.YesNo,
                                                  MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    g_lICImages[currentQuestionNum].picBox.Image.Dispose(); // Destroy the image
                    g_lICImages[currentQuestionNum].picBox.Image = null;
                    g_lICImages[currentQuestionNum].SetSize(0, 0);

                    ResizeAndAdjust();

                    browseBut.Content = "+Изображение";
                }
                return;
            }

            OpenFileDialog imageDialog = new OpenFileDialog(); // Create an Open dialog for the image
            imageDialog.Multiselect = false;
            imageDialog.Filter = "Image File (.jpg, .jpeg, .bmp, .png)|*.jpg; *.jpeg; *.bmp; *.png";
            imageDialog.FilterIndex = 1;

            bool? nbClickedOK = imageDialog.ShowDialog(); // Show the dialog and let the user choose an image
            if (nbClickedOK == false || nbClickedOK == null) return; // If the user clicked 'Cancel', return

            browseBut.Content = "-Изображение";

            System.Drawing.Image img = System.Drawing.Image.FromFile(imageDialog.FileName); // Create an image object base on the file chosen

            double imageRatio = (double)(img.Width) / img.Height; // Calculate the image's width/height proportions
            int controlsHeight = (int)(tabControl.ActualHeight - 95); // Set the image's height based on the TabControl
            int controlsWidth = (int)(imageRatio * controlsHeight); // Set the image's width based on the image ratio

            // Set the image and its size and show it
            ImageClass currentImage = g_lICImages[currentQuestionNum];
            currentImage.picBox.Image = img;
            currentImage.SetSize(controlsWidth, controlsHeight);
            currentImage.Show();

            //Set up the image's MouseDown and MouseUp events for the click option
            currentImage.picBox.MouseDown += ImageClass.picBox_MouseDown;
            currentImage.picBox.MouseUp +=
                (object mouseUpSender, System.Windows.Forms.MouseEventArgs mouseUpE) =>
                {
                    // Add the appropriate methods to be executed when the image is opened and closed
                    ImageClass.pictureOpenMethods += ResizeAndAdjust;
                    ImageClass.pictureOpenMethods += DisableControls;
                    ImageClass.pictureCloseMethods += ResizeAndAdjust;
                    ImageClass.pictureCloseMethods += EnableControls;

                    currentImage.picBox_MouseUp(mouseUpSender, mouseUpE);
                };

            ResizeAndAdjust();
        }

        /// <summary>
        /// If the remove button is clicked this method asks if the user is sure about deleting the question
        /// and if yes - does just that and moves to the next question (if any)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Сигурни ли сте, че искате да изтриете този въпрос?",
                                "Внимание!",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }
            tabControl.SelectedIndex = 0;  // Set current tab to 0

            // Remove appropiate index of the question from the global lists
            g_lQCQuestions.RemoveAt(currentQuestionNum);
            g_l2rbAnswers.RemoveAt(currentQuestionNum);
            g_lTITabs.RemoveAt(currentQuestionNum + 1);
            g_lICImages.RemoveAt(currentQuestionNum);

            if (g_lTITabs.Count == 2) // If all the tabs are 2 (no more questions) select the last one to create a question
            {
                tabControl.SelectedIndex = g_lTITabs.Count - 1;
            }
            else if (currentQuestionNum > 0) // Else if the current question is not the first, decrease it by 1
            {
                currentQuestionNum--;
            }
            tabControl.SelectedIndex = currentQuestionNum + 1; // Select the tab corresponding to the current question number

            // Manage all the tab headers after the deleted question
            for (int i = tabControl.SelectedIndex; i < g_lTITabs.Count - 1; i++)
            {
                g_lTITabs[i].Header = String.Format("Въпрос {0}", i);
            }
        }

        /// <summary>
        /// If the button "Готов съм" is clicked this method checks if there are invalid textboxes or radio buttons.
        /// If there are any - shows an error. If everything is fine - asks for the location of the output file and saves the encoded and encrypted output there.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateButtonReady_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if something is wrong with each question
                for (int i = 0; i < g_lQCQuestions.Count; i++)
                {
                    TestErrorCode errorCode = g_lQCQuestions[i].IsSomethingWrong(g_l2rbAnswers[i]);
                    if (errorCode == TestErrorCode.AllFine) continue; // If everthing is fine, continue to next question

                    string sError = String.Empty;
                    switch (errorCode)
                    {
                        case TestErrorCode.NoQuestion:
                            sError = "Не сте въвели въпрос при";
                            break;
                        case TestErrorCode.NoAnswers:
                            sError = "Не сте въвели нито един отговор при";
                            break;
                        case TestErrorCode.TooFewAnswers:
                            sError = "Въвели сте прекалено малко отговори при ";
                            break;
                        case TestErrorCode.DuplicateAnswers:
                            sError = "Въвели сте два еднакви отговора при ";
                            break;
                        case TestErrorCode.NoRightAnswer:
                            sError = "Не сте избрали верен отговор при";
                            break;
                        case TestErrorCode.NoPoints:
                            sError = "Не сте въвели брой точки за верен отговор на";
                            break;
                    }
                    MessageBox.Show(String.Format(sError + " въпрос {0}!", i + 1),
                                    "Внимание",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return; // Is there is something wrong - return, do not continue
                }

                // Check if time has been set
                if (QuestionClass.Time == TimeSpan.Zero)
                {
                    if (MessageBox.Show("Не сте избрали време за решаване на теста! Сигурни ли " +
                                        "сте, че не искате да задавате време?",
                                        "Внимание",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question) == MessageBoxResult.No)
                    {
                        return; // If no time has been set (the time is 00:00:00) but the user wants to add - return
                    }
                }

                SaveFileDialog saveFileDialog1 = new SaveFileDialog(); // Create a save dialog for the test
                saveFileDialog1.Filter = "SUT Test (.sut)|*.sut";
                saveFileDialog1.FilterIndex = 1;

                bool? nbClickedOK = saveFileDialog1.ShowDialog(); // Show the dialog and let the user choose a destination for the test
                if (nbClickedOK != true) return; // If the user clicked 'Cancel', return    

                List<string> lsEncoded; // Create a list of strings for the encoded output
                if (!Encode(out lsEncoded, g_lQCQuestions)) // Encode the question objects to the list of strings
                {
                    throw new OperationFailedException("Кодирането на данните се провали."); // If the encoding fails
                }

                List<string> lsEncrypted = new List<string>(); // Create a list of strings for the encrypted output
                for (int i = 0; i < lsEncoded.Count; i++)
                {
                    string sOutput = lsEncoded[i];
                    lsEncrypted.Add(Encrypt(ref sOutput, "I like spagetti!")); // Encrypt each item from the encoded strings
                }

                using (Stream fileStream = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                    {
                        binaryWriter.Write(g_lQCQuestions.Count); // Fisrt write the count of the questions
                        using (MemoryStream ms = new MemoryStream())
                        {
                            long imageBytesCount = -1; // Create a variable for the count of bytes for the current image
                            for (int i = 0; i < lsEncrypted.Count; i++) // For each of the encrypted strings
                            {
                                binaryWriter.Write(lsEncrypted[i]); // Write the encrypted string
                                if (g_lICImages[i].picBox.Image != null) // If the image is not null
                                {
                                    // Create a resized version of the image
                                    System.Drawing.Bitmap resizedImage;
                                    System.Drawing.Size newSize = new System.Drawing.Size();
                                    newSize.Height = 500;
                                    double imageRatio = (double)(g_lICImages[i].picBox.Image.Width) /
                                                                 g_lICImages[i].picBox.Image.Height;
                                    newSize.Width = (int)(imageRatio * newSize.Height);
                                    resizedImage = new System.Drawing.Bitmap(g_lICImages[i].picBox.Image, newSize);

                                    resizedImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png); // Save the resized version to a memory stream object
                                    imageBytesCount = ms.Length; // Set the bytes count variable to the lenth of the memory stream
                                    ms.Position = 0; // Reset memory stream position
                                }
                                binaryWriter.Write(imageBytesCount); // Write the count of the bytes
                                if (imageBytesCount != -1) // If there is an image
                                {
                                    ms.CopyTo(fileStream); // write the image data from the memory stream
                                    ms.Position = 0; // Reset memory stream position
                                }
                            }
                            // Write the time's minutes and seconds
                            binaryWriter.Write(QuestionClass.Time.Minutes);
                            binaryWriter.Write(QuestionClass.Time.Seconds);
                        }
                    }

                    state = TestState.DoingNothing; // Reset the state of the test
                    ResetTest(); // Reset the test
                }
            }
            // Catch exceptions
            catch (OperationFailedException exc)
            {
                MessageBox.Show("Възникна грешка при записването на теста. Грешката показва следното съобщение:{0}",
                                exc.Message);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Възникна непозната грешка при записването на теста. " +
                    "Грешката показва следното съобщение:{0}", exc.Message);
            }
        }

        /// <summary>
        /// When a radio button of an answer is unchecked updates the status of the QuestionClass object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateRadioButtonAnswer_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // Get the RadioButton's number as an answer and the question number
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
            return;
        next:
            // If it has been checked set the right answer
            if ((sender as RadioButton).IsChecked == true)
            {
                string correspondingTextboxId = String.Format("textbox_answer_" + nAnswerNum);
                object textBox = GetObjectById(correspondingTextboxId, currentQuestionNum + 1);
                if (textBox != null)
                {
                    TextBox correspondingTextbox = textBox as TextBox;
                    // If the corresponding textBox is empty or has a default value, show a message
                    if (correspondingTextbox.Text == String.Empty ||
                        correspondingTextbox.Text.Length > 8 &&
                        correspondingTextbox.Text.Substring(0, 9) == "[отговор ")
                    {
                        MessageBox.Show("Моля въведете отговор преди да сте го отбелязали като верен!",
                                        "Внимание",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);
                        (sender as RadioButton).IsChecked = false;
                    }
                }
                g_lQCQuestions[nQuestionNum].Answers[nAnswerNum].IsRightAnswer = true;
            }
            else
                g_lQCQuestions[nQuestionNum].Answers[nAnswerNum].IsRightAnswer = false;
        }

        /// <summary>
        /// Clears any textbox starting with the default values when it gets focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            string sText = (sender as TextBox).Text;
            if (sText == "[въпрос]" ||
                sText == "[брой точки]" ||
                sText == "[време (мин:сек)]" ||
                sText.Length > 8 &&
                sText.Substring(0, 9) == "[отговор ")
                (sender as TextBox).Text = String.Empty;
        }

        /// <summary>
        /// Whenever a textbox loses focus if it's empty this method makes its content equal to the appropriate default value.
        /// If not it updates the list of questions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //Get ID of text box (format is "textbox_answer_{number}")
            string sID = AutomationProperties.GetAutomationId(sender as TextBox);

            //Set the question number from the object
            int nQuestionNum = GetTabIndexByObject(sender) - 1;

            string sContent = (sender as TextBox).Text;

            if (sID.Length > 14 && sID.Substring(0, 14) == "textbox_answer")
            {
                //Get the answer number from the ID and assign it to nAnswerNum
                int nAnswerNum = Int32.Parse(sID.Substring(15));
                //Check if the string in the textbox is empty
                if (sContent == String.Empty)
                {
                    //Set the answer in the global list of questions to be empty as well
                    g_lQCQuestions[nQuestionNum].Answers[nAnswerNum].Value = String.Empty;
                    //and set the textbox'es text equal to the appropriate default value
                    (sender as TextBox).Text = String.Format("[отговор {0}]", nAnswerNum + 1);

                    RadioButton correspondingRadioButton = g_l2rbAnswers[nQuestionNum][nAnswerNum];
                    correspondingRadioButton.IsChecked = false;
                }
                else
                    //If the textbox is not empty set the answer to its content
                    g_lQCQuestions[nQuestionNum].Answers[nAnswerNum].Value = sContent;
            }
            else if (sID == "textbox_question")
            {
                //Check if the string in the textbox is empty
                if (sContent == String.Empty)
                {
                    //Set the question in the global list of questions to be empty as well
                    g_lQCQuestions[currentQuestionNum].Question = String.Empty;
                    //and set the textbox'es text the appropriate default value
                    (sender as TextBox).Text = String.Format("[въпрос]");
                }
                else
                {
                    //If the textbox is not empty set the question equal to its content
                    g_lQCQuestions[currentQuestionNum].Question = sContent;
                }
            }
            else if (sID == "textbox_points")
            {
                int nRes = -1;
                //Try to convert the content of the textbox into a number
                if (!Int32.TryParse(sContent, out nRes) || nRes <= 0)
                {
                    //if it fails or the number is not positive set the textbox'es content the appropriate default value
                    (sender as TextBox).Text = "[брой точки]";
                }
                //Set the points equal to the texbox'es content
                g_lQCQuestions[currentQuestionNum].Points = nRes;
            }
            else if (sID == "textbox_timer")
            {
                //Regex r = new Regex("\\d\\d\\:\\d\\d");
                //Match m = r.Match(sContent);
                //if (sContent.Length == 5 && m.Success && !sContent.Equals("00:00"))
                int a;
                if (sContent.Contains(':') &&
                    Int32.TryParse(sContent.SubstringCharToChar(':', 0, true), out a) &&
                    Int32.TryParse(sContent.Substring(sContent.IndexOf(':') + 1), out a))
                {
                    string sMinutes = sContent.SubstringCharToChar(':', 0, true);
                    int minutes = Int32.Parse(sMinutes);
                    string sSeconds = sContent.Substring(sContent.IndexOf(':') + 1);
                    int seconds = Int32.Parse(sSeconds);

                    QuestionClass.Time = new TimeSpan(0, minutes, seconds);
                    (sender as TextBox).Text = String.Format("{0:00}:{1:00}",
                                               QuestionClass.Time.Hours * 60 +
                                               QuestionClass.Time.Minutes,
                                               QuestionClass.Time.Seconds);
                }
                else
                {
                    QuestionClass.Time = TimeSpan.Zero;
                    (sender as TextBox).Text = "[време (мин:сек)]";
                }
                for (int i = 1; i < g_lTITabs.Count - 1; i++)
                {
                    TextBox textBox = GetObjectById("textbox_timer", i) as TextBox;
                    textBox.Text = (sender as TextBox).Text;
                }
            }
        }
    }
}