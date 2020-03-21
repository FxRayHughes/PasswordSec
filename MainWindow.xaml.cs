using System.Windows;

namespace PasswordSec {
    //TO-DO ARRAY {
    //   TODO Full App
    //   TODO Socket
    //   TODO Protect
    //}
    
    public partial class MainWindow : Window {


        public MainWindow() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            new Main("").Show();
            this.Close();
            //Dev使用，功能写好后请使用网验 谢谢a
        }

        public bool checkUpdate()
        {
            return true;
        }
    }
}
