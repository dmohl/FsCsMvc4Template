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

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            IncludeTestsProject =
                cbIncludeTestsProject.IsChecked.HasValue ? cbIncludeTestsProject.IsChecked.Value : false;
            SelectedViewEngine = cbViewEngine.SelectionBoxItem.ToString();
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
