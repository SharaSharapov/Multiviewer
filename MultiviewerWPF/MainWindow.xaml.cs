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
using System.Xml.Linq;
using Hardcodet.Wpf.TaskbarNotification;

namespace MultiviewerWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string ffmpegDirectory;
        private XDocument document;
        private List<Channel> ChannelList;
        private List<int[,]> coords;

        public void Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public MainWindow()
        {
            InitializeComponent();
            TB.Icon = Properties.Resources.sticker;
             MenuItem item = new MenuItem();
            item.Header = "Close App";
            item.Click += Click;
            TB.ContextMenu.Items.Add(item);
            item = new MenuItem();
            item.Header = "stream info";
            item.Click += StreamInfo;
            TB.ContextMenu.Items.Add(item);

            int baseX = 384;
            int baseY = 250;
            coords = new List<int[,]>();
            
            for(int i=0;i<4; i++)
            {
                for(int j = 0; j < 5; j++)
                {
                    int[,] position = { { j * baseX+1, i * baseY} };
                    coords.Add(position);
                }
            }

            ffmpegDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg\\bin");

           
                document = XDocument.Load(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "channels.xml"));
                if (document != null)
                {
                    ChannelList = new List<Channel>();

                    IEnumerable<XElement> xElements = document.Document.Elements("Channals").Elements("Channel");

                    foreach (XElement element in xElements)
                    {
                        Channel channel = new Channel(element, ffmpegDirectory);
                        int number = int.Parse(element.Element("number").Attribute("value").Value);
                        channel.number = number;
                        channel.x = coords[number - 1][0, 0];
                        channel.y = coords[number - 1][0, 1];
                        Label zz = MainGrid.Children.OfType<Label>().FirstOrDefault(label => label.Name.Equals("ChName" + number.ToString()));
                        zz.Content = channel.name;
                        ChannelList.Add(channel);
                        channel.Start();
                    }
                }
        
        }

        private void StreamInfo(object sender, RoutedEventArgs e)
        {
            Probe probe = new Probe(ChannelList[0].link);
            probe.FFmpegPath = ffmpegDirectory;
            probe.StartTest();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach(Channel ch in ChannelList)
            {
                ch.Close();
            }
        }
    }


}
