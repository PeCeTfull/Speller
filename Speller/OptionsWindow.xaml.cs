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

namespace Speller
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        MainWindow main = (MainWindow)Application.Current.MainWindow;
        string configFileName;
        bool checkingLanguage;

        private void EnableCustomSampleRate()
        {
            CustomSampleRateNumberBox.IsEnabled = true;
            CustomSampleRateButtonOneMore.IsEnabled = true;
            CustomSampleRateButtonOneLess.IsEnabled = true;
            SampleRateComboBox.IsEnabled = false;
        }

        private void DisableCustomSampleRate()
        {
            CustomSampleRateNumberBox.IsEnabled = false;
            CustomSampleRateButtonOneMore.IsEnabled = false;
            CustomSampleRateButtonOneLess.IsEnabled = false;
            SampleRateComboBox.IsEnabled = true;
        }

        public OptionsWindow(string configFileName, bool diacriticalOption, bool derivativeOption, bool zeroAsOLetterOption, bool altSHotkeyOption, Int16 rateNumber, Int16 volumeNumber, int sampleRateNumber, Int16 bitDepthNumber, Int16 channelsAmount, int delayBetweenCharsNumber, bool sapiOption, string speechbankName, string sapiName, string languageCode)
        {
            this.configFileName = configFileName;
            InitializeComponent();
            EnableSpecialCharsCheckBox.IsChecked = diacriticalOption;
            DeriveFromDefaultSpeechbankCheckBox.IsChecked = derivativeOption;
            ReadZeroAsOLetterCheckBox.IsChecked = zeroAsOLetterOption;
            SpellWithAltSHotkeyCheckBox.IsChecked = altSHotkeyOption;
            SapiRateNumberBox.Text = rateNumber.ToString();
            VolumeNumberBox.Text = volumeNumber.ToString();
            if (sampleRateNumber != 8000 && sampleRateNumber != 11025 && sampleRateNumber != 12000 && sampleRateNumber != 16000 && sampleRateNumber != 22050 && sampleRateNumber != 24000 && sampleRateNumber != 32000 && sampleRateNumber != 44100 && sampleRateNumber != 48000)
            {
                CustomSampleRateCheckBox.IsChecked = true;
                EnableCustomSampleRate();
            }
            else
            {
                CustomSampleRateCheckBox.IsChecked = false;
                DisableCustomSampleRate();
                if (sampleRateNumber == 48000)
                    SampleRateComboBox.SelectedIndex = 8;
                else if (sampleRateNumber == 44100)
                    SampleRateComboBox.SelectedIndex = 7;
                else if (sampleRateNumber == 32000)
                    SampleRateComboBox.SelectedIndex = 6;
                else if (sampleRateNumber == 24000)
                    SampleRateComboBox.SelectedIndex = 5;
                else if (sampleRateNumber == 22050)
                    SampleRateComboBox.SelectedIndex = 4;
                else if (sampleRateNumber == 16000)
                    SampleRateComboBox.SelectedIndex = 3;
                else if (sampleRateNumber == 12000)
                    SampleRateComboBox.SelectedIndex = 2;
                else if (sampleRateNumber == 11025)
                    SampleRateComboBox.SelectedIndex = 1;
                else
                    SampleRateComboBox.SelectedIndex = 0;
            }
            CustomSampleRateNumberBox.Text = sampleRateNumber.ToString();
            if (bitDepthNumber == 16)
                BitDepthComboBox.SelectedIndex = 1;
            else
                BitDepthComboBox.SelectedIndex = 0;
            if (channelsAmount == 1)
                ChannelsComboBox.SelectedIndex = 0;
            else
                ChannelsComboBox.SelectedIndex = 1;
            DelayBetweenCharsNumberBox.Text = delayBetweenCharsNumber.ToString();
            if (sapiOption)
                SAPI5SynthesisRadioButton.IsChecked = sapiOption;
            else
                SpeechbanksRadioButton.IsChecked = !sapiOption;
            // Obtaining all the subfolder names found in the "Banks" folder and adding them to the list of available speechbanks
            ComboBoxItem cbi;
            string[] folderNames;
            try
            {
                folderNames = Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory + "Banks\\");
            }
            catch (DirectoryNotFoundException)
            {
                folderNames = new string[0];
                SAPI5SynthesisRadioButton.IsChecked = true;
                SpeechbanksRadioButton.IsEnabled = false;
                SpeechbankComboBox.IsEnabled = false;
                MessageBox.Show(String.Format(Properties.Resources.BanksFolderNotFoundMessage), Properties.Resources.ErrorCaption, MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            for (int i = 0; i < folderNames.Length; i++)
            {
                string[] folderNameStructure = folderNames[i].Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                string folderName = Properties.Resources.BanksFolderNotFound;
                for (int j = 0; j < folderNameStructure.Length - 1; j++)
                    if (folderNameStructure[j] == "Banks")
                    {
                        folderName = folderNameStructure[j + 1];
                        break;
                    }
                cbi = new ComboBoxItem();
                cbi.Content = folderName;
                SpeechbankComboBox.Items.Add(cbi);
            }
            // Verifying if the recently chosen speechbank still exists in the "Banks" folder
            for (int i = 0; i < SpeechbankComboBox.Items.Count; i++)
            {
                SpeechbankComboBox.SelectedIndex = i;
                if (SpeechbankComboBox.SelectedItem.ToString().Substring(38) == speechbankName)
                    break;
                if (i == SpeechbankComboBox.Items.Count - 1)
                {
                    SpeechbankComboBox.SelectedIndex = 0;
                    MessageBox.Show(String.Format(Properties.Resources.SpeechbankFolderNotFoundMessage, speechbankName), main.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            // Retrieving currently installed SAPI5 speech synthesizers in the system
            var allInstalledVoices = new SpeechSynthesizer().GetInstalledVoices();
            foreach (var voice in allInstalledVoices)
            {
                cbi = new ComboBoxItem();
                cbi.Content = voice.VoiceInfo.Name;
                SAPI5VoiceComboBox.Items.Add(cbi);
            }
            // Verifying if the recently chosen SAPI5 synthesizer still exists in the system
            for (int i = 0; i < SAPI5VoiceComboBox.Items.Count; i++)
            {
                SAPI5VoiceComboBox.SelectedIndex = i;
                if (SAPI5VoiceComboBox.SelectedItem.ToString().Substring(38) == sapiName)
                    break;
                if (i == SAPI5VoiceComboBox.Items.Count - 1)
                {
                    SAPI5VoiceComboBox.SelectedIndex = 0;
                    MessageBox.Show(String.Format(Properties.Resources.SAPI5VoiceNotFoundMessage, sapiName), main.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            // Checking which interface language is currently in use
            checkingLanguage = true;
            for (int i = 0; i < LanguageComboBox.Items.Count; i++)
            {
                LanguageComboBox.SelectedIndex = i;
                if (Convert.ToInt16(LanguageComboBox.SelectedIndex) == 1 && languageCode == "pl")
                    break;
                else if (Convert.ToInt16(LanguageComboBox.SelectedIndex) == 2 && languageCode == "de")
                    break;
                if (i == LanguageComboBox.Items.Count - 1)
                    LanguageComboBox.SelectedIndex = 0;
            }
            checkingLanguage = false;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            WindowIcon.Remove(this);
        }

        private void OKDockButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EnableSpecialCharsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            main.includeDiacriticalChars = true;
        }

        private void EnableSpecialCharsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            main.includeDiacriticalChars = false;
        }

        private void DeriveFromDefaultSpeechbankCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            main.deriveFromDefaultSpeechbank = true;
        }

        private void DeriveFromDefaultSpeechbankCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            main.deriveFromDefaultSpeechbank = false;
        }

        private void SpeechbanksRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            main.useSapi = false;
        }

        private void SAPI5SynthesisRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            main.useSapi = true;
        }

        private void ReadZeroAsOLetterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            main.readZeroAsO = true;
        }

        private void ReadZeroAsOLetterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            main.readZeroAsO = false;
        }

        private void SapiRateNumberBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Int16.TryParse(SapiRateNumberBox.Text, out main.sapiRate))
                SapiRateNumberBox.Text = main.sapiRate.ToString();
            else if (Convert.ToInt16(SapiRateNumberBox.Text) > 10)
                SapiRateNumberBox.Text = "10";
            else if (Convert.ToInt16(SapiRateNumberBox.Text) < -10)
                SapiRateNumberBox.Text = "-10";
        }

        private void VolumeNumberBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Int16.TryParse(VolumeNumberBox.Text, out main.sapiVolume))
                VolumeNumberBox.Text = main.sapiVolume.ToString();
            else if (Convert.ToInt16(VolumeNumberBox.Text) > 100)
                VolumeNumberBox.Text = "100";
            else if (Convert.ToInt16(VolumeNumberBox.Text) < 0)
                VolumeNumberBox.Text = "0";
        }

        private void CustomSampleRateNumberBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Int32.TryParse(CustomSampleRateNumberBox.Text, out main.sampleRate))
                CustomSampleRateNumberBox.Text = main.sampleRate.ToString();
            else if (Convert.ToInt32(CustomSampleRateNumberBox.Text) > 48000) // sample rate no higher than 48,000 kHz
                CustomSampleRateNumberBox.Text = "48000";
            else if (Convert.ToInt32(CustomSampleRateNumberBox.Text) < 1)
                CustomSampleRateNumberBox.Text = "1";
        }

        private void DelayBetweenCharsNumberBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Int32.TryParse(DelayBetweenCharsNumberBox.Text, out main.delayBetweenCharsInMs))
                DelayBetweenCharsNumberBox.Text = main.delayBetweenCharsInMs.ToString();
            else if (Convert.ToInt32(DelayBetweenCharsNumberBox.Text) > 20000) // up to 20 seconds for a delay
                DelayBetweenCharsNumberBox.Text = "20000";
            else if (Convert.ToInt32(DelayBetweenCharsNumberBox.Text) < 0)
                DelayBetweenCharsNumberBox.Text = "0";
        }

        private void SapiRateButtonOneMore_Click(object sender, RoutedEventArgs e)
        {
            main.sapiRate++;
            SapiRateNumberBox.Text = main.sapiRate.ToString();
        }

        private void SapiRateButtonOneLess_Click(object sender, RoutedEventArgs e)
        {
            main.sapiRate--;
            SapiRateNumberBox.Text = main.sapiRate.ToString();
        }

        private void VolumeButtonOneMore_Click(object sender, RoutedEventArgs e)
        {
            main.sapiVolume++;
            VolumeNumberBox.Text = main.sapiVolume.ToString();
        }

        private void VolumeButtonOneLess_Click(object sender, RoutedEventArgs e)
        {
            main.sapiVolume--;
            VolumeNumberBox.Text = main.sapiVolume.ToString();
        }

        private void CustomSampleRateButtonOneMore_Click(object sender, RoutedEventArgs e)
        {
            main.sampleRate++;
            CustomSampleRateNumberBox.Text = main.sampleRate.ToString();
        }

        private void CustomSampleRateButtonOneLess_Click(object sender, RoutedEventArgs e)
        {
            main.sampleRate--;
            CustomSampleRateNumberBox.Text = main.sampleRate.ToString();
        }

        private void DelayBetweenCharsButtonOneMore_Click(object sender, RoutedEventArgs e)
        {
            main.delayBetweenCharsInMs++;
            DelayBetweenCharsNumberBox.Text = main.delayBetweenCharsInMs.ToString();
        }

        private void DelayBetweenCharsButtonOneLess_Click(object sender, RoutedEventArgs e)
        {
            main.delayBetweenCharsInMs--;
            DelayBetweenCharsNumberBox.Text = main.delayBetweenCharsInMs.ToString();
        }

        private void StyleButton1_Click(object sender, RoutedEventArgs e)
        {
            main.MainTextBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7FFFFFC0"));
            main.MainTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
            main.inputScheme = 0;
        }

        private void StyleButton2_Click(object sender, RoutedEventArgs e)
        {
            main.MainTextBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
            main.MainTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"));
            main.inputScheme = 1;
        }

        private void StyleButton3_Click(object sender, RoutedEventArgs e)
        {
            main.MainTextBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"));
            main.MainTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FF00"));
            main.inputScheme = 2;
        }

        private void SpeechbankComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            main.currentSpeechbank = SpeechbankComboBox.SelectedItem.ToString().Substring(38);
        }

        private void SAPI5VoiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            main.currentSapi = SAPI5VoiceComboBox.SelectedItem.ToString().Substring(38);
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string languageUsedBefore = main.currentLanguage;
            if (Convert.ToInt16(LanguageComboBox.SelectedIndex) == 1)
                main.currentLanguage = "pl";
            else if (Convert.ToInt16(LanguageComboBox.SelectedIndex) == 2)
                main.currentLanguage = "de";
            else
                main.currentLanguage = "en";
            if (checkingLanguage == false && main.currentLanguage != languageUsedBefore)
                MessageBox.Show(Properties.Resources.ChangedLanguageMessage, main.Title, MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private void SaveDockButton_Click(object sender, RoutedEventArgs e)
        {
            string speechbank = main.currentSpeechbank;
            if (speechbank == Properties.Resources.Default)
                speechbank = "<default>";
            var sw = new StreamWriter(new FileStream(configFileName, FileMode.Create), Encoding.UTF8);
            sw.WriteLine("; Don't modify this file manually! Nie modyfikować tego pliku ręcznie! Modifizieren Sie nicht diese Datei manuell!\r\n[Przeliterowywacz]\r\nLanguage=" + main.currentLanguage + "\r\nIncludeDiacriticalChars=" + Convert.ToInt16(main.includeDiacriticalChars) + "\r\nDeriveFromDefaultSpeechbank=" + Convert.ToInt16(main.deriveFromDefaultSpeechbank) + "\r\nReadZeroAsO=" + Convert.ToInt16(main.readZeroAsO) + "\r\nSpellWithAltSHotkey=" + Convert.ToInt16(main.spellWithAltSHotkey) + "\r\nRate=" + main.sapiRate + "\r\nVolume=" + main.sapiVolume + "\r\nSampleRate=" + main.sampleRate + "\r\nBitDepth=" + main.bitDepth + "\r\nChannels=" + main.channels + "\r\nUseSAPI5=" + Convert.ToInt16(main.useSapi) + "\r\nDelayBetweenCharsInMs=" + Convert.ToInt16(main.delayBetweenCharsInMs) + "\r\nInputScheme=" + main.inputScheme + "\r\nSpeechbank=" + speechbank + "\r\nSAPI5Voice=" + main.currentSapi); // Spisywanie konfiguracji domyślnej na plik o stronie kodowej UTF-8 (poprzednio Windows-1250 - Encoding.GetEncoding(1250))
            sw.Close();
        }

        private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void CustomSampleRateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            EnableCustomSampleRate();
        }

        private void CustomSampleRateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DisableCustomSampleRate();
        }

        private void SampleRateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SampleRateComboBox.SelectedIndex == 8)
                main.sampleRate = 48000;
            else if (SampleRateComboBox.SelectedIndex == 7)
                main.sampleRate = 44100;
            else if (SampleRateComboBox.SelectedIndex == 6)
                main.sampleRate = 32000;
            else if (SampleRateComboBox.SelectedIndex == 5)
                main.sampleRate = 24000;
            else if (SampleRateComboBox.SelectedIndex == 4)
                main.sampleRate = 22050;
            else if (SampleRateComboBox.SelectedIndex == 3)
                main.sampleRate = 16000;
            else if (SampleRateComboBox.SelectedIndex == 2)
                main.sampleRate = 12000;
            else if (SampleRateComboBox.SelectedIndex == 1)
                main.sampleRate = 11025;
            else
                main.sampleRate = 8000;

            CustomSampleRateNumberBox.Text = main.sampleRate.ToString();
        }

        private void BitDepthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BitDepthComboBox.SelectedIndex == 1)
                main.bitDepth = 16;
            else
                main.bitDepth = 8;
        }

        private void ChannelsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChannelsComboBox.SelectedIndex == 1)
                main.channels = 2;
            else
                main.channels = 1;
        }

        private void SpellWithAltSHotkeyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            main.spellWithAltSHotkey = true;
            main.RegisterAltSHotkey();
        }

        private void SpellWithAltSHotkeyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            main.spellWithAltSHotkey = false;
            main.UnregisterAltSHotkey();
        }
    }
}
