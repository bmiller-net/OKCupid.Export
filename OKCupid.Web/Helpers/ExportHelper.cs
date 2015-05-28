using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace OKCupid.Web.Helpers
{
    public class ExportHelper
    {
        private CookieContainer _container;
        private const string LoginUrl = "https://www.okcupid.com/login";
        private readonly string _username = ConfigurationManager.AppSettings["OKCupid_Username"];
        private readonly string _password = ConfigurationManager.AppSettings["OKCupid_Password"];
        private bool _isOpen;

        public string GetPage(string url)
        {
            if (!_isOpen)
            {
                Login();
            }

            return GetSecondaryLoginPage(url);
        }

        public void Login()
        {

            // Create a request using a URL that can receive a post. 
            var request = (HttpWebRequest)WebRequest.Create(LoginUrl);
            // Set the Method property of the request to POST.
            request.Method = "POST";

            _container = new CookieContainer();

            request.CookieContainer = _container;

            // Create POST data and convert it to a byte array.  Modify this line accordingly
            var postData = String.Format("username={0}&password={1}", _username, _password);

            ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;

            var byteArray = Encoding.UTF8.GetBytes(postData);
            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/x-www-form-urlencoded";
            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;
            // Get the request stream.
            var dataStream = request.GetRequestStream();
            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.
            dataStream.Close();
            // Get the response.
            var response = request.GetResponse();
            // Get the stream containing content returned by the server.
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            var reader = new StreamReader(dataStream);
            // Read the content.
            var responseFromServer = reader.ReadToEnd();

            using (var outfile = new StreamWriter("output.html"))
            {
                outfile.Write(responseFromServer.ToString(CultureInfo.InvariantCulture));
            }

            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();

            _isOpen = true;
        }

        public string GetSecondaryLoginPage(string url)
        {
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.CookieContainer = _container;


            var response = request.GetResponse();
            // Get the stream containing content returned by the server.
            var dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            var reader = new StreamReader(dataStream);
            // Read the content.
            var responseFromServer = reader.ReadToEnd();

            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer;
        }


        public bool AcceptAllCertifications(object sender,
                                            System.Security.Cryptography.X509Certificates.X509Certificate certification,
                                            System.Security.Cryptography.X509Certificates.X509Chain chain,
                                            System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}