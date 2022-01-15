using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Client
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page
    {
        public Login()
        {
            this.InitializeComponent();

        }

        private async void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            Login_Button.IsEnabled = false;
            string account = in_Account.Text;
            string password = in_Passwd.Password;
            int r = await Task.Run(() =>
            {
                return App.Auth.Login(account, password);
            });

            if (r == 0x6)
            {
                this.Frame.Navigate(typeof(Control_Center));
                Login_Button.IsEnabled = true;
            }
            else if (r == 0x1)
            {
                Login_Button.IsEnabled = true;
                Login_Fail.IsOpen = true;
            }
            else if(r == 0x2)
            {
                Login_Button.IsEnabled = true;
                Password_Error.Title = account + password;
                Password_Error.IsOpen = true;
            }
            else
            {
                Login_Button.IsEnabled = true;
            }
        }

        private void REG_Login_PG_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Register));
        }

    }
}
