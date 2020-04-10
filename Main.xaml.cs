using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace PasswordSec {
    public partial class Main : Window {
        public string ENCRYPT_KEY = "sbdrfqr2wqerf2eqy80rtehfuqdbfgeh";
        public static Main instance;
        public DirectoryInfo baseDirectory = Directory.CreateDirectory("C:\\passsave");
        public List<EncryptInfo> infolist = new List<EncryptInfo>();
        public bool editflag = false;
        public bool addflag = false;

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
            foreach (FileInfo fileInfo in this.baseDirectory.GetFiles())
            {
                if (fileInfo.Extension.Equals(".encrypteduap"))
                {
                    FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    byte[] b = new byte[fileStream.Length];
                    while (fileStream.Read(b, 0, (int) fileStream.Length) != 0) ;
                    string json = AesUtil.AesDecrypt(Encoding.UTF8.GetString(b), ENCRYPT_KEY);

                    EncryptInfo info = this.GetInfo(json, true, fileInfo.FullName);
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
                else if (fileInfo.Extension.Equals(".uap"))
                {
                    FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    byte[] b = new byte[fileStream.Length];
                    while (fileStream.Read(b, 0, (int) fileStream.Length) != 0) ;

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
            }

            this.infolist.ForEach((info) => { applist.Items.Add(info.getAppName()); });
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
                    enc.IsChecked = info.isEncrypt();
                    
                    sublist.Items.Clear();
                    foreach (var k in info.getExraInfo().Keys)
                    {
                        sublist.Items.Add(k);
                    }

                    break;
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
                eiv.IsEnabled = true;
                pas.IsEnabled = true;
                eml.IsEnabled = true;
                enc.IsEnabled = true;
                editbtn.Content = "禁用编辑";
            }
            else
            {
                usr.IsEnabled = false;
                eiv.IsEnabled = false;
                pas.IsEnabled = false;
                eml.IsEnabled = false;
                enc.IsEnabled = false;
                editbtn.Content = "启用编辑";
            }

            if (sublist.SelectedItem == null) {
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
            if (applist.SelectedItem == null) { return; }
            if (sublist.SelectedItem == null) { return; }
            EncryptInfo info = getInfoByAppName(applist.SelectedItem as string);
            info.getExraInfo().Remove(sublist.SelectedItem as string);
        }

        private void SaveExtraInfo(object sender, RoutedEventArgs e) {
            if (applist.SelectedItem == null) { return; }
            if (sublist.SelectedItem == null) { return; }

            EncryptInfo info = getInfoByAppName(applist.SelectedItem as string);
            info.getExraInfo()[sublist.SelectedItem as string] = eiv.Text;
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
            infolist[getIndexFromList(info)] = info;
        }

        private void Lock(object sender, RoutedEventArgs e) {
            new MainWindow().Show();
            this.Close();
        }

        private void SaveAndReload(object sender, RoutedEventArgs e) {
            foreach (var info in infolist)
            {
                if (info.isEdited())
                {
                    string path = info.getPath();
                    string delpath = "";
                    if (info.getPath().Contains(".encrypteduap") && !info.isEncrypt()) {
                        path = path.Replace(".encrypteduap", ".uap");
                        delpath = info.getPath();
                    } else if (info.getPath().Contains(".uap") && info.isEncrypt()) {
                        delpath = info.getPath();
                        path = path.Replace(".uap", ".encrypteduap");
                    }

                    RootObject obj = new RootObject();
                    obj.username = info.getUsername();
                    obj.appname = info.getAppName();
                    obj.email = info.getEmail();
                    obj.password = info.getPassword();
                    List<ExtraInfo> eilist = new List<ExtraInfo>();

                    bool delflag = false;
                    if (delpath != String.Empty)
                    {
                        foreach (var item in baseDirectory.GetFileSystemInfos())
                        {
                            if (item.FullName == delpath)
                            {
                                item.Delete();
                                delflag = true;
                                break;
                            }
                        }

                        if (!delflag)
                        {
                            MessageBox.Show("文件删除失败 请反馈给开发者", "保存失败");
                            MainWindow.instance.disconnectToServer();
                            return;
                        }
                    }


                    foreach (var kvp in info.getExraInfo())
                    {
                        var ei = new ExtraInfo();
                        ei.infovalue = kvp.Value;
                        ei.infokey = kvp.Key;
                        eilist.Add(ei);
                    }

                    obj.extraInfo = eilist;

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
            if (applist.SelectedItem == null) { return; }


            if (addflag)
            {
                if (eik.Text == String.Empty && eiv.Text == string.Empty)
                {
                    addflag = false;
                    eik.IsEnabled = false;
                    eiv.IsEnabled = false;
                    addei.Content = "添加附加信息";
                    return;
                }
                addflag = false;
                eik.IsEnabled = false;
                eiv.IsEnabled = false;
                if (eik.Text != string.Empty)
                {
                    EncryptInfo info = getInfoByAppName(applist.SelectedItem as string);
                    if (info.getExraInfo().ContainsKey(eik.Text))
                    {
                        MessageBox.Show("key不能重复哦！", "添加失败");
                        addflag = true;
                        eik.IsEnabled = true;
                        eiv.IsEnabled = true;
                        return;
                    }
                    info.getExraInfo().Add(eik.Text, eiv.Text);
                    addei.Content = "添加附加信息";

                    sublist.Items.Clear();
                    foreach (var kvp in info.getExraInfo())
                    {
                        sublist.Items.Add(kvp.Key);
                    }
                }
                else
                {
                    addflag = true;
                    eik.IsEnabled = true;
                    eiv.IsEnabled = true;
                    MessageBox.Show("输入东西啊喂！");
                }
            }
            else
            {
                addflag = true;
                eik.IsEnabled = true;
                eiv.IsEnabled = true;
                addei.Content = "再次点击来确认";
            }
        }

        private void Deleate(object sender, RoutedEventArgs e)
        {
            if (applist.SelectedItem == null) { return; }
            var box = MessageBox.Show("确认删除？", "请务必再三确认！", MessageBoxButton.YesNo);
            if (box == MessageBoxResult.Yes)
            {
                EncryptInfo info = getInfoByAppName(applist.SelectedItem as string);
                bool delflag = false;

                foreach (var item in baseDirectory.GetFileSystemInfos()) {
                    if (item.FullName == info.getPath()) {
                        item.Delete();
                        delflag = true;
                        break;
                    }
                }

                if (!delflag)
                {
                    MessageBox.Show("文件删除失败 请反馈给开发者", "删除失败");
                }
            }
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


    public class AesUtil
    {
        public static string AesDecrypt(string str, string key) {
            if (string.IsNullOrEmpty(str)) return null;
            byte[] arrayFromHexString = getByteArrayFromHexString(str);
            RijndaelManaged rijndaelManaged = new RijndaelManaged();
            rijndaelManaged.Key = Encoding.UTF8.GetBytes(key) ;
            rijndaelManaged.Mode = CipherMode.ECB;
            rijndaelManaged.Padding = PaddingMode.PKCS7;
            return Encoding.UTF8.GetString(rijndaelManaged.CreateDecryptor().TransformFinalBlock(arrayFromHexString, 0, arrayFromHexString.Length));
        }

        public static string AesEncrypt(string str, string key) {
            if (string.IsNullOrEmpty(str)) return null;
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

    /// <summary>
    /// 注册表基项静态域
    /// 
    /// 主要包括：
    /// 1.Registry.ClassesRoot     对应于HKEY_CLASSES_ROOT主键
    /// 2.Registry.CurrentUser     对应于HKEY_CURRENT_USER主键
    /// 3.Registry.LocalMachine    对应于 HKEY_LOCAL_MACHINE主键
    /// 4.Registry.User            对应于 HKEY_USER主键
    /// 5.Registry.CurrentConfig   对应于HEKY_CURRENT_CONFIG主键
    /// 6.Registry.DynDa           对应于HKEY_DYN_DATA主键
    /// 7.Registry.PerformanceData 对应于HKEY_PERFORMANCE_DATA主键
    /// 
    /// 版本:1.0
    /// </summary>
    public enum RegDomain {
        /// <summary>
        /// 对应于HKEY_CLASSES_ROOT主键
        /// </summary>
        ClassesRoot = 0,
        /// <summary>
        /// 对应于HKEY_CURRENT_USER主键
        /// </summary>
        CurrentUser = 1,
        /// <summary>
        /// 对应于 HKEY_LOCAL_MACHINE主键
        /// </summary>
        LocalMachine = 2,
        /// <summary>
        /// 对应于 HKEY_USER主键
        /// </summary>
        User = 3,
        /// <summary>
        /// 对应于HEKY_CURRENT_CONFIG主键
        /// </summary>
        CurrentConfig = 4,
        /// <summary>
        /// 对应于HKEY_DYN_DATA主键
        /// </summary>
        DynDa = 5,
        /// <summary>
        /// 对应于HKEY_PERFORMANCE_DATA主键
        /// </summary>
        PerformanceData = 6,
    }

    /// <summary>
    /// 指定在注册表中存储值时所用的数据类型，或标识注册表中某个值的数据类型
    /// 
    /// 主要包括：
    /// 1.RegistryValueKind.Unknown
    /// 2.RegistryValueKind.String
    /// 3.RegistryValueKind.ExpandString
    /// 4.RegistryValueKind.Binary
    /// 5.RegistryValueKind.DWord
    /// 6.RegistryValueKind.MultiString
    /// 7.RegistryValueKind.QWord
    /// 
    /// 版本:1.0
    /// </summary>
    public enum RegValueKind {
        /// <summary>
        /// 指示一个不受支持的注册表数据类型。例如，不支持 Microsoft Win32 API 注册表数据类型 REG_RESOURCE_LIST。使用此值指定
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 指定一个以 Null 结尾的字符串。此值与 Win32 API 注册表数据类型 REG_SZ 等效。
        /// </summary>
        String = 1,
        /// <summary>
        /// 指定一个以 NULL 结尾的字符串，该字符串中包含对环境变量（如 %PATH%，当值被检索时，就会展开）的未展开的引用。
        /// 此值与 Win32 API注册表数据类型 REG_EXPAND_SZ 等效。
        /// </summary>
        ExpandString = 2,
        /// <summary>
        /// 指定任意格式的二进制数据。此值与 Win32 API 注册表数据类型 REG_BINARY 等效。
        /// </summary>
        Binary = 3,
        /// <summary>
        /// 指定一个 32 位二进制数。此值与 Win32 API 注册表数据类型 REG_DWORD 等效。
        /// </summary>
        DWord = 4,
        /// <summary>
        /// 指定一个以 NULL 结尾的字符串数组，以两个空字符结束。此值与 Win32 API 注册表数据类型 REG_MULTI_SZ 等效。
        /// </summary>
        MultiString = 5,
        /// <summary>
        /// 指定一个 64 位二进制数。此值与 Win32 API 注册表数据类型 REG_QWORD 等效。
        /// </summary>
        QWord = 6,
    }

    /// <summary>
    /// 注册表操作类
    /// 
    /// 主要包括以下操作：
    /// 1.创建注册表项
    /// 2.读取注册表项
    /// 3.判断注册表项是否存在
    /// 4.删除注册表项
    /// 5.创建注册表键值
    /// 6.读取注册表键值
    /// 7.判断注册表键值是否存在
    /// 8.删除注册表键值
    /// 
    /// 版本:1.0
    /// </summary>
    public class Register {
        #region 字段定义
        /// <summary>
        /// 注册表项名称
        /// </summary>
        private string _subkey;
        /// <summary>
        /// 注册表基项域
        /// </summary>
        private RegDomain _domain;
        /// <summary>
        /// 注册表键值
        /// </summary>
        private string _regeditkey;
        #endregion

        #region 属性
        /// <summary>
        /// 设置注册表项名称
        /// </summary>
        public string SubKey {
            //get { return _subkey; }
            set { _subkey = value; }
        }

        /// <summary>
        /// 注册表基项域
        /// </summary>
        public RegDomain Domain {
            ///get { return _domain; }
            set { _domain = value; }
        }

        /// <summary>
        /// 注册表键值
        /// </summary>
        public string RegeditKey {
            ///get{return _regeditkey;}
            set { _regeditkey = value; }
        }
        #endregion

        #region 构造函数
        public Register() {
            ///默认注册表项名称
            _subkey = "software\\";
            ///默认注册表基项域
            _domain = RegDomain.LocalMachine;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="subKey">注册表项名称</param>
        /// <param name="regDomain">注册表基项域</param>
        public Register(string subKey, RegDomain regDomain) {
            ///设置注册表项名称
            _subkey = subKey;
            ///设置注册表基项域
            _domain = regDomain;
        }
        #endregion

        #region 公有方法
        #region 创建注册表项
        /// <summary>
        /// 创建注册表项，默认创建在注册表基项 HKEY_LOCAL_MACHINE下面（请先设置SubKey属性）
        /// 虚方法，子类可进行重写
        /// </summary>
        public virtual void CreateSubKey() {
            ///判断注册表项名称是否为空，如果为空，返回false
            if (_subkey == string.Empty || _subkey == null) {
                return;
            }

            ///创建基于注册表基项的节点
            RegistryKey key = GetRegDomain(_domain);

            ///要创建的注册表项的节点
            RegistryKey sKey;
            if (!IsSubKeyExist()) {
                sKey = key.CreateSubKey(_subkey);
            }
            //sKey.Close();
            ///关闭对注册表项的更改
            key.Close();
        }

        /// <summary>
        /// 创建注册表项，默认创建在注册表基项 HKEY_LOCAL_MACHINE下面
        /// 虚方法，子类可进行重写
        /// 例子：如subkey是software\\higame\\，则将创建HKEY_LOCAL_MACHINE\\software\\higame\\注册表项
        /// </summary>
        /// <param name="subKey">注册表项名称</param>
        public virtual void CreateSubKey(string subKey) {
            ///判断注册表项名称是否为空，如果为空，返回false
            if (subKey == string.Empty || subKey == null) {
                return;
            }

            ///创建基于注册表基项的节点
            RegistryKey key = GetRegDomain(_domain);

            ///要创建的注册表项的节点
            RegistryKey sKey;
            if (!IsSubKeyExist(subKey)) {
                sKey = key.CreateSubKey(subKey);
            }
            //sKey.Close();
            ///关闭对注册表项的更改
            key.Close();
        }

        /// <summary>
        /// 创建注册表项，默认创建在注册表基项 HKEY_LOCAL_MACHINE下面
        /// 虚方法，子类可进行重写
        /// </summary>
        /// <param name="regDomain">注册表基项域</param>
        public virtual void CreateSubKey(RegDomain regDomain) {
            ///判断注册表项名称是否为空，如果为空，返回false
            if (_subkey == string.Empty || _subkey == null) {
                return;
            }

            ///创建基于注册表基项的节点
            RegistryKey key = GetRegDomain(regDomain);

            ///要创建的注册表项的节点
            RegistryKey sKey;
            if (!IsSubKeyExist(regDomain)) {
                sKey = key.CreateSubKey(_subkey);
            }
            //sKey.Close();
            ///关闭对注册表项的更改
            key.Close();
        }

        /// <summary>
        /// 创建注册表项（请先设置SubKey属性）
        /// 虚方法，子类可进行重写
        /// 例子：如regDomain是HKEY_LOCAL_MACHINE，subkey是software\\higame\\，则将创建HKEY_LOCAL_MACHINE\\software\\higame\\注册表项
        /// </summary>
        /// <param name="subKey">注册表项名称</param>
        /// <param name="regDomain">注册表基项域</param>
        public virtual void CreateSubKey(string subKey, RegDomain regDomain) {
            ///判断注册表项名称是否为空，如果为空，返回false
            if (subKey == string.Empty || subKey == null) {
                return;
            }

            ///创建基于注册表基项的节点
            RegistryKey key = GetRegDomain(regDomain);

            ///要创建的注册表项的节点
            RegistryKey sKey;
            if (!IsSubKeyExist(subKey, regDomain)) {
                sKey = key.CreateSubKey(subKey);
            }
            //sKey.Close();
            ///关闭对注册表项的更改
            key.Close();
        }
        #endregion

        #region 判断注册表项是否存在
        /// <summary>
        /// 判断注册表项是否存在，默认是在注册表基项HKEY_LOCAL_MACHINE下判断（请先设置SubKey属性）
        /// 虚方法，子类可进行重写
        /// 例子：如果设置了Domain和SubKey属性，则判断Domain\\SubKey，否则默认判断HKEY_LOCAL_MACHINE\\software\\
        /// </summary>
        /// <returns>返回注册表项是否存在，存在返回true，否则返回false</returns>
        public virtual bool IsSubKeyExist() {
            ///判断注册表项名称是否为空，如果为空，返回false
            if (_subkey == string.Empty || _subkey == null) {
                return false;
            }

            ///检索注册表子项
            ///如果sKey为null,说明没有该注册表项不存在，否则存在
            RegistryKey sKey = OpenSubKey(_subkey, _domain);
            if (sKey == null) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 判断注册表项是否存在，默认是在注册表基项HKEY_LOCAL_MACHINE下判断
        /// 虚方法，子类可进行重写
        /// 例子：如subkey是software\\higame\\，则将判断HKEY_LOCAL_MACHINE\\software\\higame\\注册表项是否存在
        /// </summary>
        /// <param name="subKey">注册表项名称</param>
        /// <returns>返回注册表项是否存在，存在返回true，否则返回false</returns>
        public virtual bool IsSubKeyExist(string subKey) {
            ///判断注册表项名称是否为空，如果为空，返回false
            if (subKey == string.Empty || subKey == null) {
                return false;
            }

            ///检索注册表子项
            ///如果sKey为null,说明没有该注册表项不存在，否则存在
            RegistryKey sKey = OpenSubKey(subKey);
            if (sKey == null) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 判断注册表项是否存在
        /// 虚方法，子类可进行重写
        /// 例子：如regDomain是HKEY_CLASSES_ROOT，则将判断HKEY_CLASSES_ROOT\\SubKey注册表项是否存在
        /// </summary>
        /// <param name="regDomain">注册表基项域</param>
        /// <returns>返回注册表项是否存在，存在返回true，否则返回false</returns>
        public virtual bool IsSubKeyExist(RegDomain regDomain) {
            ///判断注册表项名称是否为空，如果为空，返回false
            if (_subkey == string.Empty || _subkey == null) {
                return false;
            }

            ///检索注册表子项
            ///如果sKey为null,说明没有该注册表项不存在，否则存在
            RegistryKey sKey = OpenSubKey(_subkey, regDomain);
            if (sKey == null) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 判断注册表项是否存在（请先设置SubKey属性）
        /// 虚方法，子类可进行重写
        /// 例子：如regDomain是HKEY_CLASSES_ROOT，subkey是software\\higame\\，则将判断HKEY_CLASSES_ROOT\\software\\higame\\注册表项是否存在
        /// </summary>
        /// <param name="subKey">注册表项名称</param>
        /// <param name="regDomain">注册表基项域</param>
        /// <returns>返回注册表项是否存在，存在返回true，否则返回false</returns>
        public virtual bool IsSubKeyExist(string subKey, RegDomain regDomain) {
            ///判断注册表项名称是否为空，如果为空，返回false
            if (subKey == string.Empty || subKey == null) {
                return false;
            }

            ///检索注册表子项
            ///如果sKey为null,说明没有该注册表项不存在，否则存在
            RegistryKey sKey = OpenSubKey(subKey, regDomain);
            if (sKey == null) {
                return false;
            }
            return true;
        }
        #endregion

        #region 删除注册表项
        /// <summary>
        /// 删除注册表项（请先设置SubKey属性）
        /// 虚方法，子类可进行重写
        /// </summary>
        /// <returns>如果删除成功，则返回true，否则为false</returns>
        public virtual bool DeleteSubKey() {
            ///返回删除是否成功
            bool result = false;

            ///判断注册表项名称是否为空，如果为空，返回false
            if (_subkey == string.Empty || _subkey == null) {
                return false;
            }

            ///创建基于注册表基项的节点
            RegistryKey key = GetRegDomain(_domain);

            if (IsSubKeyExist()) {
                try {
                    ///删除注册表项
                    key.DeleteSubKey(_subkey);
                    result = true;
                } catch {
                    result = false;
                }
            }
            ///关闭对注册表项的更改
            key.Close();
            return result;
        }

        /// <summary>
        /// 删除注册表项（请先设置SubKey属性）
        /// 虚方法，子类可进行重写
        /// </summary>
        /// <param name="subKey">注册表项名称</param>
        /// <returns>如果删除成功，则返回true，否则为false</returns>
        public virtual bool DeleteSubKey(string subKey) {
            ///返回删除是否成功
            bool result = false;

            ///判断注册表项名称是否为空，如果为空，返回false
            if (subKey == string.Empty || subKey == null) {
                return false;
            }

            ///创建基于注册表基项的节点
            RegistryKey key = GetRegDomain(_domain);

            if (IsSubKeyExist()) {
                try {
                    ///删除注册表项
                    key.DeleteSubKey(subKey);
                    result = true;
                } catch {
                    result = false;
                }
            }
            ///关闭对注册表项的更改
            key.Close();
            return result;
        }

        /// <summary>
        /// 删除注册表项
        /// 虚方法，子类可进行重写
        /// </summary>
        /// <param name="subKey">注册表项名称</param>
        /// <param name="regDomain">注册表基项域</param>
        /// <returns>如果删除成功，则返回true，否则为false</returns>
        public virtual bool DeleteSubKey(string subKey, RegDomain regDomain) {
            ///返回删除是否成功
            bool result = false;

            ///判断注册表项名称是否为空，如果为空，返回false
            if (subKey == string.Empty || subKey == null) {
                return false;
            }

            ///创建基于注册表基项的节点
            RegistryKey key = GetRegDomain(regDomain);

            if (IsSubKeyExist(subKey, regDomain)) {
                try {
                    ///删除注册表项
                    key.DeleteSubKey(subKey);
                    result = true;
                } catch {
                    result = false;
                }
            }
            ///关闭对注册表项的更改
            key.Close();
            return result;
        }
        #endregion

        #region 判断键值是否存在
        /// <summary>
        /// 判断键值是否存在（请先设置SubKey和RegeditKey属性）
        /// 虚方法，子类可进行重写
        /// 1.如果RegeditKey为空、null，则返回false
        /// 2.如果SubKey为空、null或者SubKey指定的注册表项不存在，返回false
        /// </summary>
        /// <returns>返回键值是否存在，存在返回true，否则返回false</returns>
        public virtual bool IsRegeditKeyExist() {
            ///返回结果
            bool result = false;

            ///判断是否设置键值属性
            if (_regeditkey == string.Empty || _regeditkey == null) {
                return false;
            }

            ///判断注册表项是否存在
            if (IsSubKeyExist()) {
                ///打开注册表项
                RegistryKey key = OpenSubKey();
                ///键值集合
                string[] regeditKeyNames;
                ///获取键值集合
                regeditKeyNames = key.GetValueNames();
                ///遍历键值集合，如果存在键值，则退出遍历
                foreach (string regeditKey in regeditKeyNames) {
                    if (string.Compare(regeditKey, _regeditkey, true) == 0) {
                        result = true;
                        break;
                    }
                }
                ///关闭对注册表项的更改
                key.Close();
            }
            return result;
        }

        /// <summary>
        /// 判断键值是否存在（请先设置SubKey属性）
        /// 虚方法，子类可进行重写
        /// 如果SubKey为空、null或者SubKey指定的注册表项不存在，返回false
        /// </summary>
        /// <param name="name">键值名称</param>
        /// <returns>返回键值是否存在，存在返回true，否则返回false</returns>
        public virtual bool IsRegeditKeyExist(string name) {
            ///返回结果
            bool result = false;

            ///判断是否设置键值属性
            if (name == string.Empty || name == null) {
                return false;
            }

            ///判断注册表项是否存在
            if (IsSubKeyExist()) {
                ///打开注册表项
                RegistryKey key = OpenSubKey();
                ///键值集合
                string[] regeditKeyNames;
                ///获取键值集合
                regeditKeyNames = key.GetValueNames();
                ///遍历键值集合，如果存在键值，则退出遍历
                foreach (string regeditKey in regeditKeyNames) {
                    if (string.Compare(regeditKey, name, true) == 0) {
                        result = true;
                        break;
                    }
                }
                ///关闭对注册表项的更改
                key.Close();
            }
            return result;
        }

        /// <summary>
        /// 判断键值是否存在
        /// 虚方法，子类可进行重写
        /// </summary>
        /// <param name="name">键值名称</param>
        /// <param name="subKey">注册表项名称</param>
        /// <returns>返回键值是否存在，存在返回true，否则返回false</returns>
        public virtual bool IsRegeditKeyExist(string name, string subKey) {
            ///返回结果
            bool result = false;

            ///判断是否设置键值属性
            if (name == string.Empty || name == null) {
                return false;
            }

            ///判断注册表项是否存在
            if (IsSubKeyExist()) {
                ///打开注册表项
                RegistryKey key = OpenSubKey(subKey);
                ///键值集合
                string[] regeditKeyNames;
                ///获取键值集合
                regeditKeyNames = key.GetValueNames();
                ///遍历键值集合，如果存在键值，则退出遍历
                foreach (string regeditKey in regeditKeyNames) {
                    if (string.Compare(regeditKey, name, true) == 0) {
                        result = true;
                        break;
                    }
                }
                ///关闭对注册表项的更改
                key.Close();
            }
            return result;
        }

        /// <summary>
        /// 判断键值是否存在
        /// 虚方法，子类可进行重写
        /// </summary>
        /// <param name="name">键值名称</param>
        /// <param name="subKey">注册表项名称</param>
        /// <param name="regDomain">注册表基项域</param>
        /// <returns>返回键值是否存在，存在返回true，否则返回false</returns>
        public virtual bool IsRegeditKeyExist(string name, string subKey, RegDomain regDomain) {
            ///返回结果
            bool result = false;

            ///判断是否设置键值属性
            if (name == string.Empty || name == null) {
                return false;
            }

            ///判断注册表项是否存在
            if (IsSubKeyExist()) {
                ///打开注册表项
                RegistryKey key = OpenSubKey(subKey, regDomain);
                ///键值集合
                string[] regeditKeyNames;
                ///获取键值集合
                regeditKeyNames = key.GetValueNames();
                ///遍历键值集合，如果存在键值，则退出遍历
                foreach (string regeditKey in regeditKeyNames) {
                    if (string.Compare(regeditKey, name, true) == 0) {
                        result = true;
                        break;
                    }
                }
                ///关闭对注册表项的更改
                key.Close();
            }
            return result;
        }
        #endregion

        #region 设置键值内容
        /// <summary>
        /// 设置指定的键值内容，不指定内容数据类型（请先设置RegeditKey和SubKey属性）
        /// 存在改键值则修改键值内容，不存在键值则先创建键值，再设置键值内容
        /// </summary>
        /// <param name="content">键值内容</param>
        /// <returns>键值内容设置成功，则返回true，否则返回false</returns>
        public virtual bool WriteRegeditKey(object content) {
            ///返回结果
            bool result = false;

            ///判断是否设置键值属性
            if (_regeditkey == string.Empty || _regeditkey == null) {
                return false;
            }

            ///判断注册表项是否存在，如果不存在，则直接创建
            if (!IsSubKeyExist(_subkey)) {
                CreateSubKey(_subkey);
            }

            ///以可写方式打开注册表项
            RegistryKey key = OpenSubKey(true);

            ///如果注册表项打开失败，则返回false
            if (key == null) {
                return false;
            }

            try {
                key.SetValue(_regeditkey, content);
                result = true;
            } catch {
                result = false;
            } finally {
                ///关闭对注册表项的更改
                key.Close();
            }
            return result;
        }

        /// <summary>
        /// 设置指定的键值内容，不指定内容数据类型（请先设置SubKey属性）
        /// 存在改键值则修改键值内容，不存在键值则先创建键值，再设置键值内容
        /// </summary>
        /// <param name="name">键值名称</param>
        /// <param name="content">键值内容</param>
        /// <returns>键值内容设置成功，则返回true，否则返回false</returns>
        public virtual bool WriteRegeditKey(string name, object content) {
            ///返回结果
            bool result = false;

            ///判断键值是否存在
            if (name == string.Empty || name == null) {
                return false;
            }

            ///判断注册表项是否存在，如果不存在，则直接创建
            if (!IsSubKeyExist(_subkey)) {
                CreateSubKey(_subkey);
            }

            ///以可写方式打开注册表项
            RegistryKey key = OpenSubKey(true);

            ///如果注册表项打开失败，则返回false
            if (key == null) {
                return false;
            }

            try {
                key.SetValue(name, content);
                result = true;
            } catch (Exception ex) {
                result = false;
            } finally {
                ///关闭对注册表项的更改
                key.Close();
            }
            return result;
        }

        /// <summary>
        /// 设置指定的键值内容，指定内容数据类型（请先设置SubKey属性）
        /// 存在改键值则修改键值内容，不存在键值则先创建键值，再设置键值内容
        /// </summary>
        /// <param name="name">键值名称</param>
        /// <param name="content">键值内容</param>
        /// <returns>键值内容设置成功，则返回true，否则返回false</returns>
        public virtual bool WriteRegeditKey(string name, object content, RegValueKind regValueKind) {
            ///返回结果
            bool result = false;

            ///判断键值是否存在
            if (name == string.Empty || name == null) {
                return false;
            }

            ///判断注册表项是否存在，如果不存在，则直接创建
            if (!IsSubKeyExist(_subkey)) {
                CreateSubKey(_subkey);
            }

            ///以可写方式打开注册表项
            RegistryKey key = OpenSubKey(true);

            ///如果注册表项打开失败，则返回false
            if (key == null) {
                return false;
            }

            try {
                key.SetValue(name, content, GetRegValueKind(regValueKind));
                result = true;
            } catch {
                result = false;
            } finally {
                ///关闭对注册表项的更改
                key.Close();
            }
            return result;
        }
        #endregion

        #region 读取键值内容
        /// <summary>
        /// 读取键值内容（请先设置RegeditKey和SubKey属性）
        /// 1.如果RegeditKey为空、null或者RegeditKey指示的键值不存在，返回null
        /// 2.如果SubKey为空、null或者SubKey指示的注册表项不存在，返回null
        /// 3.反之，则返回键值内容
        /// </summary>
        /// <returns>返回键值内容</returns>
        public virtual object ReadRegeditKey() {
            ///键值内容结果
            object obj = null;

            ///判断是否设置键值属性
            if (_regeditkey == string.Empty || _regeditkey == null) {
                return null;
            }

            ///判断键值是否存在
            if (IsRegeditKeyExist(_regeditkey)) {
                ///打开注册表项
                RegistryKey key = OpenSubKey();
                if (key != null) {
                    obj = key.GetValue(_regeditkey);
                }
                ///关闭对注册表项的更改
                key.Close();
            }
            return obj;
        }

        /// <summary>
        /// 读取键值内容（请先设置SubKey属性）
        /// 1.如果SubKey为空、null或者SubKey指示的注册表项不存在，返回null
        /// 2.反之，则返回键值内容
        /// </summary>
        /// <param name="name">键值名称</param>
        /// <returns>返回键值内容</returns>
        public virtual object ReadRegeditKey(string name) {
            ///键值内容结果
            object obj = null;

            ///判断是否设置键值属性
            if (name == string.Empty || name == null) {
                return null;
            }

            ///判断键值是否存在
            if (IsRegeditKeyExist(name)) {
                ///打开注册表项
                RegistryKey key = OpenSubKey();
                if (key != null) {
                    obj = key.GetValue(name);
                }
                ///关闭对注册表项的更改
                key.Close();
            }
            return obj;
        }

        /// <summary>
        /// 读取键值内容
        /// </summary>
        /// <param name="name">键值名称</param>
        /// <param name="subKey">注册表项名称</param>
        /// <returns>返回键值内容</returns>
        public virtual object ReadRegeditKey(string name, string subKey) {
            ///键值内容结果
            object obj = null;

            ///判断是否设置键值属性
            if (name == string.Empty || name == null) {
                return null;
            }

            ///判断键值是否存在
            if (IsRegeditKeyExist(name)) {
                ///打开注册表项
                RegistryKey key = OpenSubKey(subKey);
                if (key != null) {
                    obj = key.GetValue(name);
                }
                ///关闭对注册表项的更改
                key.Close();
            }
            return obj;
        }

        /// <summary>
        /// 读取键值内容
        /// </summary>
        /// <param name="name">键值名称</param>
        /// <param name="subKey">注册表项名称</param>
        /// <param name="regDomain">注册表基项域</param>
        /// <returns>返回键值内容</returns>
        public virtual object ReadRegeditKey(string name, string subKey, RegDomain regDomain) {
            ///键值内容结果
            object obj = null;

            ///判断是否设置键值属性
            if (name == string.Empty || name == null) {
                return null;
            }

            ///判断键值是否存在
            if (IsRegeditKeyExist(name)) {
                ///打开注册表项
                RegistryKey key = OpenSubKey(subKey, regDomain);
                if (key != null) {
                    obj = key.GetValue(name);
                }
                ///关闭对注册表项的更改
                key.Close();
            }
            return obj;
        }
        #endregion

        #region 删除键值
        /// <summary>
        /// 删除键值（请先设置RegeditKey和SubKey属性）
        /// 1.如果RegeditKey为空、null或者RegeditKey指示的键值不存在，返回false
        /// 2.如果SubKey为空、null或者SubKey指示的注册表项不存在，返回false
        /// </summary>
        /// <returns>如果删除成功，返回true，否则返回false</returns>
        public virtual bool DeleteRegeditKey() {
            ///删除结果
            bool result = false;

            ///判断是否设置键值属性，如果没有设置，则返回false
            if (_regeditkey == string.Empty || _regeditkey == null) {
                return false;
            }

            ///判断键值是否存在
            if (IsRegeditKeyExist(_regeditkey)) {
                ///以可写方式打开注册表项
                RegistryKey key = OpenSubKey(true);
                if (key != null) {
                    try {
                        ///删除键值
                        key.DeleteValue(_regeditkey);
                        result = true;
                    } catch {
                        result = false;
                    } finally {
                        ///关闭对注册表项的更改
                        key.Close();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 删除键值（请先设置SubKey属性）
        /// 如果SubKey为空、null或者SubKey指示的注册表项不存在，返回false
        /// </summary>
        /// <param name="name">键值名称</param>
        /// <returns>如果删除成功，返回true，否则返回false</returns>
        public virtual bool DeleteRegeditKey(string name) {
            ///删除结果
            bool result = false;

            ///判断键值名称是否为空，如果为空，则返回false
            if (name == string.Empty || name == null) {
                return false;
            }

            ///判断键值是否存在
            if (IsRegeditKeyExist(name)) {
                ///以可写方式打开注册表项
                RegistryKey key = OpenSubKey(true);
                if (key != null) {
                    try {
                        ///删除键值
                        key.DeleteValue(name);
                        result = true;
                    } catch {
                        result = false;
                    } finally {
                        ///关闭对注册表项的更改
                        key.Close();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 删除键值
        /// </summary>
        /// <param name="name">键值名称</param>
        /// <param name="subKey">注册表项名称</param>
        /// <returns>如果删除成功，返回true，否则返回false</returns>
        public virtual bool DeleteRegeditKey(string name, string subKey) {
            ///删除结果
            bool result = false;

            ///判断键值名称和注册表项名称是否为空，如果为空，则返回false
            if (name == string.Empty || name == null || subKey == string.Empty || subKey == null) {
                return false;
            }

            ///判断键值是否存在
            if (IsRegeditKeyExist(name)) {
                ///以可写方式打开注册表项
                RegistryKey key = OpenSubKey(subKey, true);
                if (key != null) {
                    try {
                        ///删除键值
                        key.DeleteValue(name);
                        result = true;
                    } catch {
                        result = false;
                    } finally {
                        ///关闭对注册表项的更改
                        key.Close();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 删除键值
        /// </summary>
        /// <param name="name">键值名称</param>
        /// <param name="subKey">注册表项名称</param>
        /// <param name="regDomain">注册表基项域</param>
        /// <returns>如果删除成功，返回true，否则返回false</returns>
        public virtual bool DeleteRegeditKey(string name, string subKey, RegDomain regDomain) {
            ///删除结果
            bool result = false;

            ///判断键值名称和注册表项名称是否为空，如果为空，则返回false
            if (name == string.Empty || name == null || subKey == string.Empty || subKey == null) {
                return false;
            }

            ///判断键值是否存在
            if (IsRegeditKeyExist(name)) {
                ///以可写方式打开注册表项
                RegistryKey key = OpenSubKey(subKey, regDomain, true);
                if (key != null) {
                    try {
                        ///删除键值
                        key.DeleteValue(name);
                        result = true;
                    } catch {
                        result = false;
                    } finally {
                        ///关闭对注册表项的更改
                        key.Close();
                    }
                }
            }

            return result;
        }
        #endregion
        #endregion

        #region 受保护方法
        /// <summary>
        /// 获取注册表基项域对应顶级节点
        /// 例子：如regDomain是ClassesRoot，则返回Registry.ClassesRoot
        /// </summary>
        /// <param name="regDomain">注册表基项域</param>
        /// <returns>注册表基项域对应顶级节点</returns>
        protected RegistryKey GetRegDomain(RegDomain regDomain) {
            ///创建基于注册表基项的节点
            RegistryKey key;

            #region 判断注册表基项域
            switch (regDomain) {
                case RegDomain.ClassesRoot:
                    key = Registry.ClassesRoot; break;
                case RegDomain.CurrentUser:
                    key = Registry.CurrentUser; break;
                case RegDomain.LocalMachine:
                    key = Registry.LocalMachine; break;
                case RegDomain.User:
                    key = Registry.Users; break;
                case RegDomain.CurrentConfig:
                    key = Registry.CurrentConfig; break;
                case RegDomain.DynDa:
                    key = Registry.DynData; break;
                case RegDomain.PerformanceData:
                    key = Registry.PerformanceData; break;
                default:
                    key = Registry.LocalMachine; break;
            }
            #endregion

            return key;
        }

        /// <summary>
        /// 获取在注册表中对应的值数据类型
        /// 例子：如regValueKind是DWord，则返回RegistryValueKind.DWord
        /// </summary>
        /// <param name="regValueKind">注册表数据类型</param>
        /// <returns>注册表中对应的数据类型</returns>
        protected RegistryValueKind GetRegValueKind(RegValueKind regValueKind) {
            RegistryValueKind regValueK;

            #region 判断注册表数据类型
            switch (regValueKind) {
                case RegValueKind.Unknown:
                    regValueK = RegistryValueKind.Unknown; break;
                case RegValueKind.String:
                    regValueK = RegistryValueKind.String; break;
                case RegValueKind.ExpandString:
                    regValueK = RegistryValueKind.ExpandString; break;
                case RegValueKind.Binary:
                    regValueK = RegistryValueKind.Binary; break;
                case RegValueKind.DWord:
                    regValueK = RegistryValueKind.DWord; break;
                case RegValueKind.MultiString:
                    regValueK = RegistryValueKind.MultiString; break;
                case RegValueKind.QWord:
                    regValueK = RegistryValueKind.QWord; break;
                default:
                    regValueK = RegistryValueKind.String; break;
            }
            #endregion
            return regValueK;
        }

        #region 打开注册表项
        /// <summary>
        /// 打开注册表项节点，以只读方式检索子项
        /// 虚方法，子类可进行重写
        /// </summary>
        /// <returns>如果SubKey为空、null或者SubKey指示注册表项不存在，则返回null，否则返回注册表节点</returns>
        protected virtual RegistryKey OpenSubKey() {
            ///判断注册表项名称是否为空
            if (_subkey == string.Empty || _subkey == null) {
                return null;
            }

            ///创建基于注册表基项的节点
            RegistryKey key = GetRegDomain(_domain);

            ///要打开的注册表项的节点
            RegistryKey sKey = null;
            ///打开注册表项
            sKey = key.OpenSubKey(_subkey);
            ///关闭对注册表项的更改
            key.Close();
            ///返回注册表节点
            return sKey;
        }

        /// <summary>
        /// 打开注册表项节点
        /// 虚方法，子类可进行重写
        /// </summary>
        /// <param name="writable">如果需要项的写访问权限，则设置为 true</param>
        /// <returns>如果SubKey为空、null或者SubKey指示注册表项不存在，则返回null，否则返回注册表节点</returns>
        protected virtual RegistryKey OpenSubKey(bool writable) {
            ///判断注册表项名称是否为空
            if (_subkey == string.Empty || _subkey == null) {
                return null;
            }

            ///创建基于注册表基项的节点
            RegistryKey key = GetRegDomain(_domain);

            ///要打开的注册表项的节点
            RegistryKey sKey = null;
            ///打开注册表项
            sKey = key.OpenSubKey(_subkey, writable);
            ///关闭对注册表项的更改
            key.Close();
            ///返回注册表节点
            return sKey;
        }

        /// <summary>
        /// 打开注册表项节点，以只读方式检索子项
        /// 虚方法，子类可进行重写
        /// </summary>
        /// <param name="subKey">注册表项名称</param>
        /// <returns>如果SubKey为空、null或者SubKey指示注册表项不存在，则返回null，否则返回注册表节点</returns>
        protected virtual RegistryKey OpenSubKey(string subKey) {
            ///判断注册表项名称是否为空
            if (subKey == string.Empty || subKey == null) {
                return null;
            }

            ///创建基于注册表基项的节点
            RegistryKey key = GetRegDomain(_domain);

            ///要打开的注册表项的节点
            RegistryKey sKey = null;
            ///打开注册表项
            sKey = key.OpenSubKey(subKey);
            ///关闭对注册表项的更改
            key.Close();
            ///返回注册表节点
            return sKey;
        }

        /// <summary>
        /// 打开注册表项节点，以只读方式检索子项
        /// 虚方法，子类可进行重写
        /// </summary>
        /// <param name="subKey">注册表项名称</param>
        /// <param name="writable">如果需要项的写访问权限，则设置为 true</param>
        /// <returns>如果SubKey为空、null或者SubKey指示注册表项不存在，则返回null，否则返回注册表节点</returns>
        protected virtual RegistryKey OpenSubKey(string subKey, bool writable) {
            ///判断注册表项名称是否为空
            if (subKey == string.Empty || subKey == null) {
                return null;
            }

            ///创建基于注册表基项的节点
            RegistryKey key = GetRegDomain(_domain);

            ///要打开的注册表项的节点
            RegistryKey sKey = null;
            ///打开注册表项
            sKey = key.OpenSubKey(subKey, writable);
            ///关闭对注册表项的更改
            key.Close();
            ///返回注册表节点
            return sKey;
        }

        /// <summary>
        /// 打开注册表项节点，以只读方式检索子项
        /// 虚方法，子类可进行重写
        /// </summary>
        /// <param name="subKey">注册表项名称</param>
        /// <param name="regDomain">注册表基项域</param>
        /// <returns>如果SubKey为空、null或者SubKey指示注册表项不存在，则返回null，否则返回注册表节点</returns>
        protected virtual RegistryKey OpenSubKey(string subKey, RegDomain regDomain) {
            ///判断注册表项名称是否为空
            if (subKey == string.Empty || subKey == null) {
                return null;
            }

            ///创建基于注册表基项的节点
            RegistryKey key = GetRegDomain(regDomain);

            ///要打开的注册表项的节点
            RegistryKey sKey = null;
            ///打开注册表项
            sKey = key.OpenSubKey(subKey);
            ///关闭对注册表项的更改
            key.Close();
            ///返回注册表节点
            return sKey;
        }

        /// <summary>
        /// 打开注册表项节点
        /// 虚方法，子类可进行重写
        /// </summary>
        /// <param name="subKey">注册表项名称</param>
        /// <param name="regDomain">注册表基项域</param>
        /// <param name="writable">如果需要项的写访问权限，则设置为 true</param>
        /// <returns>如果SubKey为空、null或者SubKey指示注册表项不存在，则返回null，否则返回注册表节点</returns>
        protected virtual RegistryKey OpenSubKey(string subKey, RegDomain regDomain, bool writable) {
            ///判断注册表项名称是否为空
            if (subKey == string.Empty || subKey == null) {
                return null;
            }

            ///创建基于注册表基项的节点
            RegistryKey key = GetRegDomain(regDomain);

            ///要打开的注册表项的节点
            RegistryKey sKey = null;
            ///打开注册表项
            sKey = key.OpenSubKey(subKey, writable);
            ///关闭对注册表项的更改
            key.Close();
            ///返回注册表节点
            return sKey;
        }
        #endregion
        #endregion
    }
}
