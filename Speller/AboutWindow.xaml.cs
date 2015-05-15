using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;
using System.Diagnostics;

namespace Speller
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        Assembly assemb = Assembly.GetExecutingAssembly();

        public AboutWindow()
        {
            InitializeComponent();
            AssemblyTitleAttribute title = (AssemblyTitleAttribute)assemb.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0];
            Version ver = assemb.GetName().Version;
            AssemblyCompanyAttribute company = (AssemblyCompanyAttribute)assemb.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0];
            string author;
            if (company.Company.Contains('_'))
            {
                string authorLeft = company.Company.Substring(0, company.Company.IndexOf('_') + 1);
                string authorRight = company.Company.Substring(company.Company.IndexOf('_'));
                author = authorLeft + authorRight;
            }
            else
                author = company.Company;
            AssemblyCopyrightAttribute copyright = (AssemblyCopyrightAttribute)assemb.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0];
            string copyrightInfo;
            if (copyright.Copyright.Contains('_'))
            {
                string copyrightInfoLeft = copyright.Copyright.Substring(0, copyright.Copyright.IndexOf('_') + 1);
                string copyrightInfoRight = copyright.Copyright.Substring(copyright.Copyright.IndexOf('_'));
                copyrightInfo = copyrightInfoLeft + copyrightInfoRight;
            }
            else
                copyrightInfo = copyright.Copyright;
            this.AboutLabel.Content = String.Format(this.AboutLabel.Content.ToString(), title.Title, ver.ToString(), author, copyrightInfo);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            WindowIcon.Remove(this);
        }

        private void OKDockButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WebsiteDockButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Properties.Resources.Website);
        }

        private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
    }
}
