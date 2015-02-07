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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Media;
using System.Threading;
using System.IO;
using System.Globalization;
using System.Windows.Markup;
using NAudio;
using NAudio.Wave;
using System.Speech.Synthesis;

namespace Przeliterowywacz
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string configFileName = "Przeliterowywacz.ini"; // nazwa pliku konfiguracyjnego
        // Konfiguracja domyślna
        public bool includeDiacriticalChars = true;
        public bool deriveFromDefaultSpeechbank = false;
        public bool useSapi = false;
        public bool readZeroAsO = false;
        public Int16 sapiRate = 0;
        public Int16 sapiVolume = 100;
        public Int16 inputScheme = 0;
        public string currentSpeechbank = "<default>";
        public string currentSapi = "Microsoft Anna";
        public string currentLanguage = "en";

        public MainWindow()
        {
            if (File.Exists(configFileName)) // Odczytywanie pliku konfiguracyjnego (o ile istnieje)
            {
                var sr = new StreamReader(configFileName);
                string srLine;
                while ((srLine = sr.ReadLine()) != null)
                {
                    if (srLine.Contains("Language="))
                    {
                        currentLanguage = srLine.Substring(9);
                        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(currentLanguage);
                        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(currentLanguage);
                        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
                    }
                    else if (srLine.Contains("IncludeDiacriticalChars="))
                        includeDiacriticalChars = Convert.ToBoolean(Convert.ToInt16(srLine.Substring(24)));
                    else if (srLine.Contains("DeriveFromDefaultSpeechbank="))
                        deriveFromDefaultSpeechbank = Convert.ToBoolean(Convert.ToInt16(srLine.Substring(28)));
                    else if (srLine.Contains("UseSAPI5="))
                        useSapi = Convert.ToBoolean(Convert.ToInt16(srLine.Substring(9)));
                    else if (srLine.Contains("ReadZeroAsO="))
                        readZeroAsO = Convert.ToBoolean(Convert.ToInt16(srLine.Substring(12)));
                    else if (srLine.Contains("Rate="))
                    {
                        sapiRate = Convert.ToInt16(srLine.Substring(5));
                        if (sapiVolume > 10)
                            sapiVolume = 10;
                        else if (sapiVolume < -10)
                            sapiVolume = -10;
                    }
                    else if (srLine.Contains("Volume="))
                    {
                        sapiVolume = Convert.ToInt16(srLine.Substring(7));
                        if (sapiVolume > 100)
                            sapiVolume = 100;
                        else if (sapiVolume < 0)
                            sapiVolume = 0;
                    }
                    else if (srLine.Contains("InputScheme="))
                        inputScheme = Convert.ToInt16(srLine.Substring(12));
                    else if (srLine.Contains("Speechbank="))
                    {
                        currentSpeechbank = srLine.Substring(11);
                        if (currentSpeechbank == "<default>")
                            currentSpeechbank = Properties.Resources.Default;
                    }
                    else if (srLine.Contains("SAPI5Voice="))
                        currentSapi = srLine.Substring(11);
                }
                sr.Close();
            }
            else
            {
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(currentLanguage);
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(currentLanguage);
                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
                //var sw = new StreamWriter(new FileStream(configFileName, FileMode.CreateNew), Encoding.UTF8);
                //sw.WriteLine("; Don't modify this file manually! Nie modyfikować tego pliku ręcznie! Modifizieren Sie nicht diese Datei manuell!\r\n[Przeliterowywacz]\r\nLanguage=en\r\nIncludeDiacriticalChars=1\r\nDeriveFromDefaultSpeechbank=0\r\nInputScheme=0\r\nSpeechbank=<default>"); // Spisywanie konfiguracji domyślnej na plik o stronie kodowej Windows-1250
                //sw.Close();
            }
            InitializeComponent();
            if (inputScheme == 1)
            {
                TextBox1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                TextBox1.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"));
            }
            else if (inputScheme == 2)
            {
                TextBox1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"));
                TextBox1.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FF00"));
            }
            if (!File.Exists("NAudio.dll")) // Sprawdzanie, czy w katalogu programu jest biblioteka NAudio.dll
            {
                MenuItem2.IsEnabled = false;
                MessageBox.Show(Properties.Resources.NAudioNotFoundMessage, MainSpeechWindow.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        public bool firstTimeRunning = true, isQuiet = true;
        OptionsWindow ow;
        AboutWindow aw;
        Thread T1;
        SpeechSynthesizer sapi = new SpeechSynthesizer();

        public static void Concatenate(string outputFile, IEnumerable<string> sourceFiles)
        {
            byte[] buffer = new byte[1024];
            WaveFileWriter waveFileWriter = null;

            try
            {
                foreach (string sourceFile in sourceFiles)
                {
                    using (WaveFileReader reader = new WaveFileReader(sourceFile))
                    {
                        if (waveFileWriter == null)
                            waveFileWriter = new WaveFileWriter(outputFile, reader.WaveFormat);
                        else
                        {
                            if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat))
                                throw new InvalidOperationException(Properties.Resources.CannotConcatenateDifferentFormatsException);
                        }

                        int read;
                        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                            waveFileWriter.WriteData(buffer, 0, read);
                    }
                }
            }
            finally
            {
                if (waveFileWriter != null)
                    waveFileWriter.Dispose();
            }
        }

        public string specifyFileName(int i, string toBeSaid)
        {
            string fileName = "Banki\\";
            if (currentSpeechbank != Properties.Resources.Default)
                fileName += currentSpeechbank + '\\';
            if (toBeSaid.Substring(i, 1) == " ")
                fileName = null;
            else if (toBeSaid.Substring(i, 1) == "0" && readZeroAsO)
                fileName += "o.wav";
            else if (toBeSaid.Substring(i, 1) == "." || toBeSaid.Substring(i, 1) == "," || toBeSaid.Substring(i, 1) == ":" || toBeSaid.Substring(i, 1) == ";" || toBeSaid.Substring(i, 1) == "!" || toBeSaid.Substring(i, 1) == "?" || toBeSaid.Substring(i, 1) == "'" || toBeSaid.Substring(i, 1) == "\"" || toBeSaid.Substring(i, 1) == "\\" || toBeSaid.Substring(i, 1) == "/" || toBeSaid.Substring(i, 1) == "%" || toBeSaid.Substring(i, 1) == "*" || toBeSaid.Substring(i, 1) == "|" || toBeSaid.Substring(i, 1) == "<" || toBeSaid.Substring(i, 1) == ">" || toBeSaid.Substring(i, 1) == "=")
            {
                if (includeDiacriticalChars)
                {
                    if (toBeSaid.Substring(i, 1) == ".")
                        fileName += "kropka.wav";
                    else if (toBeSaid.Substring(i, 1) == ",")
                        fileName += "przecinek.wav";
                    else if (toBeSaid.Substring(i, 1) == ":")
                        fileName += "dwukropek.wav";
                    else if (toBeSaid.Substring(i, 1) == ";")
                        fileName += "srednik.wav";
                    else if (toBeSaid.Substring(i, 1) == "!")
                        fileName += "wykrzyknik.wav";
                    else if (toBeSaid.Substring(i, 1) == "?")
                        fileName += "pytajnik.wav";
                    else if (toBeSaid.Substring(i, 1) == "'")
                        fileName += "apostrof.wav";
                    else if (toBeSaid.Substring(i, 1) == "\"")
                        fileName += "cudzyslow.wav";
                    else if (toBeSaid.Substring(i, 1) == "\\")
                        fileName += "ukosnik_wsteczny.wav";
                    else if (toBeSaid.Substring(i, 1) == "/")
                        fileName += "ukosnik.wav";
                    else if (toBeSaid.Substring(i, 1) == "%")
                        fileName += "procent.wav";
                    else if (toBeSaid.Substring(i, 1) == "*")
                        fileName += "gwiazdka.wav";
                    else if (toBeSaid.Substring(i, 1) == "|")
                        fileName += "kreska_pionowa.wav";
                    else if (toBeSaid.Substring(i, 1) == "<")
                        fileName += "znak_mniejszosci.wav";
                    else if (toBeSaid.Substring(i, 1) == ">")
                        fileName += "znak_wiekszosci.wav";
                    else if (toBeSaid.Substring(i, 1) == "=")
                        fileName += "znak_rownosci.wav";
                }
                else
                    fileName = null;
            }
            else
                fileName += toBeSaid.Substring(i, 1).ToLower() + ".wav";
            return fileName;
        }

        public void spellAsSapi(int i, string toBeSaid)
        {
            string letterToSpeak = toBeSaid.Substring(i, 1);
            if (letterToSpeak == "0" && readZeroAsO)
                sapi.Speak("o");
            else
                sapi.Speak(letterToSpeak);
        }

        void F1(object txt)
        {
            string toBeSpelled = (string)txt, waveFileName = "";
            SoundPlayer letterFile;
            try
            {
                sapi.SelectVoice(currentSapi);
                sapi.Rate = sapiRate;
                sapi.Volume = sapiVolume;
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format(Properties.Resources.SAPI5VoiceMissingMessage, currentSapi), Properties.Resources.ErrorCaption, MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            try
            {
                for (int i = 0; i < toBeSpelled.Length; i++)
                {
                    waveFileName = specifyFileName(i, toBeSpelled);
                    if (isQuiet)
                        break;
                    if (waveFileName != null)
                    {
                        if (useSapi)
                        {
                            try 
	                        {
                                spellAsSapi(i, toBeSpelled);
	                        }
	                        catch (ArgumentNullException)
	                        {
                                continue;
	                        }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        else if (!deriveFromDefaultSpeechbank)
                        {
                            letterFile = new SoundPlayer((string)waveFileName);
                            letterFile.PlaySync();
                        }
                        else
                        {
                            string actualSpeechbank = currentSpeechbank;
                            if (!File.Exists(waveFileName))
                                currentSpeechbank = Properties.Resources.Default;
                            waveFileName = specifyFileName(i, toBeSpelled);
                            letterFile = new SoundPlayer((string)waveFileName);
                            letterFile.PlaySync();
                            currentSpeechbank = actualSpeechbank;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!useSapi)
                    MessageBox.Show(String.Format(Properties.Resources.HaltedDueToFileMessage, AppDomain.CurrentDomain.BaseDirectory, waveFileName), Properties.Resources.ErrorCaption, MessageBoxButton.OK, MessageBoxImage.Hand);
                else
                    MessageBox.Show(Properties.Resources.SAPI5Error + ex.Message, Properties.Resources.ErrorCaption, MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            isQuiet = true;
        }

        private void MenuItem1_Click(object sender, RoutedEventArgs e)
        {
            if (!firstTimeRunning)
            {
                if (isQuiet)
                {
                    if (ow != null)
                        ow.Close();
                    string textToSpell = TextBox1.Text;
                    isQuiet = false;
                    Button3.IsEnabled = true;
                    T1 = new Thread(F1);
                    T1.Start(textToSpell);
                }
                else
                    MessageBox.Show(Properties.Resources.StillWorkingMessage, MainSpeechWindow.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
                MessageBox.Show(Properties.Resources.NoTextToSpellMessage, MainSpeechWindow.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void TextBox1_GotMouseCapture(object sender, MouseEventArgs e)
        {
            if (firstTimeRunning)
            {
                TextBox1.Text = "";
                firstTimeRunning = false;
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (ow != null)
            {
                ow.Close();
                ow = null;
            }
            if (isQuiet)
            {
                ow = new OptionsWindow(configFileName, includeDiacriticalChars, deriveFromDefaultSpeechbank, useSapi, readZeroAsO, sapiRate, sapiVolume, currentSpeechbank, currentSapi, currentLanguage);
                ow.Show();
            }
            else
                MessageBox.Show(Properties.Resources.IsBusyMessage, MainSpeechWindow.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private async void MenuItem2_Click(object sender, RoutedEventArgs e)
        {
            if (!firstTimeRunning)
            {
                var FD = new Microsoft.Win32.SaveFileDialog();
                FD.FileName = "*";
                FD.DefaultExt = "wav";
                FD.ValidateNames = true;
                FD.Filter = Properties.Resources.FileTypes;

                Nullable<bool> result = FD.ShowDialog();
                if (result == true)
                {
                    string textToRecord = TextBox1.Text, waveFileName = "";
                    List<string> filesToPlayList = new List<string>();
                    bool wasFailed = false;

                    if (!useSapi)
	                {
                        for (int i = 0; i < textToRecord.Length; i++)
                        {
                            waveFileName = specifyFileName(i, textToRecord);
                            if (waveFileName != null)
                            {
                                if (File.Exists(waveFileName))
                                    filesToPlayList.Add(waveFileName);
                                else
                                {
                                    string actualSpeechbank = currentSpeechbank;
                                    currentSpeechbank = Properties.Resources.Default;
                                    waveFileName = specifyFileName(i, textToRecord);
                                    currentSpeechbank = actualSpeechbank;
                                    if (File.Exists(waveFileName) && deriveFromDefaultSpeechbank)
                                        filesToPlayList.Add(waveFileName);
                                    else
                                    {
                                        wasFailed = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else //Zapisywanie nagrań SAPI5
                    {
                        try
                        {
                            sapi.SelectVoice(currentSapi);
                            sapi.Rate = sapiRate;
                            sapi.Volume = sapiVolume;
                            sapi.SetOutputToWaveFile(FD.FileName);
                            for (int i = 0; i < textToRecord.Length; i++)
                            {
                                try
                                {
                                    spellAsSapi(i, textToRecord);
                                }
                                catch (ArgumentNullException)
	                            {
                                    continue;
                                }
                                catch (Exception)
                                {
                                    throw;
                                }
                            }
                            sapi.SetOutputToDefaultAudioDevice();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(Properties.Resources.SAPI5Error + ex.Message, Properties.Resources.ErrorCaption, MessageBoxButton.OK, MessageBoxImage.Hand);
                            wasFailed = true;
                        }
                    }

                    if (!wasFailed)
                    {
                        if (!useSapi)
                            Concatenate(FD.FileName, filesToPlayList);
                        if (File.Exists(FD.FileName))
                        {
                            StatusLabel.Content = Properties.Resources.SavedStatus;
                            await Task.Delay(5000);
                            StatusLabel.Content = "";
                        }
                    }
                    else
                    {
                        if (!useSapi)
                            MessageBox.Show(Properties.Resources.WAVEFilesMissingMessage, Properties.Resources.ErrorCaption, MessageBoxButton.OK, MessageBoxImage.Hand);
                        StatusLabel.Content = Properties.Resources.FailedStatus;
                        await Task.Delay(5000);
                        StatusLabel.Content = "";
                    }
                }
            }
            else
                MessageBox.Show(Properties.Resources.NoTextToRecordMessage, MainSpeechWindow.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void MainSpeechWindow_Closed(object sender, EventArgs e)
        {
            isQuiet = true;
            if (ow != null)
                ow.Close();
            if (aw != null)
                aw.Close();
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            isQuiet = true;
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            if (aw != null)
            {
                aw.Close();
                aw = null;
            }
            aw = new AboutWindow();
            aw.Show();
        }
    }
}
