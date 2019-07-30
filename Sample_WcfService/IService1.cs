using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

namespace Sample_WcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        [WebInvoke( Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)] //Post Method for gtting data according to the parameter  
        LoginResponseData Login(LoginRequestData emp);
        
        [OperationContract]
        [WebInvoke( Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)] //Post Method for gtting data according to the parameter  
        Response Register(Employee emp);


        [OperationContract]
        [WebInvoke( Method = "GET", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        DatasetResponse GetEmployeeRecords();

   
        [OperationContract]
        string DeleteRecords(Employee emp);

        [OperationContract]
        DataSet SearchEmployeeRecord(Employee emp);

        [OperationContract]
        string UpdateEmployeeContact(Employee emp);

        // TODO: Add your service operations here
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.  
    [DataContract]
    public class Employee
    {

        string _empID = "";
        string _name = "";
        string _email = "";
        string _password = "";
        string _phone = "";
        string _gender = "";

        [DataMember(Order = 0)]
        public string EmpID
        {
            get { return _empID; }
            set { _empID = value; }
        }

        [DataMember(Order = 1)]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [DataMember(Order = 2)]
        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }
        [DataMember(Order = 3)]
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        [DataMember(Order = 4)]
        public string Phone
        {
            get { return _phone; }
            set { _phone = value; }
        }

        [DataMember(Order = 5)]
        public string Gender
        {
            get { return _gender; }
            set { _gender = value; }
        }

       
    }
    public class LoginRequestData
    {
        public int EmpId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
    //[DataContract]
    public class DatasetResponse
    {
        public string data { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
    }
}
