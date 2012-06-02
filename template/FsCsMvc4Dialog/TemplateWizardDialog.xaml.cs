using System.Windows;

namespace FsCsMvc4Dialog
{
    public partial class TemplateWizardDialog : Window
    {
        public TemplateWizardDialog()
        {
            InitializeComponent();
        }

        public bool IncludeTestsProject { get; protected set; }
        public string SelectedViewEngine { get; protected set; }
        public bool IsWebApi { get; protected set; }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            IncludeTestsProject =
                cbIncludeTestsProject.IsChecked.HasValue ? cbIncludeTestsProject.IsChecked.Value : false;
            if (IsWebApi)
            {
                SelectedViewEngine = "Razor";
                cbViewEngine.SelectedIndex = 1;
            }
            else
            {
                SelectedViewEngine = cbViewEngine.SelectionBoxItem.ToString();
            }
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void lvwProjectType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            IsWebApi = lvwProjectType.SelectedIndex == 1;
            cbViewEngine.IsEnabled = !IsWebApi;
        }
    }
}
