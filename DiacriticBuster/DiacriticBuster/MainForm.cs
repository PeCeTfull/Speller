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
    public partial class MainForm : Form
    {
        const string configFileName = "DiacriticBuster.ini"; // the configuration filename
        // DEFAULT CONFIGURATION
        string currentScheme = Properties.Resources.Default;
        string currentLanguage = "en-CA";

        // FORM'S 'PERSONAL' OPTIONS
        string schemesDirectory = Environment.CurrentDirectory + "\\Schemes\\";
        int currentSchemeBasicStringLength;
        Dictionary<string, string> currentDiacriticDealingMethods = new Dictionary<string,string>();

        public void LoadScheme()
        {
            if (currentScheme != "<default>")
            {
                currentDiacriticDealingMethods.Clear();
                string schemeFileLocation = schemesDirectory + currentScheme + ".txt";
                if (File.Exists(schemeFileLocation))
                {
                    var sr = new StreamReader(schemeFileLocation);
                    string srLine;
                    int i = 0;
                    while ((srLine = sr.ReadLine()) != null)
                    {
                        string[] diacriticRule = srLine.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        currentDiacriticDealingMethods.Add(diacriticRule[0], diacriticRule[1]);
                        i++;
                    }
                    sr.Close();
                }
                else
                    currentScheme = Properties.Resources.Default;
            }
        }

        public void ChangeLanguage(string chosenLanguage)
        {
            foreach (Control c in this.Controls)
            {
                var crm = new ComponentResourceManager(typeof(MainForm));
                crm.ApplyResources(c, c.Name, new CultureInfo(chosenLanguage));
            }
            currentLanguage = chosenLanguage;
            currentSchemeBasicStringLength = this.label3.Text.Length;
        }

        public MainForm()
        {
            if (File.Exists(configFileName)) // reading the configuration file
            {
                var sr = new StreamReader(configFileName);
                string srLine;
                while ((srLine = sr.ReadLine()) != null)
                {
                    if (srLine.Contains("Language="))
                    {
                        currentLanguage = srLine.Substring(9);
                        Thread.CurrentThread.CurrentCulture = new CultureInfo(currentLanguage);
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentLanguage);
                    }
                    else if (srLine.Contains("Scheme="))
                    {
                        currentScheme = srLine.Substring(7);
                        if (currentScheme == "<default>")
                            currentScheme = Properties.Resources.Default;
                    }
                }
                sr.Close();
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(currentLanguage);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentLanguage);
            }
            InitializeComponent();
            this.Text = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location).ProductName;
            LoadScheme();
            currentSchemeBasicStringLength = this.label3.Text.Length;
            this.label3.Text += currentScheme;
            //ChangeLanguage("en");
        }

        public string ReturnSchemesDirectoryName()
        {
            return schemesDirectory;
        }

        public string ReturnCurrentSchemeName()
        {
            return currentScheme;
        }

        public string ReturnConfigFileName()
        {
            return configFileName;
        }

        public void ChangeSchemePublicInfo(string switchedScheme)
        {
            label3.Text = label3.Text.Substring(0, currentSchemeBasicStringLength);
            currentScheme = switchedScheme;
            string schemeName;
            if (currentScheme.Length > 52)
                schemeName = currentScheme.Substring(0, 50) + "...";
            else
                schemeName = currentScheme;
            label3.Text += schemeName;
            LoadScheme();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string initialText = textBox1.Text;
            string finalText = "";
            textBox2.Text = "";
            if (currentScheme == Properties.Resources.Default)
            {
                byte[] textBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(initialText);
                finalText = System.Text.Encoding.UTF8.GetString(textBytes);
            }
            else
            {
                for (int i = 0; i < initialText.Length; i++)
                {
                    bool accentNotFound = true;
                    foreach (var accent in currentDiacriticDealingMethods)
                    {
                        if (accent.Key == initialText.Substring(i, 1))
                        {
                            if (accent.Value == "$")
                            {
                                byte[] textBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(accent.Key);
                                finalText += System.Text.Encoding.UTF8.GetString(textBytes);
                            }
                            else
                                finalText += accent.Value;
                            accentNotFound = false;
                            break;
                        }
                    }
                    if (accentNotFound)
                        finalText += initialText.Substring(i, 1);
                }
            }
            textBox2.Text = finalText;
        }

        OptionsForm of;
        AboutBox ab;

        private void button2_Click(object sender, EventArgs e)
        {
            if (of != null)
            {
                of.Close();
                of = null;
            }
            of = new OptionsForm(currentLanguage);
            of.Show(this);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ab = new AboutBox();
            ab.ShowDialog(this);
        }
    }
}
