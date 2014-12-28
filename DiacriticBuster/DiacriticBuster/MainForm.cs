using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DiacriticBuster
{
    public partial class MainForm : Form
    {
        //OPTIONS
        string currentScheme = "<none>";

        //FORM'S 'PERSONAL' OPTIONS
        int currentSchemeBasicStringLength;
        Dictionary<string, string> currentDiacriticDealingMethods = new Dictionary<string,string>();

        public void LoadScheme()
        {
            if (currentScheme != "<none>")
            {
                currentDiacriticDealingMethods.Clear();
                var sr = new StreamReader(Environment.CurrentDirectory + "\\Schemes\\" + currentScheme + ".txt");
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
        }

        public MainForm()
        {
            InitializeComponent();
            LoadScheme();
            this.Text = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location).ProductName;
            currentSchemeBasicStringLength = this.label3.Text.Length;
            this.label3.Text += currentScheme;
        }

        public string ReturnCurrentSchemeName()
        {
            return currentScheme;
        }

        public void ChangeSchemePublicInfo(string switchedScheme)
        {
            label3.Text = label3.Text.Substring(0, currentSchemeBasicStringLength);
            currentScheme = switchedScheme;
            string schemeName;
            if (currentScheme.Length > 27)
                schemeName = currentScheme.Substring(0, 25) + "...";
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
            if (currentScheme == "<none>")
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

        private void button2_Click(object sender, EventArgs e)
        {
            if (of != null)
            {
                of.Close();
                of = null;
            }
            of = new OptionsForm();
            of.Show();
        }
    }
}
