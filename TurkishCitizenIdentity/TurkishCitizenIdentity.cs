using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace TurkishCitizenIdValidator
{
    public class TurkishCitizenIdentity
    {
        private long IdentityNo { get; set; }
        private string FirstName { get; set; }
        private string LastName { get; set; }
        private int BirthYear { get; set; }

        /// <summary>
        /// TC Kimlik Numarası Doğrulama
        /// </summary>
        /// <param name="identityNo">11 haneli TC Kimlik Numarası</param>
        /// <param name="firstName">Ad</param>
        /// <param name="lastName">Soyad</param>
        /// <param name="birthYear">Doğum Yılı</param>
        public TurkishCitizenIdentity(long identityNo, string firstName, string lastName, int birthYear)
        {
            this.IdentityNo = identityNo;
            this.FirstName = firstName.Trim().ToUpper();
            this.LastName = lastName.Trim().ToUpper();
            this.BirthYear = birthYear;
        }

        public bool IsValid()
        {
            ValidateInputs();

            var requestUrl = "https://tckimlik.nvi.gov.tr/Service/KPSPublic.asmx";
            var requestXmlbytesArray = System.Text.Encoding.UTF8.GetBytes(RequestXml());
            var requestContentLength = requestXmlbytesArray.Length;
            var request = BuildRequest(requestUrl, requestContentLength);
            var response = GetResponse(request, requestXmlbytesArray);

            return response;
        }

        private bool IdentityNoCorrect()
        {
            return this.IdentityNo.ToString().Length == 11;
        }

        private bool BirthYearCorrect()
        {
            return this.BirthYear.ToString().Length == 4;
        }

        private bool FirstNameCorrect()
        {
            return string.IsNullOrEmpty(this.FirstName) == false;
        }

        private bool LastNameCorrect()
        {
            return string.IsNullOrEmpty(this.LastName) == false;
        }

        private void ValidateInputs()
        {
            if (!IdentityNoCorrect())
            {
                throw new Exception("TC Kimlik Numarası 11 haneli olmalı");
            }

            if (!FirstNameCorrect())
            {
                throw new Exception("Ad boş olamaz");
            }

            if (!LastNameCorrect())
            {
                throw new Exception("Soyad boş olamaz");
            }

            if (!BirthYearCorrect())
            {
                throw new Exception("Doğum Yılı 4 haneli olmalı");
            }
        }

        private string RequestXml()
        {
            var requestXml = @"<?xml version=""1.0"" encoding=""utf-8""?>";
            requestXml += @"<soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">";
            requestXml += @"<soap12:Body>";
            requestXml += @"<TCKimlikNoDogrula xmlns=""http://tckimlik.nvi.gov.tr/WS"">";
            requestXml += @"<TCKimlikNo>" + IdentityNo + "</TCKimlikNo>";
            requestXml += @"<Ad>" + FirstName + "</Ad>";
            requestXml += @"<Soyad>" + LastName + "</Soyad>";
            requestXml += @"<DogumYili>" + BirthYear + "</DogumYili>";
            requestXml += @"</TCKimlikNoDogrula>";
            requestXml += @"</soap12:Body>";
            requestXml += @"</soap12:Envelope>";

            return requestXml;
        }

        private HttpWebRequest BuildRequest(string requestUrl, long contentLength)
        {
            var request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.ContentType = "application/soap+xml; charset=utf-8";
            request.Method = "POST";
            request.ContentLength = contentLength;

            return request;
        }

        private bool GetResponse(HttpWebRequest request, byte[] requestBytesArray)
        {
            using (var stream = request.GetRequestStream())
            {
                stream.Write(requestBytesArray, 0, (int)request.ContentLength);
            }

            string responseString;
            try
            {
                using (var response = request.GetResponse())
                {
                    if (response == null)
                    {
                        return false;
                    }

                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseString = reader.ReadToEnd().Trim();
                    }
                }

                var responseXml = XDocument.Parse(responseString);
                var result = responseXml.Descendants().SingleOrDefault(x => x.Name.LocalName == "TCKimlikNoDogrulaResult").Value;

                return bool.Parse(result);
            }
            catch
            {
                return false;
            }
        }
    }
}
