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
using System.Diagnostics;
using System.Windows.Interop;
using System.Speech.AudioFormat;

namespace Speller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string configFileName = "Speller.ini"; // the config file name
        // Default configuration
        public bool includeDiacriticalChars = true;
        public bool deriveFromDefaultSpeechbank = false;
        public bool readZeroAsO = false;
        public bool spellWithAltSHotkey = true;
        public Int16 sapiRate = 0;
        public Int16 sapiVolume = 100;
        public int sampleRate = 44100;
        public Int16 bitDepth = 16;
        public Int16 channels = 1;
        public int delayBetweenCharsInMs = 0;
        public bool useSapi = false;
        public Int16 inputScheme = 0;
        public string currentSpeechbank = "<default>";
        public string currentSapi = "Microsoft Anna";
        public string currentLanguage = "en";

        IntPtr hWnd;

        public void RegisterAltSHotkey()
        {
            User32.RegisterHotKey(hWnd, 0, (int)User32.KeyModifier.Alt, (int)User32.Keys.S);
        }

        public void UnregisterAltSHotkey()
        {
            User32.UnregisterHotKey(hWnd, 0);
        }

        public MainWindow()
        {
            if (File.Exists(configFileName)) // Reading the configuration file (if it exists)
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
                    else if (srLine.Contains("ReadZeroAsO="))
                        readZeroAsO = Convert.ToBoolean(Convert.ToInt16(srLine.Substring(12)));
                    else if (srLine.Contains("SpellWithAltSHotkey="))
                        spellWithAltSHotkey = Convert.ToBoolean(Convert.ToInt16(srLine.Substring(20)));
                    else if (!srLine.Contains("SampleRate=") && srLine.Contains("Rate="))
                    {
                        sapiRate = Convert.ToInt16(srLine.Substring(5));
                        if (sapiRate > 10)
                            sapiRate = 10;
                        else if (sapiRate < -10)
                            sapiRate = -10;
                    }
                    else if (srLine.Contains("Volume="))
                    {
                        sapiVolume = Convert.ToInt16(srLine.Substring(7));
                        if (sapiVolume > 100)
                            sapiVolume = 100;
                        else if (sapiVolume < 0)
                            sapiVolume = 0;
                    }
                    else if (srLine.Contains("SampleRate="))
                        sampleRate = Convert.ToInt32(srLine.Substring(11));
                    else if (srLine.Contains("BitDepth="))
                        bitDepth = Convert.ToInt16(srLine.Substring(9));
                    else if (srLine.Contains("Channels="))
                        channels = Convert.ToInt16(srLine.Substring(9));
                    else if (srLine.Contains("UseSAPI5="))
                        useSapi = Convert.ToBoolean(Convert.ToInt16(srLine.Substring(9)));
                    else if (srLine.Contains("DelayBetweenCharsInMs="))
                        delayBetweenCharsInMs = Convert.ToInt32(srLine.Substring(22));
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
            }
            InitializeComponent();
            if (inputScheme == 1)
            {
                MainTextBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                MainTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"));
            }
            else if (inputScheme == 2)
            {
                MainTextBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF000000"));
                MainTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00FF00"));
            }
            if (!File.Exists("NAudio.dll")) // Looking up for NAudio.dll in the program's directory
            {
                RecordMenuItem.IsEnabled = false;
                MessageBox.Show(Properties.Resources.NAudioNotFoundMessage, MainSpeechWindow.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        public bool firstTimeRunning = true, isQuiet = true;
        OptionsWindow ow;
        AboutWindow aw;
        Thread T1;
        SpeechSynthesizer sapi = new SpeechSynthesizer();
        WaveStream waveStream;
        WaveOut waveOut;

        public void Concatenate(string outputFile, IEnumerable<string> sourceFiles)
        {
            WaveFormat targetWaveFormat = new WaveFormat(sampleRate, bitDepth, channels);
            byte[] buffer = new byte[1024];
            WaveFileWriter waveFileWriter = null;

            try
            {
                foreach (string sourceFile in sourceFiles)
                {
                    using (WaveStream reader = WaveFormatConversionStream.CreatePcmStream(new WaveFileReader(sourceFile)))
                    {
                        if (waveFileWriter == null)
                            waveFileWriter = new WaveFileWriter(outputFile, targetWaveFormat);

                        using (var conversionStream = new WaveFormatConversionStream(targetWaveFormat, reader))
                        {
                            int read;
                            while ((read = conversionStream.Read(buffer, 0, buffer.Length)) > 0)
                                waveFileWriter.WriteData(buffer, 0, read);
                        }
                    }
                }
            }
            finally
            {
                if (waveFileWriter != null)
                    waveFileWriter.Dispose();
            }
        }

        public string SpecifyFileName(int i, string toBeSaid)
        {
            string fileName = "Banks\\";
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
                        fileName += "period.wav";
                    else if (toBeSaid.Substring(i, 1) == ",")
                        fileName += "comma.wav";
                    else if (toBeSaid.Substring(i, 1) == ":")
                        fileName += "colon.wav";
                    else if (toBeSaid.Substring(i, 1) == ";")
                        fileName += "semicolon.wav";
                    else if (toBeSaid.Substring(i, 1) == "!")
                        fileName += "exclamation_mark.wav";
                    else if (toBeSaid.Substring(i, 1) == "?")
                        fileName += "question_mark.wav";
                    else if (toBeSaid.Substring(i, 1) == "'")
                        fileName += "apostrophe.wav";
                    else if (toBeSaid.Substring(i, 1) == "\"")
                        fileName += "quotation_mark.wav";
                    else if (toBeSaid.Substring(i, 1) == "\\")
                        fileName += "backslash.wav";
                    else if (toBeSaid.Substring(i, 1) == "/")
                        fileName += "slash.wav";
                    else if (toBeSaid.Substring(i, 1) == "%")
                        fileName += "percent_sign.wav";
                    else if (toBeSaid.Substring(i, 1) == "*")
                        fileName += "asterisk.wav";
                    else if (toBeSaid.Substring(i, 1) == "|")
                        fileName += "vertical_bar.wav";
                    else if (toBeSaid.Substring(i, 1) == "<")
                        fileName += "less_than.wav";
                    else if (toBeSaid.Substring(i, 1) == ">")
                        fileName += "greater_than.wav";
                    else if (toBeSaid.Substring(i, 1) == "=")
                        fileName += "equal.wav";
                }
                else
                    fileName = null;
            }
            else
                fileName += toBeSaid.Substring(i, 1).ToLower() + ".wav";
            return fileName;
        }

        public void SpellAsSapi(int i, string toBeSaid)
        {
            string letterToSpeak = toBeSaid.Substring(i, 1);
            if (letterToSpeak == "0" && readZeroAsO)
                sapi.Speak("o");
            else
                sapi.Speak(letterToSpeak);
        }

        public void SpellUsingSoundbank(string sourceFile, TimeSpan delayTime)
        {
            waveStream = WaveFormatConversionStream.CreatePcmStream(new WaveFileReader(sourceFile));
            waveOut = new WaveOut();
            waveOut.Init(waveStream);
            TimeSpan totalCharTime = waveStream.TotalTime + delayTime;
            waveOut.Play();
            Stopwatch sw = Stopwatch.StartNew();
            TimeSpan ts = sw.Elapsed;
            while (ts < totalCharTime)
                ts = sw.Elapsed;
            sw.Stop();
            waveOut.Dispose();
            waveStream.Dispose();
        }

        void F1(object txt)
        {
            string toBeSpelled = (string)txt, waveFileName = "";
            SoundPlayer letterFile;
            TimeSpan delayTimeSpan = TimeSpan.FromMilliseconds(delayBetweenCharsInMs);
            try
            {
                sapi.SelectVoice(currentSapi);
                sapi.Rate = sapiRate;
                sapi.Volume = sapiVolume;
            }
            catch (Exception)
            {
                MessageBox.Show(String.Format(Properties.Resources.SAPI5VoiceMissingMessage, currentSapi), Properties.Resources.ErrorCaption, MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            try
            {
                for (int i = 0; i < toBeSpelled.Length; i++)
                {
                    Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        StatusLabel.Content = Properties.Resources.PlayingStatus;
                    }));
                    waveFileName = SpecifyFileName(i, toBeSpelled);
                    if (isQuiet)
                        break;
                    if (waveFileName != null)
                    {
                        if (useSapi)
                        {
                            try
                            {
                                SpellAsSapi(i, toBeSpelled);
                                Thread.Sleep(delayBetweenCharsInMs);
                            }
                            catch (ArgumentNullException) // eSpeak common problem fix
                            {
                                continue;
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        else if (!deriveFromDefaultSpeechbank)
                            SpellUsingSoundbank(waveFileName, delayTimeSpan);
                        else
                        {
                            string actualSpeechbank = currentSpeechbank;
                            if (!File.Exists(waveFileName))
                                currentSpeechbank = Properties.Resources.Default;
                            waveFileName = SpecifyFileName(i, toBeSpelled);
                            SpellUsingSoundbank(waveFileName, delayTimeSpan);
                            currentSpeechbank = actualSpeechbank;
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (!useSapi)
                    MessageBox.Show(String.Format(Properties.Resources.HaltedDueToFileMessage, AppDomain.CurrentDomain.BaseDirectory, waveFileName), Properties.Resources.ErrorCaption, MessageBoxButton.OK, MessageBoxImage.Hand);
                else
                    MessageBox.Show(Properties.Resources.SAPI5Error, Properties.Resources.ErrorCaption, MessageBoxButton.OK, MessageBoxImage.Hand);
                Dispatcher.BeginInvoke(new Action(delegate()
                {
                    StatusLabel.Content = "";
                }));
            }
            isQuiet = true;
            Dispatcher.BeginInvoke(new Action(delegate()
            {
                StatusLabel.Content = "";
            }));
        }

        public void DoSpellingTask(string textToSpell, bool isExternallyCalled = false)
        {
            if (!firstTimeRunning || isExternallyCalled)
            {
                if (isQuiet)
                {
                    if (ow != null)
                        ow.Close();
                    isQuiet = false;
                    StopButton.IsEnabled = true;
                    T1 = new Thread(F1);
                    T1.Start(textToSpell);
                }
                else
                    MessageBox.Show(Properties.Resources.StillWorkingMessage, MainSpeechWindow.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
                MessageBox.Show(Properties.Resources.NoTextToSpellMessage, MainSpeechWindow.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void PlayMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DoSpellingTask(MainTextBox.Text);
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ow != null)
            {
                ow.Close();
                ow = null;
            }
            if (isQuiet)
            {
                ow = new OptionsWindow(configFileName, includeDiacriticalChars, deriveFromDefaultSpeechbank, readZeroAsO, spellWithAltSHotkey, sapiRate, sapiVolume, sampleRate, bitDepth, channels, delayBetweenCharsInMs, useSapi, currentSpeechbank, currentSapi, currentLanguage);
                ow.Show();
            }
            else
                MessageBox.Show(Properties.Resources.IsBusyMessage, MainSpeechWindow.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private async void RecordMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!firstTimeRunning)
            {
                var sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.FileName = "*";
                sfd.DefaultExt = "wav";
                sfd.ValidateNames = true;
                sfd.Filter = Properties.Resources.FileTypes;

                Nullable<bool> result = sfd.ShowDialog();
                if (result == true)
                {
                    string textToRecord = MainTextBox.Text, waveFileName = "";
                    List<string> filesToPlayList = new List<string>();
                    bool wasFailed = false;

                    if (!useSapi)
                    {
                        for (int i = 0; i < textToRecord.Length; i++)
                        {
                            waveFileName = SpecifyFileName(i, textToRecord);
                            if (waveFileName != null)
                            {
                                if (File.Exists(waveFileName))
                                    filesToPlayList.Add(waveFileName);
                                else
                                {
                                    string actualSpeechbank = currentSpeechbank;
                                    currentSpeechbank = Properties.Resources.Default;
                                    waveFileName = SpecifyFileName(i, textToRecord);
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
                    else // Saving SAPI5 recordings
                    {
                        try
                        {
                            sapi.SelectVoice(currentSapi);
                            sapi.Rate = sapiRate;
                            sapi.Volume = sapiVolume;
                            AudioBitsPerSample sapiBitDepth;
                            if (bitDepth == 16)
                                sapiBitDepth = AudioBitsPerSample.Sixteen;
                            else
                                sapiBitDepth = AudioBitsPerSample.Eight;
                            AudioChannel sapiChannels;
                            if (channels == 2)
                                sapiChannels = AudioChannel.Stereo;
                            else
                                sapiChannels = AudioChannel.Mono;
                            SpeechAudioFormatInfo sapiFormatInfo = new SpeechAudioFormatInfo(sampleRate, sapiBitDepth, sapiChannels);
                            sapi.SetOutputToWaveFile(sfd.FileName, sapiFormatInfo);
                            for (int i = 0; i < textToRecord.Length; i++)
                            {
                                try
                                {
                                    SpellAsSapi(i, textToRecord);
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
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(Properties.Resources.SAPI5Error + ex.Message, Properties.Resources.ErrorCaption, MessageBoxButton.OK, MessageBoxImage.Hand);
                            wasFailed = true;
                        }
                        sapi.SetOutputToDefaultAudioDevice();
                    }

                    if (!wasFailed)
                    {
                        if (!useSapi)
                            Concatenate(sfd.FileName, filesToPlayList);
                        if (File.Exists(sfd.FileName))
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

        private void MainSpeechWindow_Closing(object sender, EventArgs e)
        {
            UnregisterAltSHotkey();
            isQuiet = true;
            if (ow != null)
                ow.Close();
            if (aw != null)
                aw.Close();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            isQuiet = true;
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            if (aw != null)
            {
                aw.Close();
                aw = null;
            }
            aw = new AboutWindow();
            aw.ShowDialog();
        }

        private void MainTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (firstTimeRunning)
            {
                MainTextBox.Text = "";
                firstTimeRunning = false;
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0312)
            {
                try
                {
                    string clipboardTextToSpell = Clipboard.GetText();
                    if (clipboardTextToSpell != null && clipboardTextToSpell != "")
                        DoSpellingTask(clipboardTextToSpell, true);
                }
                catch (AccessViolationException) // happens mostly when the Clipboard is completely empty
                {
                    ;
                }
            }

            return IntPtr.Zero;
        }

        private void HookWndProc(Visual window)
        {
            var source = PresentationSource.FromVisual(window) as HwndSource;
            if (source == null) throw new Exception("Could not create hWnd source from window.");
            source.AddHook(WndProc);
        }

        private void MainSpeechWindow_Loaded(object sender, RoutedEventArgs e)
        {
            hWnd = new WindowInteropHelper(this).Handle;
            HookWndProc(this);
            if (spellWithAltSHotkey)
                RegisterAltSHotkey();
        }
    }
}
