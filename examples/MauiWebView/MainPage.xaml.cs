using System.Text;
namespace MauiWebView
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private StringBuilder base64StringBuilder = new StringBuilder();
        private bool isReceivingChunks = false;

        public MainPage()
        {
            InitializeComponent();
            LoadHtmlFile();
        }

        private async void LoadHtmlFile()
        {
            WebView.Source = "index.html";
            
        }

        private async void OnWebViewNavigated(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url.StartsWith("invoke://callcsharpfunction"))
            {
                var base64String = await WebView.EvaluateJavaScriptAsync("getBase64Image()");
                CallCSharpFunction(base64String);
            }
        }

        private void CallCSharpFunction(string base64String)
        {
            if (!string.IsNullOrEmpty(base64String))
            {
               
                try
                {
                    byte[] imageBytes = Convert.FromBase64String(base64String);

                    var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), GenerateFilename());
                    //string filePath = Path.Combine(FileSystem.AppDataDirectory, GenerateFilename());
                    File.WriteAllBytes(filePath, imageBytes);
                    DisplayAlert("Success", "Image saved to: " + filePath, "OK");
                    
                }
                catch (Exception ex)
                {
                    DisplayAlert("Error", ex.Message, "OK");
                }
            }
            else
            {
                DisplayAlert("Failure", "No image data found", "OK");
            }
        }

        private string GenerateFilename()
        {
            DateTime now = DateTime.Now;
            string timestamp = now.ToString("yyyyMMdd_HHmmss");
            return $"image_{timestamp}.png";
        }
    }

}
