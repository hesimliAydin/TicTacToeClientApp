using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace TicTacToeClientApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public bool IsMyTurn { get; set; } = false;
        public bool HasWonned { get; set; } = false;

        private const int port = 27001;
        private static readonly Socket ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public string MySymbol { get; set; }
        public char CurrentPlayer { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Player_Name.Text == string.Empty || string.IsNullOrWhiteSpace(Player_Name.Text))
                MessageBox.Show("Enter player Name", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                ConnectToServer();
                RequestLoop();
            }
        }


        private void ConnectToServer()
        {
            int attempts = 0;
            while (!ClientSocket.Connected)
            {
                try
                {
                    ++attempts;
                    ClientSocket.Connect(IPAddress.Parse("192.168.100.155"), port);

                    var name = Player_Name.Text;

                    if (!string.IsNullOrWhiteSpace(name))
                        SendString($"Connected:{name}");
                }
                catch (Exception)
                {
                }
            }

            MessageBox.Show("Connected Server", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];
            Array.Copy(buffer, data, received);

            string text = Encoding.ASCII.GetString(data);
            MySymbol = text;
            CurrentPlayer = text[0];
            this.Title = "Player : " + text;
            this.player.Text = this.Title;
            if (MySymbol == "X")
                IsMyTurn = true;
            else if (MySymbol == "O")
                IsMyTurn = false;
        }


        private void RequestLoop()
        {
            var receiver = Task.Run(() =>
            {
                while (true)
                {
                    EnabledAllButtons(IsMyTurn);
                    ReceiveResponse();
                    if (HasWonned && !IsMyTurn)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"{Player_Name.Text}  Won", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            Exit_Game();
                        });
                        break;
                    }
                    else if (HasWonned && IsMyTurn)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"{Player_Name.Text} Lose", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            Exit_Game();
                        });
                        break;
                    }

                }
            });
        }

        private static void Exit_Game()
        {
            ClientSocket.Close();
            ClientSocket.Dispose();
            Application.Current.Shutdown();
        }

        private void ReceiveResponse()
        {
            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];

            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);
            IntegrateToView(text);
        }


        private void IntegrateToView(string text)
        {

            App.Current.Dispatcher.Invoke(() =>
            {
                var data = text.Split('\n');
                var row1 = data[0].Split('\t');
                var row2 = data[1].Split('\t');
                var row3 = data[2].Split('\t');

                b1.Content = row1[0];
                b2.Content = row1[1];
                b3.Content = row1[2];

                b4.Content = row2[0];
                b5.Content = row2[1];
                b6.Content = row2[2];

                b7.Content = row3[0];
                b8.Content = row3[1];
                b9.Content = row3[2];

                int xCount = 0;
                int oCount = 0;

                for (int i = 0; i < row1.Length; i++)
                {
                    if (row1[i] == "X") xCount++;
                    else if (row1[i] == "O") oCount++;
                }
                for (int i = 0; i < row2.Length; i++)
                {
                    if (row2[i] == "X") xCount++;
                    else if (row2[i] == "O") oCount++;
                }
                for (int i = 0; i < row3.Length; i++)
                {
                    if (row3[i] == "X") xCount++;
                    else if (row3[i] == "O") oCount++;
                }

                if (xCount % 2 == 1 && oCount % 2 == 0 || xCount % 2 == 0 && oCount % 2 == 1)
                {
                    if (CurrentPlayer == 'X')
                        IsMyTurn = false;
                    else
                        IsMyTurn = true;
                }
                else
                {
                    if (CurrentPlayer == 'X')
                        IsMyTurn = true;
                    else
                        IsMyTurn = false;
                }

                //EnabledAllButtons(true);

                if ((row1[0] == "X" && row1[1] == "X" && row1[2] == "X") || (row2[0] == "X" && row2[1] == "X" && row2[2] == "X") || (row3[0] == "X" && row3[1] == "X" && row3[2] == "X")
                    || (row1[0] == "X" && row2[1] == "X" && row3[2] == "X") || (row3[0] == "X" && row2[1] == "X" && row1[3] == "X")
                    || (row1[0] == "X" && row2[0] == "X" && row3[0] == "X") || (row1[1] == "X" && row2[1] == "X" && row3[1] == "X") || (row1[2] == "X" && row2[2] == "X" && row3[2] == "X")
                    || (row1[0] == "O" && row1[1] == "O" && row1[2] == "O") || (row2[0] == "O" && row2[1] == "O" && row2[2] == "O") || (row3[0] == "O" && row3[1] == "O" && row3[2] == "O")
                    || (row1[0] == "O" && row2[1] == "O" && row3[2] == "O") || (row3[0] == "O" && row2[1] == "O" && row1[3] == "O")
                    || (row1[0] == "O" && row2[0] == "O" && row3[0] == "O") || (row1[1] == "O" && row2[1] == "O" && row3[1] == "O") || (row1[2] == "O" && row2[2] == "O" && row3[2] == "O"))
                {
                    HasWonned = true;
                }
            });
        }


        private void b1_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    var bt = sender as Button;
                    string request = bt.Content.ToString() + player.Text.Split(' ')[2];
                    SendString(request);
                });
            });
        }

        public void EnabledAllButtons(bool enabled)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (var item in myWrap.Children)
                    if (item is Button bt)
                        bt.IsEnabled = enabled;

            });
        }

        private void SendString(string request)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(request);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }
    }
}
