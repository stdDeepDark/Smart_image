using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;



namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    internal enum AccentState
    {
        ACCENT_DISABLED = 1,
        ACCENT_ENABLE_GRADIENT = 0,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

[StructLayout(LayoutKind.Sequential)]
internal struct AccentPolicy
{
    public AccentState AccentState;
    public int AccentFlags;
    public int GradientColor;
    public int AnimationId;
}

[StructLayout(LayoutKind.Sequential)]
internal struct WindowCompositionAttributeData
{
    public WindowCompositionAttribute Attribute;
    public IntPtr Data;
    public int SizeOfData;
}

internal enum WindowCompositionAttribute
{
    // ...
    WCA_ACCENT_POLICY = 19
    // ...
}

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>

    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
        System.Windows.Forms.Timer newLabelTimer = new System.Windows.Forms.Timer();
        BitmapImage img;
        FileStream imgFS;
        InfoLabelPro infoLabelPro;
        static public string filename;
        public static MainWindow themainWindow;
        bool hasLabel = false;
        int endOfJpg,start,end;
        byte[] srcbyte;
        bool imgready = false;
        static public bool editMode;
        //string imginfo;
        bool isNewLabel = false;
        bool istoolBar = false;
        List<InfoLabelPro> infoLabelPros;
        public MainWindow()
        {
            InitializeComponent();
            themainWindow = this;
            editMode = false;
            button_newlabel.Visibility = Visibility.Hidden;
            button_save.Visibility = Visibility.Hidden;
            button_delete.Visibility = Visibility.Hidden;
            label_fontColore.Visibility = Visibility.Hidden;
            FontColor_ComboBox.Visibility = Visibility.Hidden;
            label_fontFamily.Visibility = Visibility.Hidden;
            fontFamily_ComboBox.Visibility = Visibility.Hidden;
            label_fontSize.Visibility = Visibility.Hidden;
            fontSize_textBox.Visibility = Visibility.Hidden;
            label_borderColor.Visibility = Visibility.Hidden;
            BorderColor_ComboBox.Visibility = Visibility.Hidden;
            check_backTrans.Visibility = Visibility.Hidden;
            mode_button.Content = "编辑模式";
            double ScreenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;//WPF
            double ScreenHeight = SystemParameters.PrimaryScreenHeight;
            if (this.Width >= ScreenWidth)
                this.Width = ScreenWidth-50;
            if (this.Height >= ScreenHeight)
                this.Height = ScreenHeight-100;

            Grid1.MouseLeftButtonUp += new MouseButtonEventHandler(Grid1_MouseLeftButtonUp);
            Grid1.MouseLeftButtonDown += new MouseButtonEventHandler(Grid1_MouseLeftButtonDown);
            toolBar1.MouseDown += new MouseButtonEventHandler(toolBar_MouseLeftButtonDown);
            toolBar1.MouseLeave += new System.Windows.Input.MouseEventHandler(toolBar_MouseLeave);
            fontFamily_ComboBox.SelectionChanged += new SelectionChangedEventHandler(fontFamily_SelectionChanged);
            foreach (FontFamily fontfamily in Fonts.SystemFontFamilies)
            {
                fontFamily_ComboBox.Items.Add(fontfamily);
            }
            fontSize_textBox.TextChanged += new TextChangedEventHandler(fontSize_TextChanged);
            Type type = typeof(System.Windows.Media.Brushes);
            System.Reflection.PropertyInfo[] info = type.GetProperties();
           // foreach (System.Reflection.PropertyInfo pi in info)
            //{
               // FontColor_ComboBox.Items.Add(pi.Name);
             //   BorderColor_ComboBox.Items.Add(pi.Name);
           // }
            FontColor_ComboBox.SelectionChanged += new SelectionChangedEventHandler(fontColor_SelectionChanged);
            BorderColor_ComboBox.SelectionChanged += new SelectionChangedEventHandler(borderColor_SelectionChanged);
            ColorDialog ColorForm = new ColorDialog();
           // if (ColorForm.ShowDialog() == DialogResult.OK)
            {
                //Color GetColor = ColorForm.Color;
                //GetColor就是用户选择的颜色，接下来就可以使用该颜色了
                // button2.BackColor = GetColor;
            }
            if (App.file != null)
            {
                filename = App.file;
                imgFS = new FileStream(filename, FileMode.Open);

                img = new BitmapImage();
                img.BeginInit();
                img.StreamSource = imgFS;
                img.EndInit();


                if (img.Width <= main_image.Width && img.Height <= main_image.Height)
                {
                    main_image.Stretch = Stretch.None;
                }
                else
                {
                    main_image.Stretch = Stretch.Uniform;
                }
                main_image.Source = img;
               
                title.Content = "Smart Image - " + filename.Substring(filename.LastIndexOf('\\')+1);
                Thread thread = new Thread(getImageInfo);
                thread.Start();
            }
        }

        static byte[] ArrayCRCHigh =
          {
          0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
          0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
          0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
          0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
          0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
          0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
          0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
          0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
          0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
          0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
          0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
          0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
          0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
          0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
          0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
          0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
          0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
          0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
          0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
          0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
          0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
          0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
          0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
          0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
          0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
          0x80, 0x41, 0x00, 0xC1, 0x81, 0x40
         };
          /// <summary>
         /// CRC地位校验码checkCRCLow
         /// </summary>
         static byte[] checkCRCLow =
         {
         0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06,
         0x07, 0xC7, 0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD,
         0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09,
         0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A,
         0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC, 0x14, 0xD4,
         0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
         0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3,
         0xF2, 0x32, 0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4,
         0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A,
         0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29,
         0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF, 0x2D, 0xED,
         0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
         0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60,
         0x61, 0xA1, 0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67,
         0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F,
         0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68,
         0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA, 0xBE, 0x7E,
         0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
         0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71,
         0x70, 0xB0, 0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92,
         0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C,
         0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B,
         0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89, 0x4B, 0x8B,
         0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
         0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42,
         0x43, 0x83, 0x41, 0x81, 0x80, 0x40
         };         
        /// <summary>
          /// CRC校验
         /// </summary>
         /// <param name="data">校验的字节数组</param>
         /// <param name="length">校验的数组长度</param>
         /// <returns>该字节数组的奇偶校验字节</returns>
         public static Int16 CRC16(byte[] data, int arrayLength)
         {
             byte CRCHigh = 0xFF;
             byte CRCLow = 0xFF;
             byte index;
             int i = 0;
             while (arrayLength-- > 0)
             {
                 index = (System.Byte) (CRCHigh ^ data[i++]);
                 CRCHigh = (System.Byte) (CRCLow ^ ArrayCRCHigh[index]);
                 CRCLow = checkCRCLow[index];
             }
             return (Int16) (CRCHigh << 8 | CRCLow);
         }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();
        }

        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            DragMove();
        }



        void borderColor_SelectionChanged(object sender, System.EventArgs e)
        {
           
            if (infoLabelPros != null)
                foreach (InfoLabelPro i in infoLabelPros)
                    if (i.infoLabel.isSelected|| i.detectLabel.isSelected)
                    {
                        i.infoLabel.borderColor = BorderColor_ComboBox.SelectedValue.ToString().Substring(BorderColor_ComboBox.SelectedValue.ToString().IndexOf(' ') + 1);
                        Color color = (Color)ColorConverter.ConvertFromString(i.infoLabel.borderColor);
                        i.infoLabel.border.BorderBrush = new SolidColorBrush(color);
                        if(!i.backTrans)
                            i.infoLabel.label.Background = i.infoLabel.border.BorderBrush;
                        i.detectLabel.borderColor = BorderColor_ComboBox.SelectedValue.ToString().Substring(BorderColor_ComboBox.SelectedValue.ToString().IndexOf(' ') + 1);
                        i.detectLabel.border.BorderBrush = new SolidColorBrush(color);
                    }
        }

        void fontColor_SelectionChanged(object sender, System.EventArgs e)
        {

            if (infoLabelPros != null)
                foreach (InfoLabelPro i in infoLabelPros)
                    if (i.infoLabel.isSelected|| i.detectLabel.isSelected)
                    {
                        i.infoLabel.fontColor = FontColor_ComboBox.SelectedValue.ToString().Substring(FontColor_ComboBox.SelectedValue.ToString().IndexOf(' ')+1);
                        Color color = (Color)ColorConverter.ConvertFromString(i.infoLabel.fontColor);
                        i.infoLabel.label.Foreground = new SolidColorBrush(color);
                    }
        }
        void fontSize_TextChanged(object sender, System.EventArgs e)
        {
            double size = 12;
            try
            {
                size = Convert.ToInt32(fontSize_textBox.Text);
                if (size <= 0)
                {
                    size = 1;
                    fontSize_textBox.Text = "1";
                }
                else if (size > 10000)
                {
                    size = 10000;
                    fontSize_textBox.Text = "10000";
                }
            }
            catch(Exception )
            {
                fontSize_textBox.Text = "";
                return;
            }
            if (infoLabelPros != null)
                foreach (InfoLabelPro i in infoLabelPros)
                    if (i.infoLabel.isSelected||i.detectLabel.isSelected)
                        i.infoLabel.label.FontSize = size;
        }
        void fontFamily_SelectionChanged(object sender, System.EventArgs e)
        {
           if(infoLabelPros != null)
            foreach (InfoLabelPro i in infoLabelPros)
                if (i.infoLabel.isSelected|| i.detectLabel.isSelected)
                    i.infoLabel.label.FontFamily = new FontFamily(fontFamily_ComboBox.SelectedValue.ToString());
        }
        void toolBar_MouseLeave(object sender, System.EventArgs e)
        {
            istoolBar = false;
        }
        void toolBar_MouseLeftButtonDown(object sender, System.EventArgs e)
        {
            istoolBar = true;
        }
        void Grid1_MouseLeftButtonDown(object sender, System.EventArgs e)
        {
            bool flag1 = true;
            if (infoLabelPros != null)
                foreach (InfoLabelPro i in infoLabelPros)
                {
                    if (i.infoLabel.typingleave)
                        i.infoLabel.textBox.Visibility = Visibility.Hidden;
                    if (i.detectLabel.typingleave)
                        i.detectLabel.textBox.Visibility = Visibility.Hidden;
                    if (!istoolBar && i.infoLabel.isSelected && !i.infoLabel.mouseEnter)
                    { 
                        i.infoLabel.isSelected = false;
                    }
                    if (!istoolBar && i.detectLabel.isSelected && !i.detectLabel.mouseEnter)
                    {
                        i.detectLabel.isSelected = false;
                    }
                }
            if (infoLabelPros != null)
                foreach (InfoLabelPro i in infoLabelPros)
                {

                     if (i.infoLabel.isSelected == true||i.detectLabel.isSelected == true)
                    {
                        flag1 = false;
                        fontFamily_ComboBox.Text = i.infoLabel.label.FontFamily.ToString();
                        fontSize_textBox.Text = i.infoLabel.label.FontSize.ToString();
                        check_backTrans.IsChecked = i.backTrans;
                        string fontcolor = "System.Windows.Media.Color" + " " + i.infoLabel.fontColor;
                        for(int u = 0; u < FontColor_ComboBox.Items.Count; u++)
                        {
                            if(FontColor_ComboBox.Items[u].ToString() == fontcolor)
                            {
                                FontColor_ComboBox.SelectedIndex = u;
                                break;
                            }
                        }
                        string bordercolor = "System.Windows.Media.Color" + " " + i.infoLabel.borderColor;
                        for (int u = 0; u < BorderColor_ComboBox.Items.Count; u++)
                        {
                            if (BorderColor_ComboBox.Items[u].ToString() == bordercolor)
                            {
                                BorderColor_ComboBox.SelectedIndex = u;
                                break;
                            }
                        }
                    }

                }
            if (flag1)
            {
                fontFamily_ComboBox.Text = "";
                fontSize_textBox.Text = "";
                FontColor_ComboBox.Text = "";
                BorderColor_ComboBox.Text = "";
                check_backTrans.IsChecked = false;
            }
            if (isNewLabel)
            {
                Point point = Mouse.GetPosition(Grid1);
                infoLabelPro = new InfoLabelPro(new InfoLabel((int)point.X,(int)point.Y-150, 100, 100, "双击输入文本", 15, "White", new FontFamily("Microsoft YaHei UI"), "SkyBlue"), new InfoLabel((int)point.X, (int)point.Y, 10, 10, "", "SkyBlue"),false);
                Grid1.Children.Add(infoLabelPro.infoLabel.label);
                Grid1.Children.Add(infoLabelPro.infoLabel.border);
                Grid1.Children.Add(infoLabelPro.infoLabel.textBox);
                Grid1.Children.Add(infoLabelPro.detectLabel.label);
                Grid1.Children.Add(infoLabelPro.detectLabel.border);
                Grid1.Children.Add(infoLabelPro.detectLabel.textBox);
                infoLabelPros.Add(infoLabelPro);
                infoLabelPro.detectLabel.isdisappear = false;
                infoLabelPro.infoLabel.isdisappear = false;
                newLabelTimer = new System.Windows.Forms.Timer();
                newLabelTimer.Interval = 20;
                newLabelTimer.Tick += delegate
                {
                    point = Mouse.GetPosition(infoLabelPro.detectLabel.border);
                    infoLabelPro.detectLabel.adjustSize((int)point.X,(int)point.Y);
                };
                newLabelTimer.Start();
            }
        }
        void Grid1_MouseLeftButtonUp(object sender, System.EventArgs e)
        {
            if(isNewLabel)
            {

                infoLabelPro.infoLabel.isdisappear = true;
                infoLabelPro.detectLabel.isdisappear = true;
                newLabelTimer.Stop();
                isNewLabel = false;
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
            if (infoLabelPros == null)
                return;
            foreach( InfoLabelPro i in infoLabelPros)
            {
                if(i.infoLabel.adjustTimer.Enabled == true)
                    i.infoLabel.adjust_MouseLeftButtonUp(sender,e);
                if (i.infoLabel.moveTimer.Enabled == true)
                    i.infoLabel.stopmove_MouseLeftButtonUp(sender, e);


                if (i.detectLabel.adjustTimer.Enabled == true)
                    i.detectLabel.adjust_MouseLeftButtonUp(sender, e);
                if (i.detectLabel.moveTimer.Enabled == true)
                    i.detectLabel.stopmove_MouseLeftButtonUp(sender, e);
            }
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "请选择输入图片";
            dialog.Filter = "图片文件(*.jpg,*.jpeg)|*.jpg;*.jpeg";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                
                filename = dialog.FileName;
                imgFS = new FileStream(filename, FileMode.Open);
                
                img = new BitmapImage();
                img.BeginInit();
                img.StreamSource = imgFS;
                img.EndInit();
                
                if (img.Width <= main_image.Width && img.Height <= main_image.Height)
                {
                    main_image.Stretch = Stretch.None;
                }
                else
                {
                    main_image.Stretch = Stretch.Uniform;
                }
                main_image.Source = img;

                title.Content = "Smart Image - " + filename.Substring(filename.LastIndexOf('\\')+1);
                Thread thread = new Thread(getImageInfo);
                thread.Start();
            }
        }
        void getImageInfo()
        {
            if(infoLabelPros != null)
            {
                Grid1.Dispatcher.Invoke(new MethodInvoker(delegate
                {
                    foreach (InfoLabelPro infolabel in infoLabelPros)
                    {
                        Grid1.Children.Remove(infolabel.infoLabel.label);
                        Grid1.Children.Remove(infolabel.infoLabel.border);
                        Grid1.Children.Remove(infolabel.infoLabel.textBox);

                        Grid1.Children.Remove(infolabel.detectLabel.label);
                        Grid1.Children.Remove(infolabel.detectLabel.border);
                        Grid1.Children.Remove(infolabel.detectLabel.textBox);
                    }
                }));
            }
            this.Dispatcher.Invoke(new MethodInvoker(delegate
            {
                this.Cursor = System.Windows.Input.Cursors.Wait;
            }));
            srcbyte = new byte[imgFS.Length];
            Thread.Sleep(1000);
            imgFS.Seek(0, SeekOrigin.Begin);
            imgFS.Read(srcbyte, 0, srcbyte.Length);
            imgFS.Close();
            bool flag1 = false;
            endOfJpg = 0;
            start = 0;
            end = 0;
            hasLabel = false;
            for(int i = 0; i < srcbyte.Length; i++)
            {
                if (endOfJpg == 0)
                {
                    if (srcbyte[i] == (byte)0xFF && i + 1 < srcbyte.Length)
                    {
                        if (srcbyte[i + 1] == (byte)0xD9)
                        {
                            endOfJpg = i + 1;
                        }
                    }
                }
                else
                {
                    if (flag1)
                    {
                        if (srcbyte[i] == (byte)0xff && i + 1 < srcbyte.Length)
                        {
                            if (srcbyte[i + 1] == (byte)0xed)
                            {
                                end = i + 1;
                                hasLabel = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (srcbyte[i] == (byte)0xff && i + 1 < srcbyte.Length)
                        {
                            if (srcbyte[i + 1] == (byte)0xaa)
                            {
                                start = i;
                                flag1 = true;
                            }
                        }
                    }
                }
            }
            infoLabelPros = new List<InfoLabelPro>();
            if (hasLabel)
            {
         
                byte[] imgbyte = new byte[end - start - 3];
                Buffer.BlockCopy(srcbyte, start+2, imgbyte, 0, end - start - 3);
                Int16 crc = CRC16(imgbyte, end - start - 3);
                if(crc != 0)
                {
                    error(0);
                }
                int dx,dy,dwidth,dheight,dtextlen,x, y, width, height, fontsize,textlen, fontcolorlen ,fontfamilylen, bordercolorlen;
                string dtext,text, fontcolor, fontfamily, bordercolor;
                try
                {
                    Console.Write(imgbyte.ToString());
                    for (int i = 0; i < imgbyte.Length-2; i++)
                    {
                        
                        x = imgbyte[i] * 256 + imgbyte[i + 1];
                        y = imgbyte[i + 2] * 256 + imgbyte[i + 3];
                        width = imgbyte[i + 4] * 256 + imgbyte[i + 5];
                        height = imgbyte[i + 6] * 256 + imgbyte[i + 7];
                        textlen = imgbyte[i + 8] * 256 + imgbyte[i + 9];
                        fontsize = imgbyte[i + 10] * 256 + imgbyte[i + 11];
                        fontcolorlen = imgbyte[i + 12];
                        fontfamilylen = imgbyte[i + 13];
                        bordercolorlen = imgbyte[i + 14];
                        text = Encoding.UTF8.GetString(imgbyte, i + 15, textlen);
                        fontcolor = Encoding.UTF8.GetString(imgbyte, i + 15 + textlen, fontcolorlen);
                        fontfamily = Encoding.UTF8.GetString(imgbyte, i + 15 + textlen + fontcolorlen, fontfamilylen);
                        bordercolor = Encoding.UTF8.GetString(imgbyte, i + 15 + textlen + fontcolorlen + fontfamilylen, bordercolorlen);
                        Console.WriteLine(text);
                        Console.WriteLine(fontcolor);
                        Console.WriteLine(fontfamily);
                        Console.WriteLine(bordercolor);
                        i += 15 + textlen + fontcolorlen + fontfamilylen + bordercolorlen;
                        bool istrans = Convert.ToBoolean(imgbyte[i++]);
                        
                        dx = imgbyte[i] * 256 + imgbyte[i + 1];

                        dy = imgbyte[i + 2] * 256 + imgbyte[i + 3];
                        dwidth = imgbyte[i + 4] * 256 + imgbyte[i + 5];
                        dheight = imgbyte[i + 6] * 256 + imgbyte[i + 7];
                        dtextlen = imgbyte[i + 8] * 256 + imgbyte[i + 9];
                        
                        Console.WriteLine(dx);
                        Console.WriteLine(dtextlen);
                        dtext = Encoding.UTF8.GetString(imgbyte, i + 10, dtextlen);
                        Console.WriteLine(123);
                        Console.WriteLine("wrong3");
                        //BitConverter.ToString(imgbyte, i+10, len);
                        this.Dispatcher.Invoke(new MethodInvoker(delegate
                        {
                            infoLabelPros.Add(new InfoLabelPro(new InfoLabel(x, y, width, height, text, fontsize, fontcolor, new FontFamily(fontfamily), bordercolor),new InfoLabel(dx,dy,dwidth,dheight,dtext,bordercolor),istrans));
                        }));
                        i += 9 + dtextlen;
                    }
                    Grid1.Dispatcher.Invoke(new MethodInvoker(delegate
                    {
                        foreach (InfoLabelPro infolabel in infoLabelPros)
                        {
                            Grid1.Children.Add(infolabel.infoLabel.label);
                            Grid1.Children.Add(infolabel.infoLabel.border);
                            Grid1.Children.Add(infolabel.infoLabel.textBox);
                            Grid1.Children.Add(infolabel.detectLabel.label);
                            Grid1.Children.Add(infolabel.detectLabel.border);
                            Grid1.Children.Add(infolabel.detectLabel.textBox);
                            infolabel.infoLabel.disAppear();
                            infolabel.detectLabel.disAppear();
                        }
                    }));
                    
                }
                catch
                {
                    Console.WriteLine("wrong");
                    error(0);
                }
            }
            imgready = true;
            this.Dispatcher.Invoke(new MethodInvoker(delegate
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }));
            
        }
        public static void error(int ID)
        {

            MessageBoxResult result;
            switch (ID)
            {
                case 0:
                    result = System.Windows.MessageBox.Show("图像文本框信息解析错误", "错误提示");
                    break;
                case 1:
                    result = System.Windows.MessageBox.Show("请先选择一个图片", "错误提示");
                    break;
                case 2:
                    result = System.Windows.MessageBox.Show("保存失败,暂不支持非JPG文件", "错误提示");
                    break;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (imgready)
            {
                this.Cursor = System.Windows.Input.Cursors.Cross;
                isNewLabel = true;
            }else
            {
                error(1); 
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (infoLabelPros != null)
                for(int i = infoLabelPros.Count-1;  i >= 0; i--)
                    if (infoLabelPros[i].infoLabel.isSelected|| infoLabelPros[i].detectLabel.isSelected)
                    {
                        Grid1.Children.Remove(infoLabelPros[i].infoLabel.label);
                        Grid1.Children.Remove(infoLabelPros[i].infoLabel.border);
                        Grid1.Children.Remove(infoLabelPros[i].infoLabel.textBox);
                        Grid1.Children.Remove(infoLabelPros[i].detectLabel.label);
                        Grid1.Children.Remove(infoLabelPros[i].detectLabel.border);
                        Grid1.Children.Remove(infoLabelPros[i].detectLabel.textBox);
                        infoLabelPros.Remove(infoLabelPros[i]);
                    }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void mode_button_Click(object sender, RoutedEventArgs e)
        {
            if (editMode)
            {
                editMode = false;
                mode_button.Content = "编辑模式";

                button_newlabel.Visibility = Visibility.Hidden;
                button_save.Visibility = Visibility.Hidden;
                button_delete.Visibility = Visibility.Hidden;
                label_fontColore.Visibility = Visibility.Hidden;
                FontColor_ComboBox.Visibility = Visibility.Hidden;
                label_fontFamily.Visibility = Visibility.Hidden;
                fontFamily_ComboBox.Visibility = Visibility.Hidden;
                label_fontSize.Visibility = Visibility.Hidden;
                fontSize_textBox.Visibility = Visibility.Hidden;
                label_borderColor.Visibility = Visibility.Hidden;
                BorderColor_ComboBox.Visibility = Visibility.Hidden;
                check_backTrans.Visibility = Visibility.Hidden;
            }
            else
            {
                editMode = true;
                mode_button.Content = "查看模式";
                button_newlabel.Visibility = Visibility.Visible;
                button_save.Visibility = Visibility.Visible;
                button_delete.Visibility = Visibility.Visible;
                label_fontColore.Visibility = Visibility.Visible;
                FontColor_ComboBox.Visibility = Visibility.Visible;
                label_fontFamily.Visibility = Visibility.Visible;
                fontFamily_ComboBox.Visibility = Visibility.Visible;
                label_fontSize.Visibility = Visibility.Visible;
                fontSize_textBox.Visibility = Visibility.Visible;
                label_borderColor.Visibility = Visibility.Visible;
                BorderColor_ComboBox.Visibility = Visibility.Visible;
                check_backTrans.Visibility = Visibility.Visible;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (infoLabelPros != null)
                foreach (InfoLabelPro i in infoLabelPros)
                    if (i.infoLabel.isSelected || i.detectLabel.isSelected)
                    {
                        Console.WriteLine(check_backTrans.IsChecked);
                        i.trans_alter((bool)check_backTrans.IsChecked);
                    }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            
            if (imgready)
            {
                if (endOfJpg != 0)
                {
                    this.Cursor = System.Windows.Input.Cursors.Wait;
                    if (hasLabel)
                    {
                        List<byte> bytes = new List<byte>(), bytes1 = new List<byte>();
                        byte[] midbytes = new byte[start];
                        byte[] midbytes2, midbytes3 , midbytes4;
                        Buffer.BlockCopy(srcbyte, 0, midbytes, 0, start);
                        bytes1.AddRange(midbytes);
                        bytes1.Add(0xff);
                        bytes1.Add(0xaa);
                        foreach (InfoLabelPro ilabel in infoLabelPros)
                        {
                            bytes.Add(BitConverter.GetBytes(ilabel.infoLabel.x)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.infoLabel.x)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.infoLabel.y)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.infoLabel.y)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.infoLabel.width)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.infoLabel.width)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.infoLabel.height)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.infoLabel.height)[0]);
                            midbytes = System.Text.Encoding.UTF8.GetBytes(ilabel.infoLabel.text);
                            bytes.Add(BitConverter.GetBytes(midbytes.Length)[1]);
                            bytes.Add(BitConverter.GetBytes(midbytes.Length)[0]);
                            bytes.Add(BitConverter.GetBytes((int)ilabel.infoLabel.label.FontSize)[1]);
                            bytes.Add(BitConverter.GetBytes((int)ilabel.infoLabel.label.FontSize)[0]);
                     
                            midbytes2 = System.Text.Encoding.UTF8.GetBytes(ilabel.infoLabel.fontColor);
                            bytes.Add(BitConverter.GetBytes(midbytes2.Length)[0]);
                            Console.WriteLine(BitConverter.GetBytes(midbytes2.Length)[0] + "\n" + ilabel.infoLabel.fontColor);
                            midbytes3 = System.Text.Encoding.UTF8.GetBytes(ilabel.infoLabel.label.FontFamily.ToString());
                            bytes.Add(BitConverter.GetBytes(midbytes3.Length)[0]);
                            Console.WriteLine(BitConverter.GetBytes(midbytes3.Length)[0] + "\n" + ilabel.infoLabel.label.FontFamily.ToString());
                            midbytes4 = System.Text.Encoding.UTF8.GetBytes(ilabel.infoLabel.borderColor);
                            bytes.Add(BitConverter.GetBytes(midbytes4.Length)[0]);
                            Console.WriteLine(BitConverter.GetBytes(midbytes4.Length)[0] + "\n" + ilabel.infoLabel.borderColor);

                            for (int i = 0; i < midbytes.Length; i++)
                                bytes.Add(midbytes[i]);

                            for (int i = 0; i < midbytes2.Length; i++)
                                bytes.Add(midbytes2[i]);

                            for (int i = 0; i < midbytes3.Length; i++)
                                bytes.Add(midbytes3[i]);
                            for (int i = 0; i < midbytes4.Length; i++)
                                bytes.Add(midbytes4[i]);
                            bytes.Add(BitConverter.GetBytes(ilabel.backTrans)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.detectLabel.x)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.detectLabel.x)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.detectLabel.y)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.detectLabel.y)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.detectLabel.width)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.detectLabel.width)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.detectLabel.height)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.detectLabel.height)[0]);
                            midbytes = System.Text.Encoding.UTF8.GetBytes(ilabel.detectLabel.text);
                            bytes.Add(BitConverter.GetBytes(midbytes.Length)[1]);
                            bytes.Add(BitConverter.GetBytes(midbytes.Length)[0]);
                            for (int i = 0; i < midbytes.Length; i++)
                                bytes.Add(midbytes[i]);
                            Console.WriteLine(ilabel.detectLabel.x);
                        }
                        byte[] crcbytes = new byte[bytes.Count];
                        bytes.CopyTo(crcbytes);
                        Int16 crc= CRC16(crcbytes, bytes.Count);
                        
                        bytes.Add(BitConverter.GetBytes(crc)[1]);
                        bytes.Add(BitConverter.GetBytes(crc)[0]);
                        
                        bytes1.AddRange(bytes);
                        bytes1.Add(0xff);
                        bytes1.Add(0xed);
                        
                        midbytes = new byte[srcbyte.Length - end - 1];
                        Buffer.BlockCopy(srcbyte, end + 1, midbytes, 0, midbytes.Length);
                        bytes1.AddRange(midbytes);
                        imgFS = new FileStream(filename, FileMode.Open);
                        imgFS.Seek(0, SeekOrigin.Begin);
                        imgFS.Write(bytes1.ToArray(), 0, bytes1.Count);
                        imgFS.Flush();
                        imgFS.Close();
                    }
                    else
                    {

                        List<byte> bytes = new List<byte>(), bytes1 = new List<byte>();
                        byte[] midbytes2, midbytes3, midbytes4;
                        bytes1.Add(0xff);
                        bytes1.Add(0xaa);

                        foreach (InfoLabelPro ilabel in infoLabelPros)
                        {
                            bytes.Add(BitConverter.GetBytes(ilabel.infoLabel.x)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.infoLabel.x)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.infoLabel.y)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.infoLabel.y)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.infoLabel.width)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.infoLabel.width)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.infoLabel.height)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.infoLabel.height)[0]);
                            byte[] midbytes;
                            midbytes = System.Text.Encoding.UTF8.GetBytes(ilabel.infoLabel.text);
                            bytes.Add(BitConverter.GetBytes(midbytes.Length)[1]);
                            bytes.Add(BitConverter.GetBytes(midbytes.Length)[0]);
                            bytes.Add(BitConverter.GetBytes((int)ilabel.infoLabel.label.FontSize)[1]);
                            bytes.Add(BitConverter.GetBytes((int)ilabel.infoLabel.label.FontSize)[0]);

                            midbytes2 = System.Text.Encoding.UTF8.GetBytes(ilabel.infoLabel.fontColor);
                            bytes.Add(BitConverter.GetBytes(midbytes2.Length)[0]);
                            //Console.WriteLine(BitConverter.GetBytes(midbytes2.Length)[0] + "\n" + ilabel.infoLabel.fontColor);
                            midbytes3 = System.Text.Encoding.UTF8.GetBytes(ilabel.infoLabel.label.FontFamily.ToString());
                            bytes.Add(BitConverter.GetBytes(midbytes3.Length)[0]);
                            //Console.WriteLine(BitConverter.GetBytes(midbytes3.Length)[0] + "\n" + ilabel.infoLabel.label.FontFamily.ToString());
                            midbytes4 = System.Text.Encoding.UTF8.GetBytes(ilabel.infoLabel.borderColor);
                            bytes.Add(BitConverter.GetBytes(midbytes4.Length)[0]);
                           // Console.WriteLine(BitConverter.GetBytes(midbytes4.Length)[0] + "\n" + ilabel.infoLabel.borderColor);

                            for (int i = 0; i < midbytes.Length; i++)
                                bytes.Add(midbytes[i]);

                            for (int i = 0; i < midbytes2.Length; i++)
                                bytes.Add(midbytes2[i]);

                            for (int i = 0; i < midbytes3.Length; i++)
                                bytes.Add(midbytes3[i]);
                            for (int i = 0; i < midbytes4.Length; i++)
                                bytes.Add(midbytes4[i]);
                            bytes.Add(BitConverter.GetBytes(ilabel.backTrans)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.detectLabel.x)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.detectLabel.x)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.detectLabel.y)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.detectLabel.y)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.detectLabel.width)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.detectLabel.width)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.detectLabel.height)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.detectLabel.height)[0]);
                            midbytes = System.Text.Encoding.UTF8.GetBytes(ilabel.detectLabel.text);
                            bytes.Add(BitConverter.GetBytes(midbytes.Length)[1]);
                            bytes.Add(BitConverter.GetBytes(midbytes.Length)[0]);
                            Console.Write(ilabel.detectLabel.x);
                            for (int i = 0; i < midbytes.Length; i++)
                                bytes.Add(midbytes[i]);
                        }
                        byte[] crcbytes = new byte[bytes.Count];
                        bytes.CopyTo(crcbytes);
                        Int16 crc = CRC16(crcbytes, bytes.Count);
                        
                        bytes.Add(BitConverter.GetBytes(crc)[1]);
                        bytes.Add(BitConverter.GetBytes(crc)[0]);

                        bytes1.AddRange(bytes);          
                        bytes1.Add(0xff);
                        bytes1.Add(0xed);
                        imgFS = new FileStream(filename, FileMode.Append);
                        imgFS.Write(bytes1.ToArray(), 0, bytes1.Count);
                        imgFS.Flush();
                        imgFS.Close();
                    }

                    this.Cursor = System.Windows.Input.Cursors.Arrow;
                    MessageBoxResult result = System.Windows.MessageBox.Show("保存成功", "保存提示");
                }
                else
                    error(2);
            }
            else
                error(1);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(imgready)
            {
                foreach (InfoLabelPro i in infoLabelPros)
                {
                    i.infoLabel.Appear();
                    i.detectLabel.Appear();
                }
            }
        }
    }
    public class InfoLabelPro
    {
        public InfoLabel infoLabel;
        public InfoLabel detectLabel;
        public bool backTrans;
        public InfoLabelPro(InfoLabel info, InfoLabel detect, bool backtrans)
        {
            infoLabel = info;
            detectLabel = detect;
            backTrans = backtrans;
            if (backtrans)
                infoLabel.label.Background = Brushes.Transparent;
            infoLabel.label.MouseEnter += new System.Windows.Input.MouseEventHandler(appear_MouseEnter);
            infoLabel.label.MouseLeave += new System.Windows.Input.MouseEventHandler(disappear_MouseLeave);
            infoLabel.border.MouseEnter += new System.Windows.Input.MouseEventHandler(adjust_MouseEnter);
            infoLabel.border.MouseLeave += new System.Windows.Input.MouseEventHandler(adjust_MouseLeave);

            detectLabel.label.MouseEnter += new System.Windows.Input.MouseEventHandler(appear_MouseEnter_detec);
            detectLabel.label.MouseLeave += new System.Windows.Input.MouseEventHandler(disappear_MouseLeave_detec);
            detectLabel.border.MouseEnter += new System.Windows.Input.MouseEventHandler(adjust_MouseEnter_detec);
            detectLabel.border.MouseLeave += new System.Windows.Input.MouseEventHandler(adjust_MouseLeave_detec);

        }
        public void trans_alter(bool trans)
        {
            if(trans)
            {
                backTrans = true;
                infoLabel.label.Background = Brushes.Transparent;
            }
            else
            {
                backTrans = false;
                infoLabel.label.Background = infoLabel.border.BorderBrush;
            }
        }
        private void appear_MouseEnter(object sender, System.EventArgs e)
        {
            if(MainWindow.editMode)
            {
                infoLabel.mouseEnter = true;
                infoLabel.typingleave = false;
                infoLabel.label.Opacity = 1;
                infoLabel.border.Opacity = 1;
                detectLabel.label.Opacity = 1;
                detectLabel.border.Opacity = 1;
            }
        }
        private void disappear_MouseLeave(object sender, System.EventArgs e)
        {
            Console.WriteLine("leave");
            infoLabel.mouseEnter = false;
            if (infoLabel.isdisappear && !infoLabel.isSelected)
            {
                infoLabel.label.Opacity = 0;
                infoLabel.border.Opacity = 0;
            }
        }
        void adjust_MouseEnter(object sender, System.EventArgs e)
        {
            if (!MainWindow.editMode)
                return;
            System.Windows.Input.MouseEventHandler disappear_mouseEventHandler = new System.Windows.Input.MouseEventHandler(appear_MouseEnter);

            infoLabel.label.Opacity = 1;
            infoLabel.border.Opacity = 1;
            detectLabel.label.Opacity = 1;
            detectLabel.border.Opacity = 1;
            Point p = Mouse.GetPosition(infoLabel.border);
            int x = (int)p.X;
            int y = (int)p.Y;
            int size = 5;
            if (x < size)
            {
                if (y < size)
                {
                    infoLabel.border.Cursor = System.Windows.Input.Cursors.SizeNWSE;
                    infoLabel.dir = InfoLabel.Dir.upleft;
                }
                else if (Math.Abs(y - infoLabel.height) < size)
                {
                    infoLabel.border.Cursor = System.Windows.Input.Cursors.SizeNESW;
                    infoLabel.dir = InfoLabel.Dir.downleft;
                }
                else
                {
                    infoLabel.border.Cursor = System.Windows.Input.Cursors.SizeWE;
                    infoLabel.dir = InfoLabel.Dir.left;
                }
            }
            else if (Math.Abs(x - infoLabel.width) < size)
            {
                if (y < size)
                {
                    infoLabel.border.Cursor = System.Windows.Input.Cursors.SizeNESW;
                    infoLabel.dir = InfoLabel.Dir.upright;
                }
                else if (Math.Abs(y - infoLabel.height) < size)
                {
                    infoLabel.border.Cursor = System.Windows.Input.Cursors.SizeNWSE;
                    infoLabel.dir = InfoLabel.Dir.downright;
                }
                else
                {
                    infoLabel.border.Cursor = System.Windows.Input.Cursors.SizeWE;
                    infoLabel.dir = InfoLabel.Dir.right;
                }
            }
            else
            {
                if (y < size)
                {
                    infoLabel.border.Cursor = System.Windows.Input.Cursors.SizeNS;
                    infoLabel.dir = InfoLabel.Dir.up;
                }
                else
                {
                    infoLabel.border.Cursor = System.Windows.Input.Cursors.SizeNS;
                    infoLabel.dir = InfoLabel.Dir.down;
                }
            }
        }
        void adjust_MouseLeave(object sender, System.EventArgs e)
        {
            if (infoLabel.isdisappear)
            {
                infoLabel.label.Opacity = 0;
                infoLabel.border.Opacity = 0;
            }
        }
        private void appear_MouseEnter_detec(object sender, System.EventArgs e)
        {
            detectLabel.mouseEnter = true;
            detectLabel.typingleave = false;
            detectLabel.label.Opacity = 1;
            detectLabel.border.Opacity = 1;
            infoLabel.label.Opacity = 1;
            infoLabel.border.Opacity = 1;
        }
        private void disappear_MouseLeave_detec(object sender, System.EventArgs e)
        {
            Console.WriteLine("leave");
            detectLabel.mouseEnter = false;
            if (detectLabel.isdisappear && !detectLabel.isSelected)
            {
                detectLabel.label.Opacity = 0;
                detectLabel.border.Opacity = 0;
                infoLabel.label.Opacity = 0;
                infoLabel.border.Opacity = 0;
            }
        }
        void adjust_MouseEnter_detec(object sender, System.EventArgs e)
        {
            if (!MainWindow.editMode)
                return;
            System.Windows.Input.MouseEventHandler disappear_mouseEventHandler = new System.Windows.Input.MouseEventHandler(appear_MouseEnter_detec);

            detectLabel.label.Opacity = 1;
            detectLabel.border.Opacity = 1;
            infoLabel.label.Opacity = 1;
            infoLabel.border.Opacity = 1;
            Point p = Mouse.GetPosition(detectLabel.border);
            int x = (int)p.X;
            int y = (int)p.Y;
            int size = 5;
            if (x < size)
            {
                if (y < size)
                {
                    detectLabel.border.Cursor = System.Windows.Input.Cursors.SizeNWSE;
                    detectLabel.dir = InfoLabel.Dir.upleft;
                }
                else if (Math.Abs(y - detectLabel.height) < size)
                {
                    detectLabel.border.Cursor = System.Windows.Input.Cursors.SizeNESW;
                    detectLabel.dir = InfoLabel.Dir.downleft;
                }
                else
                {
                    detectLabel.border.Cursor = System.Windows.Input.Cursors.SizeWE;
                    detectLabel.dir = InfoLabel.Dir.left;
                }
            }
            else if (Math.Abs(x - detectLabel.width) < size)
            {
                if (y < size)
                {
                    detectLabel.border.Cursor = System.Windows.Input.Cursors.SizeNESW;
                    detectLabel.dir = InfoLabel.Dir.upright;
                }
                else if (Math.Abs(y - detectLabel.height) < size)
                {
                    detectLabel.border.Cursor = System.Windows.Input.Cursors.SizeNWSE;
                    detectLabel.dir = InfoLabel.Dir.downright;
                }
                else
                {
                    detectLabel.border.Cursor = System.Windows.Input.Cursors.SizeWE;
                    detectLabel.dir = InfoLabel.Dir.right;
                }
            }
            else
            {
                if (y < size)
                {
                    detectLabel.border.Cursor = System.Windows.Input.Cursors.SizeNS;
                    detectLabel.dir = InfoLabel.Dir.up;
                }
                else
                {
                    detectLabel.border.Cursor = System.Windows.Input.Cursors.SizeNS;
                    detectLabel.dir = InfoLabel.Dir.down;
                }
            }
        }
        void adjust_MouseLeave_detec(object sender, System.EventArgs e)
        {
            if (detectLabel.isdisappear)
            {
                detectLabel.label.Opacity = 0;
                detectLabel.border.Opacity = 0;
                infoLabel.label.Opacity = 0;
                infoLabel.border.Opacity = 0;
            }
        }


    }
    public class InfoLabel
    {
        public bool mouseEnter = false;
        public bool typingleave = true;
        public bool isdisappear = true;
        public bool isSelected = false;
        static public bool onlyone = false;
        bool isinfo;
        public string fontColor;
        public string borderColor;
        public System.Windows.Controls.Label label;
        public Border border;
        public System.Windows.Controls.TextBox textBox;
        public System.Windows.Forms.Timer moveTimer = new System.Windows.Forms.Timer(), adjustTimer = new System.Windows.Forms.Timer();
        public int x { get; set; }
        public int y { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public string text { get; set; }
        public enum Dir{up,upright,right,downright,down,downleft,left,upleft};
        public Dir dir;
        public InfoLabel(int X, int Y, int Width, int Height, string Text,double fontSize ,string fontColorName, FontFamily fontFamily, string borderColorName)
        {
            isinfo = true;
            label = new System.Windows.Controls.Label();
            
            label.FontSize = fontSize;
            label.FontFamily = fontFamily;
            textBox = new System.Windows.Controls.TextBox();
            textBox.AcceptsReturn = true;
            border = new Border();
            this.x = X;
            this.y = Y;
            this.height = Height;
            this.width = Width;
            this.text = Text;
            this.fontColor = fontColorName;
            try
            {
                label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fontColor));
            }
            catch
            {
                MainWindow.error(0);
            }
            label.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            label.VerticalAlignment = VerticalAlignment.Top;
            border.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            border.VerticalAlignment = VerticalAlignment.Top;
            textBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            textBox.VerticalAlignment = VerticalAlignment.Top;
            label.Margin = new Thickness(x, y, 0, 0);
            label.Width = width;
            label.Height = height;
            border.Margin = new Thickness(x, y, 0, 0);
            border.Width = width;
            border.Height = height;
            adaptLabelText();
            textBox.Margin = new Thickness(x, y, 0, 0);
            textBox.Width = width;
            textBox.Height = height;
            textBox.Text = Text;
            textBox.Visibility = Visibility.Hidden;
            this.borderColor = borderColorName;
            try
            {
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(borderColor));   
            }
            catch
            {
                MainWindow.error(0);
            }
            border.Opacity = 1;
            border.BorderThickness = new Thickness(1);

            label.Background = border.BorderBrush;

            label.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
            label.VerticalContentAlignment = VerticalAlignment.Center;
           
            label.AddHandler(System.Windows.Controls.Label.MouseDoubleClickEvent, new MouseButtonEventHandler((o, a) =>
            {
                Console.Write(123);
                textBox.Visibility = Visibility.Visible;
            }));
            label.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(startmove_MouseLeftButtonDown);
            label.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(stopmove_MouseLeftButtonUp);
            textBox.TextChanged += new TextChangedEventHandler(text_TextChanged);
            textBox.MouseLeave += new System.Windows.Input.MouseEventHandler(textbox_MouseLeave);
            border.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(adjust_MouseLeftButtonDown);
            //border.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(adjust_MouseLeftButtonUp);
        }

        public InfoLabel(int X, int Y, int Width, int Height, string url, string borderColorName)
        {
            isinfo = false;
            label = new System.Windows.Controls.Label();
            textBox = new System.Windows.Controls.TextBox();
            textBox.AcceptsReturn = true;
            textBox.ToolTip = "输入URL或本地文件地址(双击可直接选择文件)";
            border = new Border();
            this.x = X;
            this.y = Y;
            this.height = Height;
            this.width = Width;
            this.text = url;
            label.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            label.VerticalAlignment = VerticalAlignment.Top;
            border.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            border.VerticalAlignment = VerticalAlignment.Top;
            textBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            textBox.VerticalAlignment = VerticalAlignment.Top;
            label.Margin = new Thickness(x, y, 0, 0);
            label.Width = width;
            label.Height = height;
            border.Margin = new Thickness(x, y, 0, 0);
            border.Width = width;
            border.Height = height;
            textBox.Margin = new Thickness(x, y, 0, 0);
            textBox.Width = width;
            textBox.Height = height;
            textBox.Text = url;
            textBox.Visibility = Visibility.Hidden;
            this.borderColor = borderColorName;
            try
            {
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(borderColor));
            }
            catch
            {
                MainWindow.error(0);
            }
            border.Opacity = 1;
            border.BorderThickness = new Thickness(1);
            

            label.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
            label.VerticalContentAlignment = VerticalAlignment.Center;
            label.AddHandler(System.Windows.Controls.Label.MouseDoubleClickEvent, new MouseButtonEventHandler((o, a) =>
            {
                Console.Write(123);
                if (MainWindow.editMode)
                    textBox.Visibility = Visibility.Visible;
                else
                    try
                    {
                        Console.Write(System.IO.Path.GetDirectoryName(MainWindow.filename) + '\\' + text);
                        if (text!="")
                            System.Diagnostics.Process.Start(System.IO.Path.GetDirectoryName(MainWindow.filename)+'\\'+text);
                    }
                    catch{
                        try { System.Diagnostics.Process.Start(text); }
                        catch { }
                    }
            }));
            textBox.AddHandler(System.Windows.Controls.Label.MouseDoubleClickEvent, new MouseButtonEventHandler((o, a) =>
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = false;//该值确定是否可以选择多个文件
                dialog.Title = "请选择文件";
                dialog.Filter = "任意文件(*.*)|*.*";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    System.Uri uri1 = new Uri(dialog.FileName);
                    Console.Write(System.IO.Path.GetDirectoryName(MainWindow.filename));
                    System.Uri uri2 = new Uri(MainWindow.filename);

                    Uri relativeUri = uri2.MakeRelativeUri(uri1);
                    text = relativeUri.ToString();
                    textBox.Text = text;
                }
            }));
            label.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(startmove_MouseLeftButtonDown);
            label.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(stopmove_MouseLeftButtonUp);
            textBox.TextChanged += new TextChangedEventHandler(text_TextChanged);
            textBox.MouseLeave += new System.Windows.Input.MouseEventHandler(textbox_MouseLeave);
            border.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(adjust_MouseLeftButtonDown);
            //border.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(adjust_MouseLeftButtonUp);
        }

        void adaptLabelText()
        {
            if (text == "")
                return;
            List<string> slist = new List<string>();
            int midnum = 0;
            for(int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    slist.Add(text.Substring(midnum, i - midnum + 1));
                    midnum = i + 1;
                }
            }
            if (midnum != text.Length)
                slist.Add(text.Substring(midnum, text.Length - midnum));
            label.Content = null;
            for (int i = 0; i < slist.Count; i++)
            {
                double d = GetTextDisplayWidth(slist[i] + "ffffff", label.FontFamily, label.FontStyle, label.FontWeight, label.FontStretch, label.FontSize);
                while (d > width)
                {
                    int len =(int)(width / d * slist[i].Length);
                    if (len == 1)
                        len = 2;
                    label.Content += slist[i].Substring(0, len-1) + "\n";
                    slist[i] = slist[i].Substring(len,slist[i].Length - len);
                    d = GetTextDisplayWidth(slist[i], label.FontFamily, label.FontStyle, label.FontWeight, label.FontStretch, label.FontSize);
                }
                label.Content += slist[i];
            }
        }
        public Double GetTextDisplayWidth(string str, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double FontSize)
        {
            var formattedText = new FormattedText(
                      str,
                      CultureInfo.CurrentUICulture,
                      System.Windows.FlowDirection.LeftToRight,
                      new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                      FontSize,
                      Brushes.Black
                      );
            Size size = new Size(formattedText.Width, formattedText.Height);
            return size.Width;
        }
        public void Appear()
        {
            label.Opacity = 1;
            border.Opacity = 1;
        }
        public void disAppear()
        {
            label.Opacity = 0;
            border.Opacity = 0;
        }
        void adjust_MouseLeftButtonDown(object sender, System.EventArgs e)
        {
            if (!MainWindow.editMode)
                return;
            adjustTimer.Stop();
            isdisappear = false;
            adjustTimer = new System.Windows.Forms.Timer();
            adjustTimer.Interval = 10;
            adjustTimer.Tick += delegate
            {
                Point p = Mouse.GetPosition(label);
                int X = (int)p.X, Y = (int)p.Y;
                switch(dir)
                {
                    case Dir.up:
                        move(x,y+Y);
                        adjustSize(width, height - Y);
                        
                        break;
                    case Dir.upright:
                        move(x, y + Y);
                        adjustSize(X, height - Y);
                        break;
                    case Dir.right:
                        adjustSize(X, height);
                        break;
                    case Dir.downright:
                        adjustSize(X, Y);
                        break;
                    case Dir.down:
                        adjustSize(width, Y);
                        break;
                    case Dir.downleft:
                        move(x+X, y);
                        adjustSize(width - X, Y);
                        break;
                    case Dir.left:
                        move(x+X, y);
                        adjustSize(width - X, height);
                        break;
                    case Dir.upleft:
                        move(x+X, y + Y);
                        adjustSize(width - X, height - Y);
                        break;
                }

            };
            adjustTimer.Start();
        }
        public void adjust_MouseLeftButtonUp(object sender, System.EventArgs e)
        {
            isdisappear = true;
            adjustTimer.Stop();

        }
        
        public void adjustSize(int Width, int Height)
        {
            if (Width <= 0 || Height <= 0)
                return;
            width = Width;
            height = Height;
            label.Width = width;
            label.Height = height;
            border.Width = width;
            border.Height = height;
            textBox.Width = width;
            textBox.Height = height;
            if(isinfo)
                adaptLabelText();
        }
        public void move(int X, int Y)
        {
            this.x = Math.Max(X,10);
            this.x = Math.Min(x,(int)MainWindow.themainWindow.Width - 10 - this.width);
            this.y = Math.Max(Y,40);
            this.y = Math.Min(y, (int)MainWindow.themainWindow.Height - 40 - this.height);
            label.Margin = new Thickness(x, y, 0, 0);
            border.Margin = new Thickness(x, y, 0, 0);
            textBox.Margin = new Thickness(x, y, 0, 0);
        }
        private void startmove_MouseLeftButtonDown(object sender, System.EventArgs e)
        {
            if (!MainWindow.editMode)
                return;
            if (onlyone)
                return;
            else
                onlyone = true;
            isSelected = true;
            if (textBox.Visibility == Visibility.Visible)
                return;
            isdisappear = false;
            moveTimer = new System.Windows.Forms.Timer();
            moveTimer.Interval = 20;
            int i = 0;
            moveTimer.Tick += delegate
            {
                i++;
                if(i>5)
                move2mouse();
            };

            moveTimer.Start();
        }
        void move2mouse()
        {
            Point p = Mouse.GetPosition(label);
            move(x + (int)p.X - width/2, y + (int)p.Y - height/2);
        }
        //void 
        public void stopmove_MouseLeftButtonUp(object sender, System.EventArgs e)
        {
            if(moveTimer != null)
                moveTimer.Stop();
            onlyone = false;
            isdisappear = true;
        }
        private void text_TextChanged(object sender, System.EventArgs e)
        {
            text = textBox.Text;
            if(isinfo)
                adaptLabelText();
        }
        private void textbox_MouseLeave(object sender, System.EventArgs e)
        {
            typingleave = true;
        }
    }
}
