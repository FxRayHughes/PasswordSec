using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// <summary>
    /// NewProfile.xaml 的交互逻辑
    /// </summary>
    public partial class NewProfile : Window
    {
        public Dictionary<string, string> ei = new Dictionary<string, string>();

        public NewProfile() {
            InitializeComponent();
        }


        private void eilist_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (eilist.SelectedItem != null)
            {
                try
                {
                    MessageBox.Show(sender.ToString());
                    string s = ei[eilist.SelectedItem as string];
                    crv.Content = s;
                }
                catch (ArgumentException e1)
                {
                    MessageBox.Show("吶 跟你说个事 这里莫名其妙就null了 我也不知道我做了啥 QA Q");
                }
            }
        }

        public static string GetTimeStamp() {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        private void AddExtraInfo(object sender, RoutedEventArgs e) {
            if (ei.ContainsKey(eik.Text)) {
                MessageBox.Show("你在想啥啊？里面都有了啊", "添加失败");
                return;
            }
            ei.Add(eik.Text, eiv.Text);
            eilist.Items.Add(eik.Text);
        }

        private void Save(object sender, RoutedEventArgs e) {
            if (apn.Text.Length == 0) {
                MessageBox.Show("你是不是有什么东西没有填写鸭！", "保存失败");
                return;
            }

            RootObject obj = new RootObject();
            obj.appname = apn.Text;
            obj.email = eml.Text;
            obj.username = usr.Text;
            obj.password = pas.Text;
            List<ExtraInfo> list = new List<ExtraInfo>();
            foreach (var entry in ei) {
                ExtraInfo info = new ExtraInfo();
                info.infokey = entry.Key;
                info.infovalue = entry.Value;
                list.Add(info);
            }

            obj.extraInfo = list;

            try {
                string json = JsonConvert.SerializeObject(obj);
                FileStream fs = new FileStream(Main.instance.baseDirectory.FullName + @"\" + GetTimeStamp() + ((bool)enc.IsChecked ? ".encrypteduap" : ".uap"), FileMode.CreateNew, FileAccess.Write);
                byte[] bytes = Encoding.UTF8.GetBytes((bool)enc.IsChecked ? AesUtil.AesEncrypt(json, Main.instance.ENCRYPT_KEY) : json);
                fs.Write(bytes, 0, bytes.Length);
                fs.Flush();
                fs.Dispose();
                fs.Close();
                MessageBox.Show("哇哦，成功了呢", "保存成功");
                Main.instance.Loaduap();
                this.Close();
            } catch (IOException exc) {
                MessageBox.Show("无法报错 未知原因\n" + exc.StackTrace, "保存失败");
            }
        }

        private void DeleateExtraInfo(object sender, RoutedEventArgs e) {
            if (eilist.SelectedItem == null) {
                MessageBox.Show("你在干嘛呀！选择一个啊！", "删除失败");
                return;
            }

            if (!ei.Remove(eilist.SelectedItem as string)) {
                MessageBox.Show("未知原因", "删除失败");
                return;
            }

            eilist.Items.Remove(eilist.SelectedItem);
        }
    }
}
