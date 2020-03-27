using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace PasswordSec {
    public partial class Main : Window {
        public string ENCRYPT_KEY = "sbdrfqr2wqerf2eqy80rtehfuqdbfgeh";
        public static Main instance;
        public DirectoryInfo baseDirectory = Directory.CreateDirectory("C:\\passsave");
        public List<EncryptInfo> infolist = new List<EncryptInfo>();
        public bool editflag = false;

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
            foreach (FileInfo fileInfo in this.baseDirectory.GetFiles()) {
                if (fileInfo.Extension.Equals(".encrypteduap")) {
                    FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    byte[] b = new byte[fileStream.Length];
                    while (fileStream.Read(b, 0, (int)fileStream.Length) != 0) ;
                    string json = AesUtil.AesDecrypt(Encoding.UTF8.GetString(b), ENCRYPT_KEY);

                    EncryptInfo info = this.GetInfo(json, true, fileInfo.FullName);
                    if (info.getAppName().Contains("无法反序列化Json")) {
                        MessageBox.Show(info.getAppName(), "无法反序列化Json");
                        continue;
                    }

                    this.infolist.Add(info);
                    fileStream.Flush();
                    fileStream.Dispose();
                    fileStream.Close();
                    
                } else if (fileInfo.Extension.Equals(".uap")) {
                    FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    byte[] b = new byte[fileStream.Length];
                    while (fileStream.Read(b, 0, (int) fileStream.Length) != 0);

                    EncryptInfo info = this.GetInfo(Encoding.UTF8.GetString(b), false, fileInfo.FullName);
                    if (info.getAppName().Contains("无法反序列化Json"))
                    {
                        MessageBox.Show(info.getAppName(), "无法反序列化Json");
                        continue;
                    }

                    this.infolist.Add(info);
                    fileStream.Flush();
                    fileStream.Dispose();
                    fileStream.Close();
                }
                else
                {
                    MessageBox.Show("给爷爪巴");
                }
            }
            this.infolist.ForEach((info) => applist.Items.Add(info.getAppName()));
        }

        public EncryptInfo GetInfo(string json, bool encrypt, string filename) {
            try
            {
                RootObject obj = JsonConvert.DeserializeObject<RootObject>(json);
                Dictionary<string, string> extraInfo1 = new Dictionary<string, string>();
                string username = obj.username;
                string password = obj.password;
                string email = obj.email;
                string appname = obj.appname;
                if (obj.extraInfo != null && obj.extraInfo.Count != 0)
                {
                    obj.extraInfo.ForEach((i) => extraInfo1.Add(i.infokey, i.infovalue));
                }


                return new EncryptInfo(appname, username, password, email, extraInfo1, encrypt, filename);
            }
            catch (JsonException e)
            {
                return new EncryptInfo("无法反序列化Json 原因\n" + e.StackTrace, null, null, null, null, false, null);
            }
        }

        private void applist_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            foreach (var info in infolist)
            {
                if (info.getAppName().Equals(applist.SelectedItem))
                {
                    usr.Text = info.getUsername();
                    pas.Text = info.getPassword();
                    eml.Text = info.getEmail();
                    MessageBox.Show(info.isEncrypt().ToString());
                    enc.IsChecked = info.isEncrypt();
                    
                    sublist.Items.Clear();
                    foreach (var k in info.getExraInfo().Keys)
                    {
                        sublist.Items.Add(k);
                    }
                }
            }
            UpdateEdit(false);

            eik.Text = "";
            eiv.Text = "";
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

            UpdateEdit(false);
        }

        //false = 禁用编辑 true = 启用
        public void UpdateEdit(bool flag)
        {
            editflag = flag;

            if (applist.SelectedItem == null) {
                usr.IsEnabled = false;
                eik.IsEnabled = false;
                eiv.IsEnabled = false;
                pas.IsEnabled = false;
                eml.IsEnabled = false;
                enc.IsEnabled = false;
                editflag = false;
                editbtn.Content = "启用编辑";
                return;
            }


            if (flag)
            {
                usr.IsEnabled = true;
                eik.IsEnabled = true;
                eiv.IsEnabled = true;
                pas.IsEnabled = true;
                eml.IsEnabled = true;
                enc.IsEnabled = true;
                editbtn.Content = "禁用编辑";
            }
            else
            {
                usr.IsEnabled = false;
                eik.IsEnabled = false;
                eiv.IsEnabled = false;
                pas.IsEnabled = false;
                eml.IsEnabled = false;
                enc.IsEnabled = false;
                editbtn.Content = "启用编辑";
            }

            if (sublist.SelectedItem == null) {
                eik.IsEnabled = false;
                eiv.IsEnabled = false;
            }

        }

        public int getIndexFromList(EncryptInfo info)
        {
            for (int count = infolist.Count - 1; count != 0; count--)
            {
                if (infolist[count].getPath().Equals(info.getPath()))
                {
                    return count;
                }
            }

            return -1;
        }

        public EncryptInfo getInfoByAppName(string name)
        {
            foreach(var info in infolist) 
            {
                if (info.getAppName().Equals(name))
                {
                    return info;
                }
            }
             throw new ArgumentException("真你妈玄学草");
        }

        public string getKeyByIndex(int i, EncryptInfo info)
        {
            int count = 0;
            foreach (var kvp in info.getExraInfo())
            {
                if (count == i)
                {
                    return kvp.Key;
                }

                count++;
            }
            throw new NullReferenceException("我真的佛了");
        }

        private void AddProfile(object sender, RoutedEventArgs e) {
            new NewProfile().Show();
        }

        private void DeleateExtraInfo(object sender, RoutedEventArgs e) {

        }

        private void SaveExtraInfo(object sender, RoutedEventArgs e) {

        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (applist.SelectedItem == null) { return; }
            if(pas.Text.Length == 0 || usr.Text.Length == 0 || eml.Text.Length == 0) { return; }
            EncryptInfo info = getInfoByAppName(applist.SelectedItem as string);
            info.setPassword(pas.Text);
            info.setUsername(usr.Text);
            info.setEmail(eml.Text);
            info.setEdited();
            info.setEncrypt((bool) enc.IsChecked);
            MessageBox.Show(info.isEncrypt().ToString());
            infolist[getIndexFromList(info)] = info;
        }

        private void Lock(object sender, RoutedEventArgs e) {
            new MainWindow().Show();
            this.Close();
        }

        //Done
        private void SaveAndReload(object sender, RoutedEventArgs e) {
            foreach (var info in infolist)
            {
                if (info.isEdited())
                {
                    string path = info.getPath();
                    if (info.getPath().Contains(".encrypteduap") && !info.isEncrypt()) {
                        path = path.Replace(".encrypteduap", ".uap");
                    } else if (info.getPath().Contains(".uap") && info.isEncrypt()) {
                        path = path.Replace(".uap", ".encrypteduap");
                    }

                    RootObject obj = new RootObject();
                    obj.username = info.getUsername();
                    obj.appname = info.getAppName();
                    obj.email = info.getEmail();
                    obj.password = info.getPassword();
                    List<ExtraInfo> eilist = new List<ExtraInfo>();

                    foreach (var kvp in info.getExraInfo())
                    {
                        var ei = new ExtraInfo();
                        ei.infovalue = kvp.Value;
                        ei.infokey = kvp.Key;
                        eilist.Add(ei);
                    }

                    eilist.ForEach((i) => MessageBox.Show(i.infovalue, i.infokey));

                    FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write);
                    byte[] bytes = info.isEncrypt() ? Encoding.UTF8.GetBytes(AesUtil.AesEncrypt(JsonConvert.SerializeObject(obj), ENCRYPT_KEY)) : Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));

                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush();
                    fs.Dispose();
                    fs.Close();
                }
            }
            Loaduap();
        }

        private void AddExtraInfo(object sender, RoutedEventArgs e) {

        }

        private void Deleate(object sender, RoutedEventArgs e) {

        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            if (editflag) { UpdateEdit(false); } else { UpdateEdit(true); }
        }

        private void CopyPassword(object sender, RoutedEventArgs e) {
            Clipboard.SetDataObject(pas.Text);
        }

        private void CopyUsername(object sender, RoutedEventArgs e) {
            Clipboard.SetDataObject(usr.Text);
        }
    }


    //Utils
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

        public static byte[] getByteArrayFromHexString(string hexString)
        {
            hexString = hexString.Replace(" ", "").Replace("\0", string.Empty);
            if (hexString.Length % 2 != 0) { 
                MessageBox.Show("字符串无效，可能是非加密文件？", "解密失败");
                throw new NullReferenceException("字符串无效或者为空");
            }
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

    public class EncryptInfo
    {
        private string appname;
        private string username;
        private string password;
        private string email;
        private bool encrypt;
        private string fileName;
        private bool edited = false;
        
        private Dictionary<string, string> extraInfo;

        public EncryptInfo(string appname, string username, string password, string email, Dictionary<string, string> extraInfo, bool encrypt, string filename) {
            this.password = password;
            this.username = username;
            this.email = email;
            this.extraInfo = extraInfo;
            this.appname = appname;
            this.encrypt = encrypt;
            this.fileName = filename;
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

        public void setUsername(string name)
        {
            this.username = name;
        }

        public void setPassword(string pass)
        {
            this.password = pass;
        }

        public void setEmail(string eml)
        {
            this.email = eml;
        }

        public void setEi(Dictionary<string, string> ei)
        {
            this.extraInfo = ei;
        }

        public bool isEncrypt()
        {
            return encrypt;
        }

        public void setEncrypt(bool en)
        {
            this.encrypt = en;
        }

        public string getPath()
        {
            return this.fileName;
        }

        public void setEdited()
        {
            this.edited = true;
        }

        public bool isEdited()
        {
            return this.edited;
        }
     }
}
