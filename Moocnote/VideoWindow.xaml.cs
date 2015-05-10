using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Threading;
using Moocnote.Utils;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Reflection;


namespace Moocnote
{
    /// <summary>
    /// VideoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class VideoWindow : Window
    {
        /*
         * 进度时间设置1
         */
        private TimeSpan duration;
        private DispatcherTimer timer = new DispatcherTimer();
        private TimeSpan ts2 = new TimeSpan();

        //public Bitmap bmp = new Bitmap(Screen.AllScreens[0].Bounds.Width, Screen.AllScreens[0].Bounds.Height);

        //数据库操作对象实例化
        DBConnection db = new DBConnection();

        //视频位置
        String filepath = "";

        //存储视频时间和对应text
        Dictionary<String, Dictionary<String, String>> dic = new Dictionary<String, Dictionary<String, String>>();
        //暂停时移动进度条的辅助
        //bool isPlayed = true ;

        public VideoWindow()
        {
            InitializeComponent();
            SetPlayer(false);
            SetTxtbtn(false);

            this.Loaded += new RoutedEventHandler(Window_Loaded);

            mediaElement.MediaOpened += new RoutedEventHandler(mediaElement_MediaOpened);
            mediaElement.MediaEnded += new RoutedEventHandler(mediaElement_MediaEnded);
            SetImageForMediaElement();
        }

        //将录像的第一帧作为播放前MediaElement显示的录像截图
        public void SetImageForMediaElement()
        {
            mediaElement.Play();
            mediaElement.Pause();
            mediaElement.Position = TimeSpan.Zero;
        }
        


        #region 选择视频文件对话框
        private void openBtn_Click(object sender, RoutedEventArgs e)
        {

            //弹出打开文件对话框
            OpenFileDialog openDialog = new OpenFileDialog();
            //openDialog.
            openDialog.Filter = @"视频文件|*.avi;*.wav;*.wmv;*.rm;*.rmvb;*.mkv;*.moc;*.asf;*.flv;*.mp4";
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            //获取文件路径
            filepath = openDialog.FileName;
            if (filepath == "")
            {
                return;
            }
            mediaElement.Source = new Uri(filepath, UriKind.Relative);
            playBtn.IsEnabled = true;


            //timer.Tick += new EventHandler(timer_Tick);
            //openBtn.Click += playBtn_Click;
           

            setNote_List();

        }

        #endregion

        #region 刷新笔记列表
        private void setNote_List()
        {
            dic.Clear();

            //刷新notepanel信息
            String sqlfilepath = filepath.Replace("\\", "\\\\");
            MySqlDataReader reader = db.executeQuery("select * from note where videoaddr='" + sqlfilepath + "' order by videotime;");
            if (reader != null)
            {
                noteItem.Items.Clear();
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        StackPanel notePanel = new StackPanel();
                        notePanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                        TextBlock tb1 = new TextBlock();
                        TextBlock tb2 = new TextBlock();
                        TextBlock tb3 = new TextBlock();
                        tb1.Width = 60;
                        tb2.Width = 200;
                        tb3.Width = 75;


                        //int m = int.Parse(reader.GetString(2)) / 60000;
                        //int s = int.Parse(reader.GetString(2)) / 1000-m*60;
                        //string minute = Convert.ToString(m);
                        //string second = Convert.ToString(s);
                        //tb1.Text = string.Format ("{0:mm}:{1:ss}",minute ,second );

                        String videoTime = reader.GetString(2);
                        String note = reader.GetString(3);
                        String id = reader.GetString(0);

                        tb1.Text = videoTime;
                        tb2.Text = note;
                        tb3.Text = reader.GetString(4);

                        tb2.TextTrimming = TextTrimming.CharacterEllipsis;
                        tb3.Margin =new Thickness(13,0,0,0);

                        if (dic.Keys.Contains(videoTime))
                        {
                            dic[videoTime].Add(id, note);
                        }
                        else
                        {
                            Dictionary<String, String> temp = new Dictionary<String, String>();
                            temp.Add(id, note);
                            dic.Add(videoTime, temp);
                        }

                        notePanel.Children.Add(tb1);
                        notePanel.Children.Add(tb2);
                        notePanel.Children.Add(tb3);

                        notePanel.Uid = id;

                        notePanel.MouseDown += notePanel_MouseDown;
                        notePanel.MouseEnter += notePanel_MouseEnter;
                        notePanel.MouseLeave += notePanel_MouseLeave;

                        noteItem.Items.Add(notePanel);
                        //system.windows.forms.messagebox.show(reader.getint32(0) + " " + reader.getstring(1));
                    }
                }
            }
            reader.Close();
        }
        #endregion

        #region 笔记列表的鼠标动作函数
        private void notePanel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            StackPanel notepanel = (StackPanel)e.Source;
            //notepanel.Background = System.Windows.Media.Brushes.White;
            notepanel.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF6F6F6"));
        }

        private void notePanel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //TextBlock temp = (TextBlock)e.Source;
            //StackPanel notepanel = (StackPanel)temp.Parent;
            StackPanel notepanel = (StackPanel)e.Source;
            //notepanel.Background = System.Windows.Media.Brushes.Red;
            notepanel.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF3399FF"));
        }

        private void notePanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                TextBlock temp = (TextBlock)e.Source;
                StackPanel notepanel = (StackPanel)temp.Parent;

                int index = 0;
                foreach (TextBlock tb in notepanel.Children)
                {
                    //一个stackpanel包含三个textblock，由index区别
                    if (index == 0)
                    {
                        TimeSpan ts = TimeSpan.Parse(tb.Text);
                        mediaElement.Position = ts;
                        //Console.WriteLine(tb.Text);
                    }
                    else if (index == 1)
                    {

                        checkedNote.Text = tb.Text;
                        assist.Text = notepanel.Uid;
                        assist2.Text = tb.Text;
                        deleteBtn.IsEnabled = true;
                        canupdateBtn.IsEnabled = true;

                    }
                    else if (index == 2)
                    {

                    }
                    index++;
                }
            }


        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //timelineSlider.ValueChanged += timelineSlider_ValueChanged2;
            //timelineSlider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(timelineSlider_ValueChanged), true);

            //窗口设置
            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip;

            Rect rect = SystemParameters.WorkArea;
            this.MaxWidth = rect.Width + 14;
            this.MaxHeight = rect.Height + 14;
            
            //this.Left = 0.0.; this.Top = 0.0;
            //this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            //this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;

        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //使播放进度条跟随播放时间移动
            //timelineSlider.ToolTip = mediaElement.Position.ToString().Substring(0, 8);

            timelineSlider.Value = mediaElement.Position.TotalSeconds;

            // txtTime.Text = mediaElement.Position.ToString().Substring(0, 8);
            txtTime.Text = string.Format(
                               "{0:00}:{1:00}:{2:00}",
                               
                               mediaElement.Position.Hours,
                               mediaElement.Position.Minutes,
                               mediaElement.Position.Seconds);

        }

        #region 播放器的各种按钮函数
        /*
         * 设置播放，暂停，停止，前进，后退按钮是否可用
         */
        private void SetPlayer(bool bVal)
        {
            playBtn.IsEnabled = bVal;
            //pauseBtn.IsEnabled = bVal;
            stopBtn.IsEnabled = bVal;
            backBtn.IsEnabled = bVal;
            forwardBtn.IsEnabled = bVal;
        }

        private void SetTxtbtn(bool bVal)
        {
            deleteBtn.IsEnabled = bVal;
            canupdateBtn.IsEnabled = bVal;
            cancelBtn.IsEnabled = bVal;
            updateBtn.IsEnabled = bVal;

        }

        /*
         * 播放视频
         */
        private void playBtn_Click(object sender, RoutedEventArgs e)
        {
            SetPlayer(true);
            mediaElement.Play();
            /***************************
            *进度时间设置4
            ****************************/
            //处于暂停状态则继续播放
            //mediaElement.Position = TimeSpan.FromSeconds(uineSlider.Value);
            //判断播放器是否处于暂停状态
            /***************************
            *进度时间设置4
            ****************************/
        }

        /*
         * 暂停播放视频
         */
        private void pauseBtn_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Pause();
        }
        private MediaState GetMediaState(MediaElement myMedia)
        {
            FieldInfo hlp = typeof(MediaElement).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance);
            object helperObject = hlp.GetValue(myMedia);
            FieldInfo stateField = helperObject.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
            MediaState state = (MediaState)stateField.GetValue(helperObject);
            return state;
        }

        private void playImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MediaState state = this.GetMediaState(mediaElement);
            Console.WriteLine("点击按钮之前的状态：" + state);
            if (state == MediaState.Play)
            {
                mediaElement.Pause();
                System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
                Uri uri = new Uri("Images/play.png", UriKind.Relative);
                image.Source = new BitmapImage(uri);
                playBtn.ToolTip = "播放";
            }
            else if (state == MediaState.Pause || state == MediaState.Stop)
            {
                SetPlayer(true);
                mediaElement.Play();
                System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
                Uri uri = new Uri("Images/pause.png", UriKind.Relative);
                image.Source = new BitmapImage(uri);
                playBtn.ToolTip = "暂停";
            }
            
        }


        /*
         * 停止播放视频
         */
        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
            SetPlayer(false);
            playBtn.IsEnabled = true;
            /*****************************
              *进度时间设置5
            *****************************/
            timer.Stop();
            timelineSlider.Value = 0;

            System.Windows.Controls.Image i = (System.Windows.Controls.Image)playBtn.Template.FindName("playImage", playBtn);
            Uri uri = new Uri("Images/play.png", UriKind.Relative);
            i.Source = new BitmapImage(uri);
            playBtn.ToolTip = "播放";
           
            /*****************************
            *进度时间设置5
            *****************************/
        }
        /*
         * 视频快退
         */
        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = mediaElement.Position - TimeSpan.FromSeconds(10);
        }
        /*
         * 视频快进
         */
        private void forwardBtn_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = mediaElement.Position + TimeSpan.FromSeconds(10);
        }

        //private void muteBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    // IsMuted - 是否静音
        //    if (mediaElement.IsMuted == true)
        //    {

        //        Uri uri = new Uri("Images/back.png", UriKind.Relative);
        //        BitmapImage bimg = new BitmapImage(uri);
        //        muteBtn.Background = new ImageBrush(bimg);
        //        //muteBtn.Content = "静音";
        //        mediaElement.IsMuted = false;
        //    }
        //    else
        //    {
        //        muteBtn.Content = "有声";
        //        mediaElement.IsMuted = true;
        //    }
        //}

        private void muteImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // IsMuted - 是否静音
            if (mediaElement.IsMuted == true)
            {
                System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
                Uri uri = new Uri("Images/voiced.png", UriKind.Relative);
                image.Source = new BitmapImage(uri);
                mediaElement.IsMuted = false;
                muteBtn.ToolTip = "对扬声器设置静音";
            }
            else
            {
                System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
                Uri uri = new Uri("Images/unvoiced.png", UriKind.Relative);
                image.Source = new BitmapImage(uri);
                mediaElement.IsMuted = true;
                muteBtn.ToolTip = "对扬声器取消静音";
            }
        }

        private void Image_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
            image.Height = 33;
        }

        private void Image_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
            image.Height = 30;
        }

        private void playImage_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
            image.Height = 41;
        }

        private void playImage_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
            image.Height = 38;
        }


        private void openImage_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
            image.Height = 26;
        }

        private void openImage_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
            image.Height = 25;
        }
        #endregion

        #region 播放进度，跳转到指定的某个地方timelineSlider_ValueChanged
        private void timelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
          
            //int SliderValue = (int)timelineSlider.Value;
            //TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
            //mediaElement.Position = ts;
            //timelineSlider.ToolTip = mediaElement.Position.ToString().Substring(0, 8);
            //mediaElement.Play();

            //在鼠标拖动Thumb的过程中记录其值。
            ts2 = TimeSpan.FromSeconds(e.NewValue);
            string currentPosition = string.Format("{0:00}:{1:00}:{2:00}", (int)ts2.TotalHours, ts2.Minutes, ts2.Seconds);
            txtTime.Text = currentPosition;
            //视频跳到有笔记的一点时，笔记变红，可以修改
            String videoTime = mediaElement.Position.ToString().Substring(0, 8);
            if (dic.Keys.Contains(videoTime))
            {
                assist.Text = dic[videoTime].Keys.ElementAt(0);
                assist2.Text = dic[videoTime].Values.ElementAt(0);
                checkedNote.Text = dic[videoTime].Values.ElementAt(0);
                deleteBtn.IsEnabled = true;
                canupdateBtn.IsEnabled = true;

                foreach (StackPanel notepanel in noteItem.Items)
                {
                    if (notepanel.Uid == dic[videoTime].Keys.ElementAt(0))
                    {
                        //notepanel.Background = System.Windows.Media.Brushes.Red;
                        notepanel.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF3399FF"));
                    }
                    else
                    {
                        //notepanel.Background = System.Windows.Media.Brushes.White;
                        notepanel.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF6F6F6"));
                    }
                }
            }
        }
       

        //当拖动Thumb的鼠标放开时，从指定时间开始播放
        private void timelineSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            mediaElement.Position = ts2;
        }
        #endregion

        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {

            timelineSlider.Minimum = 0;
            timelineSlider.Maximum = mediaElement.NaturalDuration.HasTimeSpan ? mediaElement.NaturalDuration.TimeSpan.TotalSeconds : 0;
            duration = mediaElement.NaturalDuration.HasTimeSpan ? mediaElement.NaturalDuration.TimeSpan : TimeSpan.FromSeconds(0);
            totalTime.Text = string.Format(
                 "{0}{1:00}:{2:00}:{3:00}", "/",
                 duration.Hours,
                 duration.Minutes,
                 duration.Seconds);

            SetupTimer();
            
        }

        private void SetupTimer()
        {

            timer = new DispatcherTimer();

            // 每 500 毫秒调用一次指定的方法
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
            timelineSlider.Value = 0;

            System.Windows.Controls.Image i = (System.Windows.Controls.Image)playBtn.Template.FindName("playImage", playBtn);
            Uri uri = new Uri("Images/play.png", UriKind.Relative);
            i.Source = new BitmapImage(uri);
            playBtn.ToolTip = "播放";

        }

        #region 笔记操作的各种按钮
        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            String note = noteTxt.Text;
            if (note != "" && filepath != "")
            {
                String sqlfilepath = filepath.Replace("\\", "\\\\");
                string a = mediaElement.Position.ToString();
                string b = a.Substring(0, 8);//获取当前视频的时间
                //Console.WriteLine(System.DateTime.Now.ToString("MM-dd HH:mm"));
                db.executeUpdate("insert into note values(id," + "'" + sqlfilepath + "'" + ",'" + b + "'," + "'" + note + "'" + "," + "'" + System.DateTime.Now.ToString("MM-dd HH:mm") + "'" + ")");
                setNote_List();
            }
            noteTxt.Text = "点击输入笔记";
            noteTxt.Foreground = System.Windows.Media.Brushes.Gray;
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            //消息框中需要显示哪些按钮，此处显示“确定”和“取消”

            MessageBoxButtons messButton = MessageBoxButtons.OKCancel;

            //第一个参数是对话框的显示信息，第二个是对话框的标题

            DialogResult dr = System.Windows.Forms.MessageBox.Show("确定要删除此条笔记吗?", "删除笔记", messButton);

            if (dr == System.Windows.Forms.DialogResult.OK)//如果点击“确定”按钮
            {

                db.executeUpdate("delete from note where id = '" + assist.Text + "';");
                checkedNote.Text = "";
                deleteBtn.IsEnabled = false;
                canupdateBtn.IsEnabled = false;
                setNote_List();

            }

            else//如果点击“取消”按钮
            {

            }

        }

        private void canupdateBtn_Click(object sender, RoutedEventArgs e)
        {
            deleteBtn.IsEnabled = false;
            canupdateBtn.IsEnabled = false;
            cancelBtn.IsEnabled = true;
            updateBtn.IsEnabled = true;
            checkedNote.IsReadOnly = false;
            //canupdateBtn.Click += pauseBtn_Click;
            mediaElement.Pause();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            
            checkedNote.IsReadOnly = true;
            checkedNote.Text = assist2.Text;
            deleteBtn.IsEnabled = true;
            canupdateBtn.IsEnabled = true;
            cancelBtn.IsEnabled = false;
            updateBtn.IsEnabled = false;

        }

        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (checkedNote.Text ==assist2 .Text )
            {
                //MessageBoxButtons messButton = MessageBoxButtons.OK;
                DialogResult dr = System.Windows.Forms.MessageBox.Show("您对此条笔记没有做任何更改");
                checkedNote.IsReadOnly = true;
                deleteBtn.IsEnabled = true;
                canupdateBtn.IsEnabled = true;
                cancelBtn.IsEnabled = false;
                updateBtn.IsEnabled = false;
            }
            else if (checkedNote.Text != "")
            {
                assist2.Text  = checkedNote.Text;
                db.executeUpdate("update note set note='"+checkedNote .Text +"' where id = '"+assist .Text +"';");
                setNote_List();
                checkedNote.IsReadOnly = true;
                deleteBtn.IsEnabled = true;
                canupdateBtn.IsEnabled = true;
                cancelBtn.IsEnabled = false;
                updateBtn.IsEnabled = false;
            }
            else if (checkedNote.Text == "")
            {
                DialogResult dr = System.Windows.Forms.MessageBox.Show("不能存入一条空笔记");
                checkedNote.Text = assist2.Text;
                checkedNote.IsReadOnly = true;
                deleteBtn.IsEnabled = true;
                canupdateBtn.IsEnabled = true;
                cancelBtn.IsEnabled = false;
                updateBtn.IsEnabled = false;
            }
        }
        #endregion

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void minimizedBtn_Click(object sender, RoutedEventArgs e)
        {
            
                this.WindowState = WindowState.Minimized;
            
            
        }

        private void maximizedBtn_Click(object sender, RoutedEventArgs e)
        {

            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                System.Windows.Controls.Image i = (System.Windows.Controls.Image)maximizedBtn.Template.FindName("zoomImage", maximizedBtn);
                Uri uri = new Uri("Images/maximized.png", UriKind.Relative);
                i.Source = new BitmapImage(uri);
                maximizedBtn.ToolTip = "最大化";

            }
            else
            {
                this.WindowState = WindowState.Maximized;
                System.Windows.Controls.Image i = (System.Windows.Controls.Image)maximizedBtn.Template.FindName("zoomImage", maximizedBtn);
                Uri uri = new Uri("Images/diminish.png", UriKind.Relative);
                i.Source = new BitmapImage(uri);
                maximizedBtn.ToolTip = "向下还原";
                
            } 
          
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void zoomImage_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
            if (this.WindowState == WindowState.Maximized)
            {

                Uri uri = new Uri("Images/diminishPressed.png", UriKind.Relative);
                image.Source = new BitmapImage(uri);
               

            }
            else
            {
                Uri uri = new Uri("Images/maximizedPressed.png", UriKind.Relative);
                image.Source = new BitmapImage(uri);
            } 
            
        }

        private void zoomImage_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
            if (this.WindowState == WindowState.Maximized)
            {

                Uri uri = new Uri("Images/diminish.png", UriKind.Relative);
                image.Source = new BitmapImage(uri);


            }
            else
            {
                Uri uri = new Uri("Images/maximized.png", UriKind.Relative);
                image.Source = new BitmapImage(uri);
            } 
            
        }


        private void closeImage_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
 
                Uri uri = new Uri("Images/closePressed.png", UriKind.Relative);
                image.Source = new BitmapImage(uri);
        }

        private void closeImage_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;
           

                Uri uri = new Uri("Images/close.png", UriKind.Relative);
                image.Source = new BitmapImage(uri);
        }

        private void minimizedImage_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;

            Uri uri = new Uri("Images/minimizedPressed.png", UriKind.Relative);
            image.Source = new BitmapImage(uri);
        }

        private void minimizedImage_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;


            Uri uri = new Uri("Images/minimized.png", UriKind.Relative);
            image.Source = new BitmapImage(uri);
        }

        private void saveImage_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;

            Uri uri = new Uri("Images/savePressed.png", UriKind.Relative);
            image.Source = new BitmapImage(uri);
        }

        private void saveImage_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.Image image = sender as System.Windows.Controls.Image;


            Uri uri = new Uri("Images/save.png", UriKind.Relative);
            image.Source = new BitmapImage(uri);
        }

        private void noteTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            if (noteTxt.Text == "点击输入笔记")
            {
                noteTxt.Text = "";
                noteTxt.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void noteTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            if (noteTxt.Text == "")
            {
                noteTxt.Text = "点击输入笔记";
                noteTxt.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }


    }


}