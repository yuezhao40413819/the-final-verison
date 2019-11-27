using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EustonLeisureMessageFilteringDesktop
{
    public class MainWindowViewModel : ViewModelBase
    {

        private MessageService _currentMessageService;
        private Dictionary<string, string> _abbreviationDic;
        private string _inputMessageHeader;
        private string _originalMessageBody;
        private int _selectedType;
        private string _messageSender;
        private string _subject;
        private string _messageContent;
        private bool _displaySubject;
        private bool _displayItems;
        private int _maxCount;
        private int _count;
        private Dictionary<string, List<string>> _additionalItems;

        public string MessageHeader
        {
            get { return _inputMessageHeader; }
            set { Set(ref _inputMessageHeader, value); }
        }

        public string OriginalMessageBody
        {
            get { return _originalMessageBody; }
            set { Set(ref _originalMessageBody, value); }
        }

        public int SelectedType
        {
            get { return _selectedType; }
            set { Set(ref _selectedType, value); }
        }

        public string MessageSender
        {
            get { return _messageSender; }
            set { Set(ref _messageSender, value); }
        }

        public string MessageSubject
        {
            get { return _subject; }
            set { Set(ref _subject, value); }
        }

        public string MessageContent
        {
            get { return _messageContent; }
            set { Set(ref _messageContent, value); }
        }

        public bool DisplayItems
        {
            get { return _displayItems; }
            set { Set(ref _displayItems, value); }
        }

        public bool DisplaySubject
        {
            get { return _displaySubject; }
            set { Set(ref _displaySubject, value); }
        }

        public int MaxCount
        {
            get { return _maxCount; }
            set { Set(ref _maxCount, value); }
        }

        public int Count
        {
            get { return _count; }
            set { Set(ref _count, value); }
        }

        public Dictionary<string, List<string>> AdditionalItems
        {
            get { return _additionalItems; }
            set { Set(ref _additionalItems, value); }
        }

        public List<string> MessageTypeList { get; set; }

        public ICommand ExportJsonCommand { get; set; }


        public MainWindowViewModel()
        {
            Initialize();
            ExportJsonCommand = new RelayCommand(ExportJsonExecute);
            this.PropertyChanged += MainWindowViewModel_PropertyChanged;
        }

        private void MainWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MessageHeader):
                    OnMessageHeaderChanged();
                    break;
                case nameof(OriginalMessageBody):
                    OnInputMessageBodyChanged();
                    break;
                case nameof(SelectedType):
                    OnMessageTypeChanged();
                    break;
                default:
                    break;
            }
        }

        private void Initialize()
        {
            LoadMessageType();
            LoadAbbreviationDic();
        }

        private void LoadMessageType()
        {
            MessageTypeList = new List<string> { "Unknown", "SMS", "Email", "Tweet" };
        }

        private void LoadAbbreviationDic()
        {
            _abbreviationDic = new Dictionary<string, string>();
            string csvFilePath = System.Configuration.ConfigurationManager.AppSettings["AbbreviationFile"];
            if (!File.Exists(csvFilePath))
                return;
            using (StreamReader strReader = new StreamReader(csvFilePath, Encoding.Default))
            {
                string line;
                while ((line = strReader.ReadLine()) != null)
                {
                    string[] array = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (array.Length != 2 || string.IsNullOrEmpty(array[0]))
                        continue;
                    _abbreviationDic[array[0]] = array[1];
                }
            }

        }

        private void OnMessageHeaderChanged()
        {
            string regStr = @"^[EST]\d{9}$";
            bool match = Regex.IsMatch(MessageHeader, regStr);
            if (!match)
            {
                SelectedType = 0;
                return;
            }
            switch (MessageHeader[0])
            {
                case 'E':
                    SelectedType = MessageTypeList.IndexOf("Email");
                    break;
                case 'S':
                    SelectedType = MessageTypeList.IndexOf("SMS");
                    break;
                case 'T':
                    SelectedType = MessageTypeList.IndexOf("Tweet");
                    break;
                default:
                    SelectedType = 0;
                    break;
            }
        }

        private void OnMessageTypeChanged()
        {
            DisplaySubject = SelectedType == MessageTypeList.IndexOf("Email") ? true : false;
            DisplayItems = SelectedType == MessageTypeList.IndexOf("Unknown") ? false : true;
            _currentMessageService = MessageServiceFactory.CreateMessageService(SelectedType);
        }

        private void OnInputMessageBodyChanged()
        {
            var service = MessageServiceFactory.CreateMessageService(SelectedType);
            service.ProcessMessage(_abbreviationDic, OriginalMessageBody.TrimStart(new char[] { ' ' }));

            MessageSender = service.Sender;
            MessageContent = service.MessageBody;
            Count = service.CharacterCount;
            MaxCount = service.MaxCharacterCount;
            MessageSubject = service.GetSubject();
            AdditionalItems = service.ExtraList;
        }

        private void ExportJsonExecute()
        {
            if (_currentMessageService == null)
            {
                System.Windows.MessageBox.Show("No valid message to export!");
                return;
            }
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Export to Json File";
            saveDialog.Filter = "所有文件(*.*)|*.*";
            var result = saveDialog.ShowDialog(System.Windows.Application.Current.MainWindow);
            if (result != true)
                return;

            try
            {

                
                string jsonStr = JsonConvert.SerializeObject(_currentMessageService);
                using (FileStream fs = new FileStream(saveDialog.FileName, FileMode.OpenOrCreate))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(jsonStr);
                        sw.Dispose();
                        fs.Dispose();
                    }
                }
 
                System.Windows.MessageBox.Show("Export to json file succeed!");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
    }
}
