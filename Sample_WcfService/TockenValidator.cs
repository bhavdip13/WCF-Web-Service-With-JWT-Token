using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Sample_WcfService
{
    public class TockenValidator : ServiceAuthorizationManager
    {
        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            if (operationContext.IncomingMessageHeaders.To.Segments.Count() >= 3)
            {
                var CurrentMethodName = operationContext.IncomingMessageHeaders.To.Segments[2];
                if (CurrentMethodName.ToLower() == "login" || CurrentMethodName.ToLower() == "register")
                    return true;
                //Extract the Authorization header, and parse out the credentials converting the Base64 string:  
                var tokenString = WebOperationContext.Current.IncomingRequest.Headers["Authorization"];
                if ((tokenString != null) && (tokenString != string.Empty))
                {
                    var jwtEncodedString = tokenString.Substring(7); // trim 'Bearer ' from the start since its just a prefix for the token string
                    var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);
                    var user = new { EmpId = token.Claims.First(c => c.Type == "EmpId").Value, Email = token.Claims.First(c => c.Type == "Email").Value, Password = token.Claims.First(c => c.Type == "Password").Value };
                    DataSet dsEmployee = CommonMethods.GetEmployeeRecoredFromEmpId(Convert.ToInt32(user.EmpId));
                    if ((user.Email == dsEmployee.Tables[0].Rows[0]["Email"].ToString() && user.Password == dsEmployee.Tables[0].Rows[0]["Password"].ToString()))
                    {
                        //User is authrized and originating call will proceed  
                        return true;
                    }
                    else
                    {
                        //not authorized  
                        return false;
                    }



                }
                else
                {
                    //No authorization header was provided, so challenge the client to provide before proceeding:  
                    WebOperationContext.Current.OutgoingResponse.Headers.Add("WWW-Authenticate: Basic realm=\"Sample_WcfService\"");
                    //Throw an exception with the associated HTTP status code equivalent to HTTP status 401  
                    //throw new WebFaultException(HttpStatusCode.Unauthorized);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
    }
}