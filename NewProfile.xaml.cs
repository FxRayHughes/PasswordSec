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
using System.Windows.Shapes;

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

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (ei.ContainsKey(eik.Text))
            {
                MessageBox.Show("你在想啥啊？里面都有了啊", "添加失败");
                return;
            }
            ei.Add(eik.Text, eiv.Text);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (eilist.SelectedItem == null)
            {
                MessageBox.Show("你在干嘛呀！选择一个啊！", "删除失败");
                return;
            }

            if (ei.Remove(eilist.SelectedItem as string))
            {
                MessageBox.Show("未知原因", "删除失败");
            }
            
        }

        private void eilist_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (eilist.SelectedItem != null)
            {
                crv.Content = ei[eilist.SelectedItem as string];

            }
        }
    }
}
