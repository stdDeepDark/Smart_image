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
        InfoLabel infoLabel;
        string filename;
        public static MainWindow themainWindow;
        bool hasLabel = false;
        int endOfJpg,start,end;
        byte[] srcbyte;
        bool imgready = false;
        static public bool editMode;
        //string imginfo;
        bool isNewLabel = false;
        bool istoolBar = false;
        List<InfoLabel> infoLabels;
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
           
            if (infoLabels != null)
                foreach (InfoLabel i in infoLabels)
                    if (i.isSelected)
                    {
                        i.borderColor = BorderColor_ComboBox.SelectedValue.ToString().Substring(BorderColor_ComboBox.SelectedValue.ToString().IndexOf(' ') + 1);
                        Color color = (Color)ColorConverter.ConvertFromString(i.borderColor);
                        i.border.BorderBrush = new SolidColorBrush(color);
                    }
        }

        void fontColor_SelectionChanged(object sender, System.EventArgs e)
        {

            if (infoLabels != null)
                foreach (InfoLabel i in infoLabels)
                    if (i.isSelected)
                    {
                        i.fontColor = FontColor_ComboBox.SelectedValue.ToString().Substring(FontColor_ComboBox.SelectedValue.ToString().IndexOf(' ')+1);
                        Color color = (Color)ColorConverter.ConvertFromString(i.fontColor);
                        i.label.Foreground = new SolidColorBrush(color);
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
            if (infoLabels != null)
                foreach (InfoLabel i in infoLabels)
                    if (i.isSelected)
                        i.label.FontSize = size;
        }
        void fontFamily_SelectionChanged(object sender, System.EventArgs e)
        {
           if(infoLabels != null)
            foreach (InfoLabel i in infoLabels)
                if (i.isSelected)
                    i.label.FontFamily = new FontFamily(fontFamily_ComboBox.SelectedValue.ToString());
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
            if (infoLabels != null)
                foreach (InfoLabel i in infoLabels)
                {
                    if (i.typingleave)
                        i.textBox.Visibility = Visibility.Hidden;
                    
                    if (!istoolBar && i.isSelected && !i.mouseEnter)
                    { 
                        i.isSelected = false;
                    }
                
                }
            if (infoLabels != null)
                foreach (InfoLabel i in infoLabels)
                {

                     if (i.isSelected == true)
                    {
                        flag1 = false;
                        fontFamily_ComboBox.Text = i.label.FontFamily.ToString();
                        fontSize_textBox.Text = i.label.FontSize.ToString();
                        string fontcolor = "System.Windows.Media.Color" + " " + i.fontColor;
                        for(int u = 0; u < FontColor_ComboBox.Items.Count; u++)
                        {
                            if(FontColor_ComboBox.Items[u].ToString() == fontcolor)
                            {
                                FontColor_ComboBox.SelectedIndex = u;
                                break;
                            }
                        }
                        string bordercolor = "System.Windows.Media.Color" + " " + i.borderColor;
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
            }
            if (isNewLabel)
            {
                Point point = Mouse.GetPosition(Grid1);
                infoLabel = new InfoLabel((int)point.X,(int)point.Y, 10, 10, "双击输入文本", 10, "Black", new FontFamily("Microsoft YaHei UI"), "Black");
                Grid1.Children.Add(infoLabel.label);
                Grid1.Children.Add(infoLabel.border);
                Grid1.Children.Add(infoLabel.textBox);
                infoLabels.Add(infoLabel);
                infoLabel.isdisappear = false;
                newLabelTimer = new System.Windows.Forms.Timer();
                newLabelTimer.Interval = 20;
                newLabelTimer.Tick += delegate
                {
                    point = Mouse.GetPosition(infoLabel.border);
                    infoLabel.adjustSize((int)point.X,(int)point.Y);
                };
                newLabelTimer.Start();
            }
        }
        void Grid1_MouseLeftButtonUp(object sender, System.EventArgs e)
        {
            if(isNewLabel)
            {

                infoLabel.isdisappear = true;
                newLabelTimer.Stop();
                isNewLabel = false;
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
            if (infoLabels == null)
                return;
            foreach( InfoLabel i in infoLabels)
            {
                if(i.adjustTimer.Enabled == true)
                    i.adjust_MouseLeftButtonUp(sender,e);
                if (i.moveTimer.Enabled == true)
                    i.stopmove_MouseLeftButtonUp(sender, e);
            }
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(*.*)|*.*";
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
            if(infoLabels != null)
            {
                Grid1.Dispatcher.Invoke(new MethodInvoker(delegate
                {
                    foreach (InfoLabel infolabel in infoLabels)
                    {
                        Grid1.Children.Remove(infolabel.label);
                        Grid1.Children.Remove(infolabel.border);
                        Grid1.Children.Remove(infolabel.textBox);
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
            infoLabels = new List<InfoLabel>();
            if (hasLabel)
            {
                byte[] imgbyte = new byte[end - start + 1];
                Buffer.BlockCopy(srcbyte, start, imgbyte, 0, end - start + 1);
                int x, y, width, height, fontsize,textlen, fontcolorlen ,fontfamilylen, bordercolorlen;
                string text, fontcolor, fontfamily, bordercolor;
                try
                {
                    for (int i = 2; i < imgbyte.Length - 2; i++)
                    {
                        if (i + 13 >= imgbyte.Length - 2)
                        {
                            error(0);
                            break;
                        }
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

                        //BitConverter.ToString(imgbyte, i+10, len);
                        this.Dispatcher.Invoke(new MethodInvoker(delegate
                        {
                            infoLabels.Add(new InfoLabel(x, y, width, height, text, fontsize, fontcolor, new FontFamily(fontfamily), bordercolor));
                        }));
                        i += 14 + textlen + fontcolorlen + fontfamilylen + bordercolorlen;
                    }
                    Grid1.Dispatcher.Invoke(new MethodInvoker(delegate
                    {
                        foreach (InfoLabel infolabel in infoLabels)
                        {
                            Grid1.Children.Add(infolabel.label);
                            Grid1.Children.Add(infolabel.border);
                            Grid1.Children.Add(infolabel.textBox);
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
            if (infoLabels != null)
                for(int i = infoLabels.Count-1;  i >= 0; i--)
                    if (infoLabels[i].isSelected)
                    {
                        Grid1.Children.Remove(infoLabels[i].label);
                        Grid1.Children.Remove(infoLabels[i].border);
                        Grid1.Children.Remove(infoLabels[i].textBox);
                        infoLabels.Remove(infoLabels[i]);
                    }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void mode_button_Click(object sender, RoutedEventArgs e)
        {
            if(editMode)
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
                        List<byte> bytes = new List<byte>();
                        byte[] midbytes = new byte[start];
                        byte[] midbytes2, midbytes3 , midbytes4;
                        Buffer.BlockCopy(srcbyte, 0, midbytes, 0, start);
                        bytes.AddRange(midbytes);
                        bytes.Add(0xff);
                        bytes.Add(0xaa);
                        foreach (InfoLabel ilabel in infoLabels)
                        {
                            bytes.Add(BitConverter.GetBytes(ilabel.x)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.x)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.y)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.y)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.width)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.width)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.height)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.height)[0]);
                            midbytes = System.Text.Encoding.UTF8.GetBytes(ilabel.text);
                            bytes.Add(BitConverter.GetBytes(midbytes.Length)[1]);
                            bytes.Add(BitConverter.GetBytes(midbytes.Length)[0]);
                            bytes.Add(BitConverter.GetBytes((int)ilabel.label.FontSize)[1]);
                            bytes.Add(BitConverter.GetBytes((int)ilabel.label.FontSize)[0]);
                     
                            midbytes2 = System.Text.Encoding.UTF8.GetBytes(ilabel.fontColor);
                            bytes.Add(BitConverter.GetBytes(midbytes2.Length)[0]);
                            Console.WriteLine(BitConverter.GetBytes(midbytes2.Length)[0] + "\n" + ilabel.fontColor);
                            midbytes3 = System.Text.Encoding.UTF8.GetBytes(ilabel.label.FontFamily.ToString());
                            bytes.Add(BitConverter.GetBytes(midbytes3.Length)[0]);
                            Console.WriteLine(BitConverter.GetBytes(midbytes3.Length)[0] + "\n" + ilabel.label.FontFamily.ToString());
                            midbytes4 = System.Text.Encoding.UTF8.GetBytes(ilabel.borderColor);
                            bytes.Add(BitConverter.GetBytes(midbytes4.Length)[0]);
                            Console.WriteLine(BitConverter.GetBytes(midbytes4.Length)[0] + "\n" + ilabel.borderColor);

                            for (int i = 0; i < midbytes.Length; i++)
                                bytes.Add(midbytes[i]);

                            for (int i = 0; i < midbytes2.Length; i++)
                                bytes.Add(midbytes2[i]);

                            for (int i = 0; i < midbytes3.Length; i++)
                                bytes.Add(midbytes3[i]);
                            for (int i = 0; i < midbytes4.Length; i++)
                                bytes.Add(midbytes4[i]);
                        }
                        bytes.Add(0xff);
                        bytes.Add(0xed);
                        midbytes = new byte[srcbyte.Length - end - 1];
                        Buffer.BlockCopy(srcbyte, end + 1, midbytes, 0, midbytes.Length);
                        bytes.AddRange(midbytes);
                        imgFS = new FileStream(filename, FileMode.Open);
                        imgFS.Seek(0, SeekOrigin.Begin);
                        imgFS.Write(bytes.ToArray(), 0, bytes.Count);
                        imgFS.Flush();
                        imgFS.Close();
                    }
                    else
                    {
                        List<byte> bytes = new List<byte>();
                        byte[] midbytes2, midbytes3, midbytes4;
                        bytes.Add(0xff);
                        bytes.Add(0xaa);

                        foreach (InfoLabel ilabel in infoLabels)
                        {
                            bytes.Add(BitConverter.GetBytes(ilabel.x)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.x)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.y)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.y)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.width)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.width)[0]);
                            bytes.Add(BitConverter.GetBytes(ilabel.height)[1]);
                            bytes.Add(BitConverter.GetBytes(ilabel.height)[0]);
                            byte[] midbytes;
                            midbytes = System.Text.Encoding.UTF8.GetBytes(ilabel.text);
                            bytes.Add(BitConverter.GetBytes(midbytes.Length)[1]);
                            bytes.Add(BitConverter.GetBytes(midbytes.Length)[0]);
                            bytes.Add(BitConverter.GetBytes((int)ilabel.label.FontSize)[1]);
                            bytes.Add(BitConverter.GetBytes((int)ilabel.label.FontSize)[0]);

                            midbytes2 = System.Text.Encoding.UTF8.GetBytes(ilabel.fontColor);
                            bytes.Add(BitConverter.GetBytes(midbytes2.Length)[0]);
                            midbytes3 = System.Text.Encoding.UTF8.GetBytes(ilabel.label.FontFamily.ToString());
                            bytes.Add(BitConverter.GetBytes(midbytes3.Length)[0]);
                            midbytes4 = System.Text.Encoding.UTF8.GetBytes(ilabel.borderColor);
                            bytes.Add(BitConverter.GetBytes(midbytes4.Length)[0]);
                            Console.WriteLine(BitConverter.GetBytes(midbytes2.Length)[0] + "\n" + ilabel.fontColor);
                            Console.WriteLine(BitConverter.GetBytes(midbytes3.Length)[0] + "\n" + ilabel.label.FontFamily.ToString());
                            Console.WriteLine(BitConverter.GetBytes(midbytes4.Length)[0] + "\n" + ilabel.borderColor);
                            for (int i = 0; i < midbytes.Length; i++)
                                bytes.Add(midbytes[i]);

                            for (int i = 0; i < midbytes2.Length; i++)
                                bytes.Add(midbytes2[i]);

                            for (int i = 0; i < midbytes3.Length; i++)
                                bytes.Add(midbytes3[i]);

                            for (int i = 0; i < midbytes4.Length; i++)
                                bytes.Add(midbytes4[i]);
                        }
                        bytes.Add(0xff);
                        bytes.Add(0xed);
                        imgFS = new FileStream(filename, FileMode.Append);
                        imgFS.Write(bytes.ToArray(), 0, bytes.Count);
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
                foreach (InfoLabel i in infoLabels)
                    i.Appear();
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
        enum Dir{up,upright,right,downright,down,downleft,left,upleft};
        Dir dir;
        public InfoLabel(int X, int Y, int Width, int Height, string Text,double fontSize ,string fontColorName, FontFamily fontFamily, string borderColorName)
        {
            label = new System.Windows.Controls.Label();

            label.FontSize = fontSize;
            label.FontFamily = fontFamily;
            textBox = new System.Windows.Controls.TextBox();
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

            label.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
            label.VerticalContentAlignment = VerticalAlignment.Center;
           
            label.MouseEnter += new System.Windows.Input.MouseEventHandler(appear_MouseEnter);
            label.MouseLeave += new System.Windows.Input.MouseEventHandler(disappear_MouseLeave);

            border.MouseEnter += new System.Windows.Input.MouseEventHandler(adjust_MouseEnter);
            border.MouseLeave += new System.Windows.Input.MouseEventHandler(adjust_MouseLeave);

            label.AddHandler(System.Windows.Controls.Label.MouseDoubleClickEvent, new MouseButtonEventHandler((o, a) =>
            {
                textBox.Visibility = Visibility.Visible;
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
                double d = GetTextDisplayWidth(slist[i] + "ffff", label.FontFamily, label.FontStyle, label.FontWeight, label.FontStretch, label.FontSize);
                while (d > width)
                {
                    int len =(int)(width / d * slist[i].Length);
                    if (len == 0)
                        len = 1;
                    label.Content += slist[i].Substring(0, len) + "\n";
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
        void adjust_MouseEnter(object sender, System.EventArgs e)
        {
            if (!MainWindow.editMode)
                return;
            System.Windows.Input.MouseEventHandler disappear_mouseEventHandler = new System.Windows.Input.MouseEventHandler(appear_MouseEnter);

            label.Opacity = 1;
            border.Opacity = 1;
            Point p = Mouse.GetPosition(border);
            int x = (int)p.X;
            int y = (int)p.Y;
            int size = 5;
            if (x < size )
            {
                if (y < size)
                {
                    border.Cursor = System.Windows.Input.Cursors.SizeNWSE;
                    dir = Dir.upleft;
                }
                else if (Math.Abs(y - height) < size )
                {
                    border.Cursor = System.Windows.Input.Cursors.SizeNESW;
                    dir = Dir.downleft;
                }
                else
                {
                    border.Cursor = System.Windows.Input.Cursors.SizeWE;
                    dir = Dir.left;
                }
            }
            else if(Math.Abs( x - width) < size)
            {
                if (y < size)
                {
                    border.Cursor = System.Windows.Input.Cursors.SizeNESW;
                    dir = Dir.upright;
                }
                else if (Math.Abs(y - height) < size)
                {
                    border.Cursor = System.Windows.Input.Cursors.SizeNWSE;
                    dir = Dir.downright;
                }
                else
                {
                    border.Cursor = System.Windows.Input.Cursors.SizeWE;
                    dir = Dir.right;
                }
            }
            else
            {
                if (y < size)
                {
                    border.Cursor = System.Windows.Input.Cursors.SizeNS;
                    dir = Dir.up;
                }
                else 
                {
                    border.Cursor = System.Windows.Input.Cursors.SizeNS;
                    dir = Dir.down;
                }
            }
        }
        void adjust_MouseLeave(object sender, System.EventArgs e)
        {
            if (isdisappear)
            {
                label.Opacity = 0;
                border.Opacity = 0;
            }
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
                        adjustSize(width, height - Y);
                        
                        break;
                    case Dir.upright:
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
                        adjustSize(width - X, Y);
                        break;
                    case Dir.left:
                        adjustSize(width - X, height);
                        break;
                    case Dir.upleft:
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
            adaptLabelText();
        }
        private void appear_MouseEnter(object sender, System.EventArgs e)
        {
            mouseEnter = true;
            typingleave = false; 
            label.Opacity = 1;
            border.Opacity = 1;
        }
        private void disappear_MouseLeave(object sender, System.EventArgs e)
        {
            Console.WriteLine("leave");
            mouseEnter = false;
            if (isdisappear && !isSelected)
            {
                label.Opacity = 0;
                border.Opacity = 0;
            }
        }
        private void textbox_MouseLeave(object sender, System.EventArgs e)
        {
            typingleave = true;
        }
    }
}
