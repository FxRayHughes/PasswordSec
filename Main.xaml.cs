using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace PasswordSec {
    public partial class Main : Window {
        public string ENCRYPT_KEY = "sbdrfqr2wqerf2eqy80rtehfuqdbfgeh";
        public static Main instance;
        public DirectoryInfo baseDirectory = Directory.CreateDirectory("C:\\passsave");
        public List<EncryptInfo> infolist = new List<EncryptInfo>();

        public Main(string decryptkey)
        {
            //this.ENCRYPT_KEY = decryptkey;
            InitializeComponent();
            instance = this;
            Loaduap();
        }

        public void Loaduap()
        {
            this.sublist.Items.Clear();
            this.applist.Items.Clear();
            this.infolist.Clear();
            if (!this.baseDirectory.Exists)
                this.baseDirectory.Create();
            foreach (FileInfo fileInfo in new List<FileInfo>((IEnumerable<FileInfo>)this.baseDirectory.GetFiles())) {
                if (fileInfo.Extension.Equals(".encrypteduap")) {
                    FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    byte[] b = new byte[fileStream.Length];
                    while (fileStream.Read(b, 0, (int)fileStream.Length) != 0) ;
                    string json = AesUtil.AesDecrypt(Encoding.UTF8.GetString(b), ENCRYPT_KEY);

                    this.infolist.Add(this.GetInfo(json , true));
                    fileStream.Flush();
                    fileStream.Dispose();
                    fileStream.Close();
                    
                } else if (fileInfo.Extension.Equals(".uap")) {
                    FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    byte[] b = new byte[fileStream.Length];
                    while (fileStream.Read(b, 0, (int) fileStream.Length) != 0) ;

                    this.infolist.Add(this.GetInfo(Encoding.UTF8.GetString(b), false));
                    fileStream.Flush();
                    fileStream.Dispose();
                    fileStream.Close();
                }
            }
            foreach (EncryptInfo encryptInfo in this.infolist)
                this.applist.Items.Add((object)encryptInfo.getAppName());
        }

        public EncryptInfo GetInfo(string json, bool encrypt) {
            try
            {
                RootObject obj = JsonConvert.DeserializeObject<RootObject>(json);
                Dictionary<string, string> extraInfo1 = new Dictionary<string, string>();
                string username = obj.username;
                string password = obj.password;
                string email = obj.email;
                string appname = obj.appname;
                foreach (ExtraInfo extraInfo2 in (obj.extraInfo))
                {
                    extraInfo1.Add(extraInfo2.infokey, extraInfo2.infovalue);

                }
                return new EncryptInfo(appname, username, password, email, extraInfo1, encrypt);
            }
            catch (JsonException e)
            {
                return new EncryptInfo("无法反序列化Json 原因\n" + e.StackTrace, null, null, null, null, false);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private void applist_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            foreach (var info in infolist)
            {
                if (info.getAppName().Equals(applist.SelectedItem))
                {
                    usr.Text = info.getUsername();
                    pas.Text = info.getPassword();
                    eml.Text = info.getEmail();
                    
                    sublist.Items.Clear();
                    foreach (var k in info.getExraInfo().Keys)
                    {
                        sublist.Items.Add(k);
                    }
                }
            }

            eik.Text = "";
            eiv.Text = "";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            Clipboard.SetDataObject(usr.Text);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            Clipboard.SetDataObject(pas.Text);
        }

        private void sublist_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (sublist.Items.Count == 0) { return; }
            if (sublist.SelectedItem == null) { return; }

            foreach (var info in infolist)
            {
                if (info.getAppName().Equals(applist.SelectedItem))
                {
                    eik.Text = sublist.SelectedItem as string;
                    eiv.Text = info.getExi(sublist.SelectedItem as string);
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) {
            //TODO 实在是想不出来怎么写了，先这样
        }

        private void Button_Click_4(object sender, RoutedEventArgs e) {
            Loaduap();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            new NewProfile().Show();
        }
    }

    public class AesUtil
    {
        public static string AesDecrypt(string str, string key) {
            if (string.IsNullOrEmpty(str))
                return (string)null;
            byte[] arrayFromHexString = getByteArrayFromHexString(str);
            RijndaelManaged rijndaelManaged = new RijndaelManaged();
            rijndaelManaged.Key = Encoding.UTF8.GetBytes(key);
            rijndaelManaged.Mode = CipherMode.ECB;
            rijndaelManaged.Padding = PaddingMode.PKCS7;
            return Encoding.UTF8.GetString(rijndaelManaged.CreateDecryptor().TransformFinalBlock(arrayFromHexString, 0, arrayFromHexString.Length));
        }

        public static string AesEncrypt(string str, string key) {
            if (string.IsNullOrEmpty(str))
                return (string)null;
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            RijndaelManaged rijndaelManaged = new RijndaelManaged();
            rijndaelManaged.Key = Encoding.UTF8.GetBytes(key);
            rijndaelManaged.Mode = CipherMode.ECB;
            rijndaelManaged.Padding = PaddingMode.PKCS7;
            return getHexStringFromByteArray(rijndaelManaged.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length));
        }

        public static string getHexStringFromByteArray(byte[] b) {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte num in b)
                stringBuilder.Append(Convert.ToString(num, 16).PadLeft(2, '0'));
            return stringBuilder.ToString().ToUpper();
        }

        public static byte[] getByteArrayFromHexString(string hexString) {
            hexString = hexString.Replace(" ", "").Replace("\0", string.Empty);
            if (hexString.Length % 2 != 0)
                throw new ArgumentException("参数长度不正确");
            byte[] numArray = new byte[hexString.Length / 2];
            for (int index = 0; index < numArray.Length; ++index) {
                string str = hexString.Substring(index * 2, 2);
                numArray[index] = Convert.ToByte(str, 16);
            }
            return numArray;
        }
    }

    public class RootObject {
        public string appname { get; set; }

        public string username { get; set; }

        public string password { get; set; }

        public string email { get; set; }

        public List<ExtraInfo> extraInfo { get; set; }
    }

    public class ExtraInfo {
        public string infokey { get; set; }

        public string infovalue { get; set; }
    }

    public class EncryptInfo {
        private string appname;
        private string username;
        private string password;
        private string email;
        private bool encrypt;
        private Dictionary<string, string> extraInfo;

        public EncryptInfo(string appname, string username, string password, string email, Dictionary<string, string> extraInfo, bool encrypt) {
            this.password = password;
            this.username = username;
            this.email = email;
            this.extraInfo = extraInfo;
            this.appname = appname;
            this.encrypt = encrypt;
        }

        public string getAppName() {
            return this.appname;
        }

        public string getUsername() {
            return this.username;
        }

        public string getPassword() {
            return this.password;
        }

        public string getEmail() {
            return this.email;
        }

        public Dictionary<string, string> getExraInfo() {
            return this.extraInfo;
        }

        public string getExi(string key) {
            foreach (KeyValuePair<string, string> keyValuePair in this.extraInfo) {
                if (keyValuePair.Key.Equals(key))
                    return keyValuePair.Value;
            }
            return "";
        }
    }
}
