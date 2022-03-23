using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace JA_BMP_TO_ASCII
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport(@"C:\Users\Blazej\source\repos\JA_BMP_TO_ASCII\x64\Release\Dll_cpp.dll")]
        static extern void cppASCIIArt(int lenght, IntPtr byteArray);

        [DllImport(@"C:\Users\Blazej\source\repos\JA_BMP_TO_ASCII\x64\Release\ASM_DLL.dll")]
        unsafe static extern void ASCIIArt(int lenght, byte* byteArray);

        public string pathImg;
        public int height;
        public int width;
        public string outputTxtFile;
        public string inputPath;
        public IntPtr rowsPtrs;
        public BitmapImage bitmap;
        public byte[] convertedBitImg;
        public int offset;
        unsafe int* intPoint;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        unsafe private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (((BitmapImage)ImageControl.Source).UriSource == null)
                {
                    ErrorLabel.Content = "Choose Image!";
                }
                else
                {
                    ErrorLabel.Content = "";
                    if (AsmCheckbox.IsChecked == false && CppCheckbox.IsChecked == false)
                    {
                        ErrorLabel.Content = "Choose Language!";
                    }
                    else
                    {
                        ErrorLabel.Content = "";
                        int numberOfTasks = getNumberOfThreads();
                        if (numberOfTasks <= 0 || numberOfTasks > 64)
                        {
                            ErrorLabel.Content = "Wrong number of threads!";
                        }
                        else
                        {
                            int cores = Environment.ProcessorCount;
                            CoreCount.Content = "Liczba wbudowanych rdzeni: " + cores;
                            ErrorLabel.Content = "";
                            convertedBitImg = File.ReadAllBytes(pathImg);
                            offset = convertedBitImg[10];
                            rowsPtrs = Marshal.AllocHGlobal(convertedBitImg.Length);
                            Marshal.Copy(convertedBitImg, 0, rowsPtrs, convertedBitImg.Length);
                            int[] array = new int[convertedBitImg.Length];
                            unsafe { fixed (int* intpoint = &array[0]) 
                                intPoint = intpoint;
                            }
                            double bytesNum = ((convertedBitImg.Length) / numberOfTasks / 12.0);
                            int bytes = (int)(Math.Floor(bytesNum) * 12);
                            int bytesMultTask = bytes * numberOfTasks;
                            int bytesPerLastTask = bytes + convertedBitImg.Length - bytesMultTask;
                            int[] bytesPerTask = new int[numberOfTasks];
                            Task[] tasks = new Task[numberOfTasks];
                            for (int i = 0; i < numberOfTasks; i++)
                            {
                                if(i == numberOfTasks - 1)
                                {
                                    bytesPerTask[i] = bytesPerLastTask;
                                }
                                else
                                {
                                    bytesPerTask[i] = bytes;
                                }
                            }
                            if(CppCheckbox.IsChecked == true)
                            {
                                Stopwatch stopwatch = new Stopwatch();
                                stopwatch.Start();
                                Trace.WriteLine(bytesPerTask[0]);
                                for(int i = 0; i < numberOfTasks; i++)
                                {
                                    int j = i;
                                    IntPtr pixelPointer = rowsPtrs + (j * bytes);
                                    int l = bytesPerTask[j];
                                    if (i == 0)
                                    {
                                        l -= offset;
                                        pixelPointer += offset;
                                    }
                                    tasks[i] = new Task(() => cppASCIIArt(l, pixelPointer));
                                    tasks[i].Start();
                                }
                                Task.WaitAll(tasks);
                                stopwatch.Stop();
                                Trace.WriteLine(stopwatch.ElapsedMilliseconds);
                                TotalTime.Content = "Łączny czas wykonania się programu: " + stopwatch.ElapsedMilliseconds + "ms";
                                outputTxtFile = pathImg + "ASCII_ART" + ".txt";
                                writeFileAsync();
                            }
                            else if (AsmCheckbox.IsChecked == true)
                            {
                                Stopwatch stopwatch = new Stopwatch();
                                stopwatch.Start();
                                for (int i = 0; i < numberOfTasks; i++)
                                {
                                    int j = i;
                                    IntPtr pixelPointer = rowsPtrs + (j * bytes);
                                    //int* pointerToAsm = intPoint + (j * bytes);
                                    if (i == 0) {
                                        bytesPerTask[j] -= offset - 4;
                                        pixelPointer += offset;
                                    }
                                    tasks[i] = new Task(() => ASCIIArt(bytesPerTask[j], (byte*)pixelPointer.ToPointer()));
                                    tasks[i].Start();
                                }
                                Task.WaitAll(tasks);
                                stopwatch.Stop();
                                TotalTime.Content = "Łączny czas wykonania się programu: " + stopwatch.ElapsedMilliseconds + "ms";
                                outputTxtFile = pathImg + "ASCII_ART" + ".txt";
                                writeFileAsync();
                            }
                        }
                    }
                }
            }
            catch (System.NullReferenceException)
            {
                ErrorLabel.Content = "Choose Image!";
            }
        }

        unsafe private void writeFileAsync()
        {
            Marshal.Copy(rowsPtrs, convertedBitImg, 0, convertedBitImg.Length);
            var sb = new StringBuilder();
            int sum = ((bitmap.PixelHeight * bitmap.PixelWidth) * 3) + offset;
            int c = 0;
            for (int i = sum; i >= convertedBitImg[10]; i-=3)
            {
                if (c % (bitmap.PixelWidth) == 0)
                {
                    sb.Append("\n");
                    c = 0;
                }
                else
                {
                    sb.Append((char)convertedBitImg[i]).Append(" ");
                }
                c++;
            }
            File.WriteAllText(outputTxtFile, sb.ToString());
            if (CppCheckbox.IsChecked == true)
                Marshal.FreeHGlobal(rowsPtrs);
            else if (AsmCheckbox.IsChecked == true)
                intPoint = null;

        }

        private int getNumberOfThreads()
        {
            try
            {
                return int.Parse(NumberOfThreads.Text);
            }
            catch (Exception)
            {
                return -1;
            }
        } 

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "C:\\Users\\Blazej\\source\\repos\\JA_BMP_TO_ASCII\\JA_BMP_TO_ASCII";
            dlg.Filter = "Image files (*.bmp)|*.bmp|All Files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    pathImg = dlg.FileName;
                    string selectedFileName = dlg.FileName;
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedFileName);
                    bitmap.EndInit();
                    inputPath = selectedFileName;
                    ImageControl.Source = bitmap;
                    Trace.WriteLine(bitmap.PixelWidth);
                }
                catch(Exception)
                {
                    ErrorLabel.Content = "Choose an image, please.";
                }
                
            }
        }
    }
}
