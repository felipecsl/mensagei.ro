using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Net;
using System.IO;
using Br.Com.Quavio.Tools.Web.Net;
using Newtonsoft.Json;
using Com.CloudTalk.Controller;
using System.ComponentModel;
using System.Web;
using Com.CloudTalk.Controller.Events;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string clientName;
        private string clientEmail;
        private const string joinRoomUrl = "http://www.quavio.com.br/cloudtalk/Home.aspx/JoinRoom";
        private const string leaveRoomUrl = "http://www.quavio.com.br/cloudtalk/Home.aspx/LeaveRoom";
        private const string sendMsgUrl = "http://www.quavio.com.br/cloudtalk/Home.aspx/SendMessage";
        private const string getMessagesUrl = "http://www.quavio.com.br/cloudtalk/Home.aspx/Events";
        private const string roomName = "PoaClinicas Chat Room";

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }

        internal class CloudTalkResult
        {
            public CloudTalkResult() { }

            public int Code;
            public string Message;
            public string EventObject;
            public string Sender;
            public DateTime Timestamp;
            public EventType Type;
        }

        internal class CloudTalkMessage
        {
            public string Message;
        }

        //Stop flashing. The system restores the window to its original state.
        public const UInt32 FLASHW_STOP = 0;
        //Flash the window caption.
        public const UInt32 FLASHW_CAPTION = 1;
        //Flash the taskbar button.
        public const UInt32 FLASHW_TRAY = 2;
        //Flash both the window caption and taskbar button.
        //This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
        public const UInt32 FLASHW_ALL = 3;
        //Flash continuously, until the FLASHW_STOP flag is set.
        public const UInt32 FLASHW_TIMER = 4;
        //Flash continuously until the window comes to the foreground.
        public const UInt32 FLASHW_TIMERNOFG = 12; 

        [DllImport("user32.dll")]
        public static extern bool FlashWindowEx(ref FLASHWINFO pfwi);
        
        public MainWindow()
        {
            InitializeComponent();

            Console.WriteLine("Starting up...");
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                SendMessage(txtInput.Text);
            }

            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                txtInput.AppendText(Environment.NewLine);
                txtInput.CaretIndex = txtInput.Text.Length;
            }
        }

        private void SendMessage(string msg)
        {
            AppendChatLog(clientName, msg);

            string parms = String.Format("Message={0}&ClientEmail={1}&RoomName={2}", msg, clientEmail, roomName);
            string result = NetFunctions.HttpPost(sendMsgUrl, parms);

            Console.WriteLine("Send message result: " + result);
        }

        private void AppendChatLog(
            string sender,
            string msg)
        {
            txtConversation.AppendText(sender + " diz: " + msg + Environment.NewLine);
            txtInput.Clear();
            HighlightText(txtConversation.Document.ContentStart, sender);
            txtConversation.ScrollToEnd();
        }

        private void btnEntrar_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtNome.Text) ||
                String.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Por favor, preencha os campos Nome e E-mail");
                return;
            }

            string parms = String.Format("Name={0}&Email={1}&RoomName={2}", txtNome.Text, txtEmail.Text, roomName);
            string result = NetFunctions.HttpPost(joinRoomUrl, parms);
            var resultObj = JsonConvert.DeserializeObject<CloudTalkResult>(result);

            if (resultObj.Code != 1)
            {
                MessageBox.Show("Falha ao entrar na sala. Detalhes do erro: \n" + resultObj.Message);
                return;
            }

            // Hide initial controls
            loginGrid.Visibility = System.Windows.Visibility.Hidden;
            txtInput.Visibility = System.Windows.Visibility.Visible;
            clientName = txtNome.Text;
            clientEmail = txtEmail.Text;

            AppendChatLog(clientName, "Entrou na sala.");

            GetMessages();
        }

        private void GetMessages()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                var resultObjList = JsonConvert.DeserializeObject<CloudTalkResult[]>(e.Result.ToString());
                foreach (var obj in resultObjList)
                {
                    AppendChatLog(obj.Sender, JsonConvert.DeserializeObject<CloudTalkMessage>(obj.EventObject).Message);
                }

                if (resultObjList.Length > 0)
                {
                    FlashWindow();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao obter lista de mensagens: " + ex.Message);
            }

            GetMessages();
        }

        private void FlashWindow()
        {
            FLASHWINFO fInfo = new FLASHWINFO();
            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = new WindowInteropHelper(this).Handle;
            fInfo.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
            fInfo.uCount = UInt32.MaxValue;
            fInfo.dwTimeout = 0;
            FlashWindowEx(ref fInfo);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = NetFunctions.HttpGet(getMessagesUrl, "Email=" + HttpUtility.UrlEncode(clientEmail));
        }

        private void HighlightText(TextPointer position, string word)
        {
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);

                    // Find the starting index of any substring that matches "word".
                    int indexInRun = textRun.IndexOf(word);
                    if (indexInRun >= 0)
                    {
                        TextPointer start = position.GetPositionAtOffset(indexInRun);
                        TextPointer end = start.GetPositionAtOffset(word.Length);
                        new TextRange(start, end).ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                    }
                }

                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        ~MainWindow()
        {
            string parms = String.Format("Email={0}&RoomName={1}", clientEmail, roomName);
            string result = NetFunctions.HttpPost(leaveRoomUrl, parms);

            Console.WriteLine("Leave room result: " + result);
        }
    }
}
