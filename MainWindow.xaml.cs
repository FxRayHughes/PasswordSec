using System.Windows;

namespace PasswordSec {
    //TO-DO ARRAY {
    //   TODO Basic   App   √
    //   TODO Adviced App   ×
    //   TODO SUPER   App   ×
    //   TODO Socket        ×
    //   TODO Protect       √
    //}

    public partial class MainWindow : Window {


        public MainWindow() {
            InitializeComponent();
            if (checkUpdate())
            {

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (!pwd.Password.Equals("\u0043\u0072\u0061\u0063\u006b\u004d\u0065\u0049\u0064\u0069\u006f\u0074"))
            {
                new Main("").Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("错了哟！");
            }

        }

        public bool checkUpdate()
        {
            return true;
        }
    }
}
