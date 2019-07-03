using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

namespace _03._07NPExam
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool _alive = false;
        private UdpClient _client;
        private const int _localport = 8001;
        private const int _remoteport = 8001;
        private const int _ttl = 20;
        private const string _host = "224.1.1.1";
        private IPAddress _groupAddress;

        private string _userName;
        public MainWindow()
        {
            InitializeComponent();
            loginButton.IsEnabled = true;
            exitButton.IsEnabled = false;
            sendButton.IsEnabled = false;
            chatTextBox.IsReadOnly = true;
            _groupAddress = IPAddress.Parse(_host);
        }


        private void SendButtonClick(object sender, RoutedEventArgs e)
        {
             try
            {
                string message = String.Format("{0}: {1}", _userName, messageTextBox.Text);
                byte[] data = Encoding.Unicode.GetBytes(message);
                _client.Send(data, data.Length, _host, _remoteport);
                messageTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            _userName = userNameTextBox.Text;
            userNameTextBox.IsReadOnly = true;

            try
            {
                _client = new UdpClient(_localport);
                _client.JoinMulticastGroup(_groupAddress, _ttl);

                Task receiveTask = new Task(ReceiveMessages);
                receiveTask.Start();

                string message = _userName + " has joined the chat";
                byte[] data = Encoding.Unicode.GetBytes(message);
                _client.Send(data, data.Length, _host, _remoteport);

                loginButton.IsEnabled = false;
                exitButton.IsEnabled = true;
                sendButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ReceiveMessages()
        {
            _alive = true;
            try
            {
                while (_alive)
                {
                    IPEndPoint remoteIp = null;
                    byte[] data = _client.Receive(ref remoteIp);
                    string message = Encoding.Unicode.GetString(data);

                    Dispatcher.BeginInvoke(new ThreadStart(() =>
                    {
                        string time = DateTime.Now.ToShortTimeString();
                        chatTextBox.Text = time + " " + message + "\r\n" + chatTextBox.Text;
                    }));
                }
            }
            catch (ObjectDisposedException)
            {
                if (!_alive)
                    return;
                throw;
            }
            catch (Exception ex)
            {
                if (_alive)
                    MessageBox.Show(ex.Message);
            }
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            string message = _userName + " leaves the chat";
            byte[] data = Encoding.Unicode.GetBytes(message);
            _client.Send(data, data.Length, _host, _remoteport);
            _client.DropMulticastGroup(_groupAddress);

            _alive = false;
            _client.Close();

            loginButton.IsEnabled = true;
            exitButton.IsEnabled = false;
            sendButton.IsEnabled = false;
        }

        private void UserNameTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            userNameTextBox.Text = "";
        }

        private void MessageTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            messageTextBox.Text = "";
        }
    }
}
