using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace Sample_WcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Service1 : IService1
    {
        //Global connection string
        SqlConnection con = new SqlConnection("Data Source=DILIP\\SQLEXPRESS01;Initial Catalog=Employee;User ID=sa;Password=vision;");
        string ConnectionString = "Data Source=DILIP\\SQLEXPRESS01;Initial Catalog=Employee;User ID=sa;Password=vision;";
        string UserToken = string.Empty;

        // [OperationContract] //service contract  
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)] //Post Method for gtting data according to the parameter  
        public LoginResponseData Login(LoginRequestData data) //Response class for retriving data  
        {
            using (SqlConnection _con = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_LogIn", _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", data.Email);
                cmd.Parameters.AddWithValue("@Password", CommonMethods.Encrypt(data.Password, true));
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet dt_Login = new DataSet();
                da.Fill(dt_Login, "table");
                DataRow dr = dt_Login.Tables[0].Rows[0];
                var response = new LoginResponseData
                {
                    token = CommonMethods.GetJwt(data.Email, dr["Password"].ToString(), Convert.ToInt32(dr["EmpID"].ToString())),
                    authenticated = true,
                    EmpId = dr["EmpID"].ToString(),
                    Name = dr["Name"].ToString(),
                    timestamp = DateTime.Now,
                    Email = data.Email
                };

                return response;
            }
        }

        //C- Add Employee Record  

        public Response Register(Employee emp)
        {
            Response _returnResponse = new Response();
            emp.Password = CommonMethods.Encrypt(emp.Password, true);
            try
            {

                SqlCommand cmd = new SqlCommand();

                string Query = @"INSERT INTO Emp (Name,Email,Password,Phone,Gender)  
                                               Values(@Name,@Email,@Password,@Phone,@Gender)";

                cmd = new SqlCommand(Query, con);
                cmd.Parameters.AddWithValue("@Name", emp.Name);
                cmd.Parameters.AddWithValue("@Email", emp.Email);
                cmd.Parameters.AddWithValue("@Password", emp.Password);
                cmd.Parameters.AddWithValue("@Phone", emp.Phone);
                cmd.Parameters.AddWithValue("@Gender", emp.Gender);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                _returnResponse.Message = "Record Added Successfully !";
                _returnResponse.IsSuccess = true;
            }
            catch (FaultException fex)
            {
                _returnResponse.Message = "Error";
                _returnResponse.IsSuccess = true;
            }
            return _returnResponse;
        }

        //Retrieve Data  
        //Retrive Record 
        public DatasetResponse GetEmployeeRecords()
        {
            DatasetResponse _res = new DatasetResponse();
            DataSet ds = new DataSet();
            try
            {
                string Query = "SELECT * FROM Emp";
                SqlDataAdapter sda = new SqlDataAdapter(Query, con);
                sda.Fill(ds);
                DataTable dt = ds.Tables[0];
                _res.data = CommonMethods.DataTableToJson(dt);
                _res.IsSuccess = true;
                _res.Message = "Success";


            }
            catch (FaultException fex)
            {
                throw new FaultException<string>("Error: " + fex);
            }
            return _res;
        }

        //Delete Record  
        public string DeleteRecords(Employee emp)
        {
            string result = "";
            SqlCommand cmd = new SqlCommand();
            string Query = "DELETE FROM Emp Where EmpID=@EmpID";
            cmd = new SqlCommand(Query, con);
            cmd.Parameters.AddWithValue("@EmpID", emp.EmpID);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
            result = "Record Deleted Successfully!";
            return result;
        }

        //Search Employee Record  
        public DataSet SearchEmployeeRecord(Employee emp)
        {
            DataSet ds = new DataSet();
            try
            {
                string Query = "SELECT * FROM Emp WHERE EmpID=@EmpID";

                SqlDataAdapter sda = new SqlDataAdapter(Query, con);
                sda.SelectCommand.Parameters.AddWithValue("@EmpID", emp.EmpID);
                sda.Fill(ds);
            }
            catch (FaultException fex)
            {
                throw new FaultException<string>("Error:  " + fex);
            }
            return ds;
        }

        //UPDATE RECORDS  
        //Update by Phone Roll   
        public string UpdateEmployeeContact(Employee emp)
        {
            string result = "";
            SqlCommand cmd = new SqlCommand();

            string Query = "UPDATE Emp SET Email=@Email,Phone=@Phone WHERE EmpID=@EmpID";

            cmd = new SqlCommand(Query, con);
            cmd.Parameters.AddWithValue("@EmpID", emp.EmpID);
            cmd.Parameters.AddWithValue("@Email", emp.Email);
            cmd.Parameters.AddWithValue("@Phone", emp.Phone);
            con.Open();
            cmd.ExecuteNonQuery();
            result = "Record Updated Successfully !";
            con.Close();

            return result;
        }


    }
}
