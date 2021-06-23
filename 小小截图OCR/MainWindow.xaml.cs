using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Diagnostics;
using System.Windows.Interop;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;

namespace 小小截图OCR
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public ICommand HideCommand { get; } = new Command<Window>(win => win.Visibility = Visibility.Hidden);

        private IReadOnlyList<Rect> windowRects;

        private Base64 base64 = new Base64();
        private OcrTools ocrTools = new OcrTools();

        public MainWindow()
        {
            InitializeComponent();

            pixelObserver.DataContext = editor;
            tipSize.DataContext = editor;
            tipRGB.DataContext = pixelObserver;

            startup();
        }

        private void Grid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(editor);
            pixelObserver.X = point.X;
            pixelObserver.Y = point.Y;
        }

        private void FocusWindow()
        {
            var curr = Mouse.GetPosition(this);
            foreach (var r in windowRects)
            {
                if (r.Contains(curr))
                {
                    editor.SelectX = r.X;
                    editor.SelectY = r.Y;
                    editor.SelectWidth = r.Width;
                    editor.SelectHeight = r.Height;
                    break;
                }
            }
        }

        private void editor_MouseMove(object sender, MouseEventArgs e)
        {
            if (editor.SelectState == SelectState.Fixed)
            {
                FocusWindow();
            }
        }

        private void editor_Click(object sender, RoutedEventArgs e)
        {
            Debug.Print("click");
            if (editor.SelectState == SelectState.Fixed) editor.Select();
        }

        private void editor_AcceptSelect(object sender, AcceptSelectEventArgs e)
        {
            var curr = Mouse.GetPosition(this);
            if (curr.X < editor.SelectX + editor.SelectWidth / 2)
            {
                if(curr.Y < editor.SelectY + editor.SelectHeight / 2)
                {
                    Console.WriteLine("复制截图");
                    Clipboard.SetImage(e.SelectBitmap);
                }
                else
                {
                    Console.WriteLine("保存到桌面");
                    Bitmap tempImgBitmap = GetBitmap(e.SelectBitmap);

                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                    var picFiles = Directory.GetFiles(desktopPath, "小小截图*.jpg");
                    int maxNum = 0;
                    foreach (var fileName in picFiles)
                    {
                        Console.WriteLine(fileName);

                        string pattern = @"小小截图(?<num>\d+?).jpg";
                        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                        Match match = regex.Match(fileName);
                        if (match.Success)
                        {
                            int picNum = int.Parse(match.Groups["num"].Value);
                            Console.WriteLine(picNum);

                            if(picNum > maxNum)
                            {
                                maxNum = picNum;
                            }
                        }
                    }

                    tempImgBitmap.Save(desktopPath + String.Format("\\小小截图{0}.jpg", maxNum+1));
                }
            }
            else
            {
                if (curr.Y < editor.SelectY + editor.SelectHeight / 2)
                {
                    Console.WriteLine("通用识别");
                    Bitmap tempImgBitmap = GetBitmap(e.SelectBitmap);
                    string tempImgBase64 = base64.ImgToBase64String(tempImgBitmap);
                    ocrTools.GeneralBasicOCR(tempImgBase64);
                }
                else
                {
                    Console.WriteLine("手写体识别");
                    Bitmap tempImgBitmap = GetBitmap(e.SelectBitmap);
                    string tempImgBase64 = base64.ImgToBase64String(tempImgBitmap);
                    ocrTools.GeneralHandwritingOCR(tempImgBase64);
                }
            }
            Hide();
        }

        Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(
              source.PixelWidth,
              source.PixelHeight,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
              new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            const int WS_EX_TOOLWINDOW = 0x00000080;
            var helper = new WindowInteropHelper(this);
            var exStyle = Tool.GetWindowExStyle(helper.Handle);
            Tool.SetWindowExStyle(helper.Handle, exStyle | WS_EX_TOOLWINDOW);

            HotKey.Register(this, ModifierKeys.Alt, Keys.A, delegate
            {
                if (Visibility != Visibility.Visible)
                {
                    editor.BackgroundBitmap = Tool.ScreenSnapshot;
                    editor.Reset();
                    windowRects = Tool.GetWindowRects();
                    Show();
                }
            });
            Hide();
        }

        private void editor_Close(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void editor_CancelSelect(object sender, RoutedEventArgs e)
        {
            editor.Reset();
            FocusWindow();
        }

        /// <summary>
        /// 将文件放到启动文件夹中开机启动
        /// </summary>
        /// <param name="setupPath">启动程序</param>
        /// <param name="linkname">快捷方式名称</param>
        /// <param name="description">描述</param>
        private bool startup()
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string targetPath = Process.GetCurrentProcess().MainModule.FileName;
            Console.WriteLine(directory);
            Console.WriteLine(targetPath);
            string shortcutName = "小小截图";

            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                //添加引用 Com 中搜索 Windows Script Host Object Model
                string shortcutPath = System.IO.Path.Combine(directory, string.Format("{0}.lnk", shortcutName));

                if (File.Exists(shortcutPath))
                {
                    return true;
                }

                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);//创建快捷方式对象
                shortcut.TargetPath = targetPath;//指定目标路径
                shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(targetPath);//设置起始位置
                //shortcut.WindowStyle = 1;//设置运行方式，默认为常规窗口
                //shortcut.Description = description;//设置备注
                shortcut.IconLocation = targetPath; //设置图标路径
                shortcut.Save();//保存快捷方式

                return true;
            }
            catch
            { }
            return false;
        }
    }
}
