﻿using System.Windows;

namespace PasswordSec {
    //   TODO Server Protection ×

    public partial class MainWindow : Window
    {
        public static MainWindow instance;

        public MainWindow() {
            InitializeComponent();
            if (checkUpdate())
            {

            }

            instance = this;
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

        public void disconnectToServer()
        {

        }
    }
}
