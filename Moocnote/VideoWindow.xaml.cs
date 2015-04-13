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
        /*
       * 进度时间设置1
       */
        public Bitmap bmp = new Bitmap(Screen.AllScreens[0].Bounds.Width, Screen.AllScreens[0].Bounds.Height);
        //数据库操作对象实例化
        DBConnection db = new DBConnection();

        //视频位置
        String filepath = "";

        public VideoWindow()
        {
            InitializeComponent();
            SetPlayer(false);
            SetTxtbtn(false);

            this.Loaded += new RoutedEventHandler(Window_Loaded);

            mediaElement.MediaOpened += new RoutedEventHandler(mediaElement_MediaOpened);
            mediaElement.MediaEnded += new RoutedEventHandler(mediaElement_MediaEnded);

            /*
           * 进度时间设置2
           */
            //装载计时器
            //timelineSlider.Value = 0;
            //设置计时器精确度 
            //timer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            //timer.Interval = TimeSpan.FromSeconds(0.1);
            /*
            * 进度时间设置2
            */
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

            setNote_List();
           
        }

        #endregion


    private void setNote_List()
        {
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
                        tb2.Width = 180;
                        tb3.Width = 95;


                        //int m = int.Parse(reader.GetString(2)) / 60000;
                        //int s = int.Parse(reader.GetString(2)) / 1000-m*60;
                        //string minute = Convert.ToString(m);
                        //string second = Convert.ToString(s);
                        //tb1.Text = string.Format ("{0:mm}:{1:ss}",minute ,second );

                        tb1.Text = reader.GetString(2);
                        tb2.Text = reader.GetString(3);
                        tb3.Text = reader.GetString(4);

                        notePanel.Children.Add(tb1);
                        notePanel.Children.Add(tb2);
                        notePanel.Children.Add(tb3);

                        notePanel.Uid = reader.GetString(0);

                        notePanel.MouseDown += notePanel_MouseDown;
                        notePanel.MouseEnter +=notePanel_MouseEnter;
                        notePanel.MouseLeave +=notePanel_MouseLeave;
                
                        noteItem.Items.Add(notePanel);
                        //system.windows.forms.messagebox.show(reader.getint32(0) + " " + reader.getstring(1));
                    }
                }
            }
            reader.Close();
        }

    private void notePanel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        StackPanel notepanel = (StackPanel)e.Source;
        notepanel.Background = System.Windows.Media.Brushes.White;
    }

    private void notePanel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        //TextBlock temp = (TextBlock)e.Source;
        //StackPanel notepanel = (StackPanel)temp.Parent;
        StackPanel notepanel = (StackPanel)e.Source;
        notepanel.Background = System.Windows.Media.Brushes.Red;
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
                    TimeSpan ts = TimeSpan.Parse(tb.Text );
                    mediaElement.Position=ts;
                    //Console.WriteLine(tb.Text);
                }
                else if (index == 1)
                {
                    
                    checkedNote.Text = tb.Text;
                    assist.Text = notepanel.Uid;
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





        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timelineSlider.ValueChanged += timelineSlider_ValueChanged;

            //窗口设置
            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
            this.ResizeMode = System.Windows.ResizeMode.CanResize;
            //this.Left = 0.0.; this.Top = 0.0;
            //this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            //this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;

        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //使播放进度条跟随播放时间移动
            //timelineSlider.ToolTip = mediaElement.Position.ToString().Substring(0, 8);

            timelineSlider.Value = mediaElement.Position.TotalMilliseconds;

            // txtTime.Text = mediaElement.Position.ToString().Substring(0, 8);

            txtTime.Text = string.Format(
                               "{0}{1:00}:{2:00}:{3:00}",
                               "播放进度：",
                               mediaElement.Position.Hours,
                               mediaElement.Position.Minutes,
                               mediaElement.Position.Seconds);
        }

        /*
         * 设置播放，暂停，停止，前进，后退按钮是否可用
         */
        private void SetPlayer(bool bVal)
        {
            playBtn.IsEnabled = bVal;
            pauseBtn.IsEnabled = bVal;
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
            string a = mediaElement.Position.ToString();
            string b = a.Substring(0, 8);//获取当前视频的时间
            string[] videotime = b.Split(':');
            int totime = int.Parse(videotime[0]) * 3600 + int.Parse(videotime[1]) * 60 + int.Parse(videotime[2]);
            textBox1.Text = Convert.ToString(totime);
            /*****************************
            *进度时间设置6
            *****************************/
            //timer.Stop();
            /*****************************
            *进度时间设置6
            *****************************/
        }


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
            /*****************************
            *进度时间设置5
            *****************************/
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = mediaElement.Position - TimeSpan.FromSeconds(10);
        }

        private void forwardBtn_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = mediaElement.Position + TimeSpan.FromSeconds(10);
        }

        #region 跳转到指定秒数播放视频
        private void skipBtn_Click(object sender, RoutedEventArgs e)
        {
            string Current_Position = textBox1.Text;
            if (Current_Position != "")
            {
                int current_time = int.Parse(Current_Position);
                int hour = current_time / 3600;
                int minutes = (current_time - (3600 * hour)) / 60;
                int second = current_time - (3600 * hour) - (minutes * 60);
                DateTime nows = DateTime.Now;
                int year = nows.Year;
                int month = nows.Month;
                int day = nows.Day;
                DateTime dt = new DateTime(year, month, day, hour, minutes, second);
                DateTime dt2 = new DateTime(year, month, day, 0, 0, 0);
                TimeSpan times = new TimeSpan((dt - dt2).Ticks);
                mediaElement.Position = times;
                mediaElement.Play();
            }
        }
        #endregion
        /*****************************
        #region 截屏
        private void radButton2_Click(object sender, RoutedEventArgs e)
        {
            Graphics g = Graphics.FromImage(bmp);
            System.Drawing.Size ss = new System.Drawing.Size(Screen.AllScreens[0].Bounds.Width, Screen.AllScreens[0].Bounds.Height);
            g.CopyFromScreen(0, 0, 0, 0, ss);
            frmScreen f = new frmScreen(bmp);
            f.Show();
        }
        #endregion
        *****************************/

        #region 播放进度，跳转到播放的哪个地方
        private void timelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int SliderValue = (int)timelineSlider.Value;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
            mediaElement.Position = ts;
            //timelineSlider.ToolTip = mediaElement.Position.ToString().Substring(0, 8);
            //mediaElement.Play();
        }
        #endregion








        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {

            timelineSlider.Minimum = 0;
            timelineSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            duration = mediaElement.NaturalDuration.HasTimeSpan ? mediaElement.NaturalDuration.TimeSpan : TimeSpan.FromMilliseconds(0);
            totalTime.Text += string.Format(
                 "{0}{1:00}:{2:00}:{3:00}", "总时长：",
                 duration.Hours,
                 duration.Minutes,
                 duration.Seconds);

            SetupTimer();
        }

        private void SetupTimer()
        {

            timer = new DispatcherTimer();

            // 每 500 毫秒调用一次指定的方法
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();


        }




        private void MediaTimeChanged(object sender, EventArgs e)
        {
            timelineSlider.Value = mediaElement.Position.TotalMilliseconds;
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
            timelineSlider.Value = 0;
        }


        private void muteBtn_Click(object sender, RoutedEventArgs e)
        {
            // IsMuted - 是否静音
            if (mediaElement.IsMuted == true)
            {
                muteBtn.Content = "静音";
                mediaElement.IsMuted = false;
            }
            else
            {
                muteBtn.Content = "有声";
                mediaElement.IsMuted = true;
            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            String note = noteTxt.Text;
            if (note != "" && filepath != "")
            {
                String sqlfilepath = filepath.Replace("\\", "\\\\");
                string a = mediaElement.Position.ToString();
                string b = a.Substring(0, 8);//获取当前视频的时间
                db.executeUpdate("insert into note values(id," + "'" + sqlfilepath + "'" + ",'" + b+ "'," + "'" + note + "'" + "," + "'" + System.DateTime.Now + "'" + ")");
                setNote_List();
            }
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            db.executeUpdate("delete from note where id = '"+assist.Text +"';");
            checkedNote.Text = "";
            setNote_List();

        }



    }
}