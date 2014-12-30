using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DiacriticBuster
{
    public partial class OptionsForm : Form
    {
        //MainForm mf = (MainForm)Form.ActiveForm; // this way is incompatible with VS breakpoints and makes the debugging application crash
        MainForm mf = (MainForm)Application.OpenForms[0];
        string activeLanguage;
        string selectedLanguage;

        public OptionsForm(string language)
        {
            activeLanguage = language;
            selectedLanguage = activeLanguage;
            InitializeComponent();
            listBox1.Items.Add(Properties.Resources.Default);
            // insert all the schemes found inside Schemes folder into "Available schemes" list
            DirectoryInfo schemes = new DirectoryInfo(mf.ReturnSchemesDirectoryName());
            FileInfo[] files = schemes.GetFiles("*.txt");
            foreach (FileInfo file in files)
                listBox1.Items.Add(file.Name.Substring(0, file.Name.Length - 4));
            // select the currently chosen scheme
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                listBox1.SelectedIndex = i;
                if (listBox1.SelectedItem.ToString() == mf.ReturnCurrentSchemeName())
                    break;
                else
                    listBox1.SelectedIndex = 0;
            }
            // determine which program language is currently being used
            if (activeLanguage.IndexOf("pl") > -1)
                radioButton2.Checked = true;
            else if (activeLanguage.IndexOf("de") > -1)
                radioButton3.Checked = true;
            else
                radioButton1.Checked = true;
        }

        private void ShowNoSchemeSelectedMessageBox()
        {
            MessageBox.Show(Properties.Resources.NoSchemeSelectedMessage, FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location).ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void ApplySettings()
        {
            if (selectedLanguage != activeLanguage)
            {
                mf.ChangeLanguage(selectedLanguage);
                activeLanguage = selectedLanguage;
                MessageBox.Show(Properties.Resources.ChangedLanguageMessage, FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location).ProductName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            mf.ChangeSchemePublicInfo(listBox1.SelectedItem.ToString());
            string scheme = mf.ReturnCurrentSchemeName();
            if (scheme == Properties.Resources.Default)
                scheme = "<default>";
            var sw = new StreamWriter(new FileStream(mf.ReturnConfigFileName(), FileMode.Create), Encoding.UTF8);
            sw.WriteLine("; Don't modify this file manually! Nie modyfikować tego pliku ręcznie! Modifizieren Sie nicht diese Datei manuell!\r\n[" + FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location).ProductName + "]\r\nLanguage=" + selectedLanguage + "\r\nScheme=" + scheme); // rewriting the configuration into the file using UTF-8 conversion
            sw.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                ApplySettings();
                this.Close();
            }
            else
                ShowNoSchemeSelectedMessageBox();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
                ApplySettings();
            else
                ShowNoSchemeSelectedMessageBox();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == 0)
                button4.Enabled = false;
            else
                button4.Enabled = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                selectedLanguage = "en-CA";
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
                selectedLanguage = "pl-PL";
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
                selectedLanguage = "de-DE";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string schemeFileLocation = mf.ReturnSchemesDirectoryName() + listBox1.SelectedItem.ToString() + ".txt";
            if (File.Exists(schemeFileLocation))
                File.Delete(schemeFileLocation);
            listBox1.Items.Remove(listBox1.SelectedItem);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var FD = new System.Windows.Forms.OpenFileDialog();
            FD.DefaultExt = "txt";
            FD.ValidateNames = true;
            FD.Filter = Properties.Resources.FileTypes;
            if (FD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (File.Exists(FD.FileName))
                {
                    // Scheme file compatibility check
                    var sr = new StreamReader(FD.FileName);
                    string srLine;
                    int i = 0;
                    bool isPassed = true; // a variable determining if the test is passed
                    while ((srLine = sr.ReadLine()) != null)
                    {
                        if (srLine.Length > 0 && (srLine.IndexOf('|') == -1 || srLine.IndexOf('|') != srLine.LastIndexOf('|'))) // there must be only one single '|' char per line
	                    {
                            isPassed = false; // TEST NOT PASSED
		                    break;
	                    }
                        i++;
                    }
                    sr.Close();
                    // Importing process
                    if (isPassed)
                    {
                        string destinationFileName = mf.ReturnSchemesDirectoryName() + FD.SafeFileName;
                        if (File.Exists(destinationFileName))
                        {
                            var overwriteMB = MessageBox.Show(Properties.Resources.OverwriteFileMessage, FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location).ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (overwriteMB == DialogResult.Yes)
                                File.Copy(FD.FileName, destinationFileName, true);
                        }
                        else
                        {
                            File.Copy(FD.FileName, destinationFileName);
                            if (File.Exists(destinationFileName))
                                listBox1.Items.Add(FD.SafeFileName.Split('.')[0]);
                        }
                    }
                    else
                        MessageBox.Show(Properties.Resources.SchemeFileNotValidMessage, Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                    MessageBox.Show(Properties.Resources.FileNotFoundMessage, Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }
    }
}
