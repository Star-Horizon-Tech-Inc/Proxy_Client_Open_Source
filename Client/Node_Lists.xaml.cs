using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Client
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Node_Lists : Page
    {
        public Node_Lists()
        {
            this.InitializeComponent();
            Load_Node_List();
        }

        private async void Load_Node_List()
        {
            req_body = await Task.Run(() =>
            {
                return App.Auth.Get_Nodes_info();
            });
            if(req_body.code == 0x13)
            {
                node_list_view.ItemsSource = req_body.msg;
            }
            else if (req_body.code == 0x14)
            {

            }
            else
            {

            }
        }

        private static BasicAuth.Req_Body req_body { get; set; }
    }
}
