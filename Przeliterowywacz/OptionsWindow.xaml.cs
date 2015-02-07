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
using System.IO;
using System.Speech.Synthesis;

namespace Przeliterowywacz
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        MainWindow main = (MainWindow)Application.Current.MainWindow;
        string configFileName;
        bool checkingLanguage;

        public OptionsWindow(string configFileName, bool diacriticalOption, bool derivativeOption, bool sapiOption, bool zeroAsOLetterOption, Int16 rateNumber, Int16 volumeNumber, string speechbankName, string sapiName, string languageCode)
        {
            this.configFileName = configFileName;
            InitializeComponent();
            CheckBox1.IsChecked = diacriticalOption;
            CheckBox2.IsChecked = derivativeOption;
            if (sapiOption)
                RadioButton2.IsChecked = sapiOption;
            else
                RadioButton1.IsChecked = !sapiOption;
            CheckBox3.IsChecked = zeroAsOLetterOption;
            NumberBox1.Text = rateNumber.ToString();
            NumberBox2.Text = volumeNumber.ToString();
            // Wyszukuje wszystkie podfoldery w folderze Banki i dodaje znalezione elementy do listy dostępnych banków mowy
            ComboBoxItem cbi;
            string[] folderNames = Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory + "Banki\\");
            for (int i = 0; i < folderNames.Length; i++)
            {
                string[] folderNameStructure = folderNames[i].Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                string folderName = Properties.Resources.BanksFolderNotFound;
                for (int j = 0; j < folderNameStructure.Length - 1; j++)
                    if (folderNameStructure[j] == "Banki")
                    {
                        folderName = folderNameStructure[j + 1];
                        break;
                    }
                cbi = new ComboBoxItem();
                cbi.Content = folderName;
                ComboBox1.Items.Add(cbi);
            }
            // Sprawdza, czy ostatnio wybrany bank mowy dalej występuje w folderze „Banki”
            for (int i = 0; i < ComboBox1.Items.Count; i++)
            {
                ComboBox1.SelectedIndex = i;
                if (ComboBox1.SelectedItem.ToString().Substring(38) == speechbankName)
                    break;
                if (i == ComboBox1.Items.Count - 1)
                {
                    ComboBox1.SelectedIndex = 0;
                    MessageBox.Show(String.Format(Properties.Resources.SpeechbankFolderNotFoundMessage, speechbankName), main.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            // Pobieranie z systemu obecnie zainstalowanych syntezatorów mowy SAPI5
            var allInstalledVoices = new SpeechSynthesizer().GetInstalledVoices();
            foreach (var voice in allInstalledVoices)
            {
                cbi = new ComboBoxItem();
                cbi.Content = voice.VoiceInfo.Name;
                ComboBox3.Items.Add(cbi);
            }
            // Sprawdza, czy ostatnio wybrany syntezator SAPI5 dalej występuje w systemie
            for (int i = 0; i < ComboBox3.Items.Count; i++)
            {
                ComboBox3.SelectedIndex = i;
                if (ComboBox3.SelectedItem.ToString().Substring(38) == sapiName)
                    break;
                if (i == ComboBox3.Items.Count - 1)
                {
                    ComboBox3.SelectedIndex = 0;
                    MessageBox.Show(String.Format(Properties.Resources.SAPI5VoiceNotFoundMessage, sapiName), main.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            // Sprawdza, jaki jest obecnie używany język interfejsu
            checkingLanguage = true;
            for (int i = 0; i < ComboBox2.Items.Count; i++)
            {
                ComboBox2.SelectedIndex = i;
                if (Convert.ToInt16(ComboBox2.SelectedIndex) == 1 && languageCode == "pl")
                    break;
                else if (Convert.ToInt16(ComboBox2.SelectedIndex) == 2 && languageCode == "de")
                    break;
                if (i == ComboBox2.Items.Count - 1)
                    ComboBox2.SelectedIndex = 0;
            }
            checkingLanguage = false;
        }

        private void DockButton1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CheckBox1_Checked(object sender, RoutedEventArgs e)
        {
            main.includeDiacriticalChars = true;
        }

        private void CheckBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            main.includeDiacriticalChars = false;
        }

        private void CheckBox2_Checked(object sender, RoutedEventArgs e)
        {
            main.deriveFromDefaultSpeechbank = true;
        }

        private void CheckBox2_Unchecked(object sender, RoutedEventArgs e)
        {
            main.deriveFromDefaultSpeechbank = false;
        }

        private void RadioButton1_Checked(object sender, RoutedEventArgs e)
        {
            main.useSapi = false;
        }

        private void RadioButton2_Checked(object sender, RoutedEventArgs e)
        {
            main.useSapi = true;
        }

        private void CheckBox3_Checked(object sender, RoutedEventArgs e)
        {
            main.readZeroAsO = true;
        }

        private void CheckBox3_Unchecked(object sender, RoutedEventArgs e)
        {
            main.readZeroAsO = false;
        }

        private void NumberBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Int16.TryParse(NumberBox1.Text, out main.sapiRate))
                NumberBox1.Text = main.sapiRate.ToString();
            else if (Convert.ToInt16(NumberBox1.Text) > 10)
                NumberBox1.Text = "10";
            else if (Convert.ToInt16(NumberBox1.Text) < -10)
                NumberBox1.Text = "-10";
        }

        private void NumberBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Int16.TryParse(NumberBox2.Text, out main.sapiVolume))
                NumberBox2.Text = main.sapiVolume.ToString();
            else if (Convert.ToInt16(NumberBox2.Text) > 100)
                NumberBox2.Text = "100";
            else if (Convert.ToInt16(NumberBox2.Text) < 0)
                NumberBox2.Text = "0";
        }

        private void buttonOneMore1_Click(object sender, RoutedEventArgs e)
        {
            main.sapiRate++;
            NumberBox1.Text = main.sapiRate.ToString();
        }

        private void buttonOneLess1_Click(object sender, RoutedEventArgs e)
        {
            main.sapiRate--;
            NumberBox1.Text = main.sapiRate.ToString();
        }

        private void buttonOneMore2_Click(object sender, RoutedEventArgs e)
        {
            main.sapiVolume++;
            NumberBox2.Text = main.sapiVolume.ToString();
        }

        private void buttonOneLess2_Click(object sender, RoutedEventArgs e)
        {
            main.sapiVolume--;
            NumberBox2.Text = main.sapiVolume.ToString();
        }

        private void StyleButton1_Click(object sender, RoutedEventArgs e)
        {
            main.TextBox1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7FFFFFC0"));
            main.TextBox1.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
            main.inputScheme = 0;
        }

        private void StyleButton2_Click(object sender, RoutedEventArgs e)
        {
            main.TextBox1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
            main.TextBox1.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"));
            main.inputScheme = 1;
        }

        private void StyleButton3_Click(object sender, RoutedEventArgs e)
        {
            main.TextBox1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"));
            main.TextBox1.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FF00"));
            main.inputScheme = 2;
        }

        private void ComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            main.currentSpeechbank = ComboBox1.SelectedItem.ToString().Substring(38);
        }

        private void ComboBox3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            main.currentSapi = ComboBox3.SelectedItem.ToString().Substring(38);
        }

        private void ComboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string languageUsedBefore = main.currentLanguage;
            if (Convert.ToInt16(ComboBox2.SelectedIndex) == 1)
                main.currentLanguage = "pl";
            else if (Convert.ToInt16(ComboBox2.SelectedIndex) == 2)
                main.currentLanguage = "de";
            else
                main.currentLanguage = "en";
            if (checkingLanguage == false && main.currentLanguage != languageUsedBefore)
                MessageBox.Show(Properties.Resources.ChangedLanguageMessage, main.Title, MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private void DockButton2_Click(object sender, RoutedEventArgs e)
        {
            string speechbank = main.currentSpeechbank;
            if (speechbank == Properties.Resources.Default)
                speechbank = "<default>";
            var sw = new StreamWriter(new FileStream(configFileName, FileMode.Create), Encoding.UTF8);
            sw.WriteLine("; Don't modify this file manually! Nie modyfikować tego pliku ręcznie! Modifizieren Sie nicht diese Datei manuell!\r\n[Przeliterowywacz]\r\nLanguage=" + main.currentLanguage + "\r\nIncludeDiacriticalChars=" + Convert.ToInt16(main.includeDiacriticalChars) + "\r\nDeriveFromDefaultSpeechbank=" + Convert.ToInt16(main.deriveFromDefaultSpeechbank) + "\r\nUseSAPI5=" + Convert.ToInt16(main.useSapi) + "\r\nReadZeroAsO=" + Convert.ToInt16(main.readZeroAsO) + "\r\nRate=" + main.sapiRate + "\r\nVolume=" + main.sapiVolume + "\r\nInputScheme=" + main.inputScheme + "\r\nSpeechbank=" + speechbank + "\r\nSAPI5Voice=" + main.currentSapi); // Spisywanie konfiguracji domyślnej na plik o stronie kodowej UTF-8 (poprzednio Windows-1250 - Encoding.GetEncoding(1250))
            sw.Close();
        }
    }
}
