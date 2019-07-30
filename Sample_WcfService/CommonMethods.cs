using Jose;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Script.Serialization;

namespace Sample_WcfService
{
    public static class CommonMethods
    {
        public static SqlConnection con = new SqlConnection("Data Source=DILIP\\SQLEXPRESS01;Initial Catalog=Employee;User ID=sa;Password=vision;");
        public static string ConnectionString = "Data Source=DILIP\\SQLEXPRESS01;Initial Catalog=Employee;User ID=sa;Password=vision;";

        private static byte[] Base64UrlDecode(string arg) // This function is for decoding string to   

        {
            string s = arg;
            s = s.Replace('-', '+'); // 62nd char of encoding  
            s = s.Replace('_', '/'); // 63rd char of encoding  
            switch (s.Length % 4) // Pad with trailing '='s  
            {
                case 0: break; // No pad chars in this case  
                case 2: s += "=="; break; // Two pad chars  
                case 3: s += "="; break; // One pad char  
                default:
                    throw new System.Exception(
                "Illegal base64url string!");
            }
            return Convert.FromBase64String(s); // Standard base64 decoder  
        }
        private static long ToUnixTime(DateTime dateTime)
        {
            return (int)(dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        public static string GetJwt(string Email, string Password, int EmpId) //function for JWT Token  
        {
            byte[] secretKey = Base64UrlDecode("Hi");//pass key to secure and decode it  
            DateTime issued = DateTime.Now;
            var User = new Dictionary<string, object>()
                    {
                        {"Email", Email},
                        {"EmpId", EmpId},
                        {"Password", Password},

                         {"iat", ToUnixTime(issued).ToString()}
                    };

            string token = JWT.Encode(User, secretKey, JwsAlgorithm.HS256);
            return token;
        }
        public static String Encrypt(string toEncrypt, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            //  Dim toEncryptArray As Byte() = UTF32Encoding.UTF32.GetBytes(toEncrypt)
            System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader();
            //  Get the key from config file
            string key = Convert.ToString((settingsReader.GetValue("SecurityKey", typeof(String))));


            // key = "AdeF5ty6Fr456Mw###"
            // System.Windows.Forms.MessageBox.Show(key)
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                // keyArray = hashmd5.ComputeHash(UTF32Encoding.UTF32.GetBytes(key))
                hashmd5.Clear();
            }
            else
            {
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
                // keyArray = UTF32Encoding.UTF32.GetBytes(key)
            }
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static String Decrypt(string cipherString, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = Convert.FromBase64String(cipherString);
            System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader();
            // Get your key from config file to open the lock!
            string key = Convert.ToString((settingsReader.GetValue("SecurityKey", typeof(String))));

            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                // keyArray = hashmd5.ComputeHash(UTF32Encoding.UTF32.GetBytes(key))
                hashmd5.Clear();
            }
            else
            {
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
                // keyArray = UTF32Encoding.UTF32.GetBytes(key)
            }
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        //public static bool IsValidateToken()
        //{
        //    //Extract the Authorization header, and parse out the credentials converting the Base64 string:  
        //    var tokenString = WebOperationContext.Current.IncomingRequest.Headers["Authorization"];

        //    if ((tokenString != null) && (tokenString != string.Empty))
        //    {
        //        var jwtEncodedString = tokenString.Substring(7); // trim 'Bearer ' from the start since its just a prefix for the token string
        //        var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);
        //        var user = new { EmpId = token.Claims.First(c => c.Type == "EmpId").Value, Email = token.Claims.First(c => c.Type == "Email").Value, Password = token.Claims.First(c => c.Type == "Password").Value };
        //        DataSet dsEmployee = GetEmployeeRecoredFromEmpId(Convert.ToInt32(user.EmpId));
        //        if ((user.Email == dsEmployee.Tables[0].Rows[0]["Email"].ToString() && user.Password == dsEmployee.Tables[0].Rows[0]["Password"].ToString()))
        //        {
        //            //User is authrized and originating call will proceed  
        //            return true;
        //        }
        //        else
        //        {
        //            //not authorized  
        //            return false;
        //        }

        //    }
        //    else
        //    {
        //        //No authorization header was provided, so challenge the client to provide before proceeding:  
        //        WebOperationContext.Current.OutgoingResponse.Headers.Add("WWW-Authenticate: Basic realm=\"Sample_WcfService\"");
        //        //Throw an exception with the associated HTTP status code equivalent to HTTP status 401  
        //        //throw new WebFaultException(HttpStatusCode.Unauthorized);
        //        return false;
        //    }
        //}
        
        public static DataSet GetEmployeeRecoredFromEmpId(int EmpId)
        {
            DataSet ds = new DataSet();
            try
            {
                string Query = "SELECT Email,Password FROM Emp WHERE EmpID=@EmpID";

                SqlDataAdapter sda = new SqlDataAdapter(Query, con);
                sda.SelectCommand.Parameters.AddWithValue("@EmpID", EmpId);
                sda.Fill(ds);
            }
            catch (FaultException fex)
            {
                throw new FaultException<string>("Error:  " + fex);
            }
            return ds;
        }
        public static string DataTableToJson(DataTable dataTable)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            var rows = (from DataRow d in dataTable.Rows
                        select dataTable.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => d[col])).ToList();
            return serializer.Serialize(rows);
        }

    }
}