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
    public partial class OptionsForm : Form
    {
        MainForm mf = (MainForm)Application.OpenForms[0];

        public OptionsForm()
        {
            InitializeComponent();
            listBox1.Items.Add("<none>");
            //insert all the schemes found inside Schemes folder into "Available schemes" list
            DirectoryInfo schemes = new DirectoryInfo(Environment.CurrentDirectory + "\\Schemes");
            FileInfo[] files = schemes.GetFiles("*.txt");
            foreach (FileInfo file in files)
                listBox1.Items.Add(file.Name.Substring(0, file.Name.Length - 4));
            //select the currently chosen scheme
            listBox1.SelectedItem = mf.ReturnCurrentSchemeName();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mf.ChangeSchemePublicInfo(listBox1.SelectedItem.ToString());
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
