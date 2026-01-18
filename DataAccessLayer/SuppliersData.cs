using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace DataAccessLayer
{
    public class SupplierDTO
    {
        public int ID { get; set; }
        public string Supplier_Name { get; set; }
        public string Contact_Person { get; set; }
        public string Phone_Number { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public bool Status { get; set; }
        public SupplierDTO()
        {
            this.ID = 0;
            this.Supplier_Name = "";
            this.Contact_Person = "";
            this.Phone_Number = "";
            this.Address = "";
            this.Email = "";
            this.Status = false;
        }
        public SupplierDTO(int ID, string Supplier_Name, string Contact_Person, string Phone_Number, string Address, string Email, bool Status)
        { 
            this.ID = ID;
            this.Supplier_Name = Supplier_Name;
            this.Contact_Person = Contact_Person;   
            this.Phone_Number = Phone_Number;
            this.Address = Address;
            this.Email = Email;
            this.Status = Status;
        }
    }
    public class SuppliersData
    {
        //static string _connectionString = "Server=.;Database=StudentsDB;User Id=sa;Password=123456;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;";
        public static List<SupplierDTO> GetAllSuppliers()
        {
            var SupplierList = new List<SupplierDTO>();

            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetAllSuppliersForApi", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    connection.Open();
                    try
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                SupplierList.Add(new SupplierDTO
                                (
                                    reader.GetInt32(reader.GetOrdinal("ID")),
                                    reader.GetString(reader.GetOrdinal("Supplier_Name")),
                                    reader.GetString(reader.GetOrdinal("Contact_Person")),
                                    reader.GetString(reader.GetOrdinal("Phone_Number")),
                                    reader.GetString(reader.GetOrdinal("Email")),
                                    reader.GetString(reader.GetOrdinal("Address")),
                                    reader.GetBoolean(reader.GetOrdinal("Status"))
                                ));
                            }
                        }
                    }
                    catch (Exception ex) { Debug.WriteLine(ex); }
                    finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                }
                return SupplierList;
            }
        }
        public static bool GetSupplierbyID(int ID,ref SupplierDTO supplier)
        {
            bool isFound = false;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("SP_GetSupplierByID", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID", ID);
                command.Parameters.Add(new SqlParameter("@Supplier_Name", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output });
                command.Parameters.Add(new SqlParameter("@Contact_Person", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output });
                command.Parameters.Add(new SqlParameter("@Phone_Number", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output });
                command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output });
                command.Parameters.Add(new SqlParameter("@Address", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output });
                command.Parameters.Add(new SqlParameter("@Status", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                var IsFoundParam = new SqlParameter("@IsFound", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                command.Parameters.Add(IsFoundParam);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    if (command.Parameters["@Supplier_Name"].Value != DBNull.Value) supplier.Supplier_Name = command.Parameters["@Supplier_Name"].Value.ToString();
                    if (command.Parameters["@Contact_Person"].Value != DBNull.Value) supplier.Contact_Person = command.Parameters["@Contact_Person"].Value.ToString();
                    if (command.Parameters["@Phone_Number"].Value != DBNull.Value) supplier.Phone_Number = command.Parameters["@Phone_Number"].Value.ToString();
                    if (command.Parameters["@Email"].Value != DBNull.Value) supplier.Email = command.Parameters["@Email"].Value.ToString();
                    if (command.Parameters["@Address"].Value != DBNull.Value) supplier.Address = command.Parameters["@Address"].Value.ToString();
                    if (command.Parameters["@Status"].Value != DBNull.Value) supplier.Status = Convert.ToBoolean(command.Parameters["@Status"].Value);
                    isFound = (IsFoundParam.Value != DBNull.Value) && Convert.ToBoolean(IsFoundParam.Value);
                }
                catch (Exception ex) { Debug.WriteLine(ex); isFound = false; }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return isFound;
            }
        }
        public static bool GetSupplierbyName(string Supplier_Name, ref SupplierDTO supplier)
        {
            bool isFound = false;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("SP_GetSupplierByName", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Supplier_Name", Supplier_Name);
                command.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Direction = ParameterDirection.Output });
                command.Parameters.Add(new SqlParameter("@Contact_Person", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output });
                command.Parameters.Add(new SqlParameter("@Phone_Number", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output });
                command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output });
                command.Parameters.Add(new SqlParameter("@Address", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output });
                command.Parameters.Add(new SqlParameter("@Status", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                var IsFoundParam = new SqlParameter("@IsFound", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                command.Parameters.Add(IsFoundParam);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    if (command.Parameters["@ID"].Value != DBNull.Value) supplier.ID = Convert.ToInt32(command.Parameters["@ID"].Value);
                    supplier.Supplier_Name = Supplier_Name;
                    if (command.Parameters["@Contact_Person"].Value != DBNull.Value) supplier.Contact_Person = command.Parameters["@Contact_Person"].Value.ToString();
                    if (command.Parameters["@Phone_Number"].Value != DBNull.Value) supplier.Phone_Number = command.Parameters["@Phone_Number"].Value.ToString();
                    if (command.Parameters["@Email"].Value != DBNull.Value) supplier.Email = command.Parameters["@Email"].Value.ToString();
                    if (command.Parameters["@Address"].Value != DBNull.Value) supplier.Address = command.Parameters["@Address"].Value.ToString();
                    if (command.Parameters["@Status"].Value != DBNull.Value) supplier.Status = Convert.ToBoolean(command.Parameters["@Status"].Value);
                    isFound = (IsFoundParam.Value != DBNull.Value) && Convert.ToBoolean(IsFoundParam.Value);
                }
                catch (Exception ex) { Debug.WriteLine(ex); isFound = false; }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return isFound;
            }
        }
        public static int AddSupplier(SupplierDTO supplier)
        {
            int? ID = null;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("Sp_AddNewSupplier", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Supplier_Name", supplier.Supplier_Name);
                command.Parameters.AddWithValue("@Contact_Person", supplier.Contact_Person);
                command.Parameters.AddWithValue("@Phone_Number", supplier.Phone_Number);
                command.Parameters.AddWithValue("@Email", supplier.Email);
                command.Parameters.AddWithValue("@Address", supplier.Address);
                command.Parameters.AddWithValue("@Status", supplier.Status);
                SqlParameter outputparameter = new SqlParameter("@NewID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputparameter);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    ID = (int)command.Parameters["@NewID"].Value;
                }
                catch (Exception ex) { Debug.WriteLine(ex); }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return ID ?? -1;
            }
        }

        public static bool UpdateSupplier(SupplierDTO supplier)
        {
            int RowAffected = 0;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("Sp_UpdateSupplier", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID", supplier.ID);
                command.Parameters.AddWithValue("@Supplier_Name", supplier.Supplier_Name);
                command.Parameters.AddWithValue("@Contact_Person", supplier.Contact_Person);
                command.Parameters.AddWithValue("@Phone_Number", supplier.Phone_Number);
                command.Parameters.AddWithValue("@Email", supplier.Email);
                command.Parameters.AddWithValue("@Address", supplier.Address);
                command.Parameters.AddWithValue("@Status", supplier.Status);
                SqlParameter returnparameter = new SqlParameter();
                returnparameter.Direction = ParameterDirection.ReturnValue;
                command.Parameters.Add(returnparameter);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    RowAffected = (int)returnparameter.Value;
                }
                catch (Exception ex) { Debug.WriteLine(ex); }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return RowAffected > 0;
            }
        }
        public static bool DeleteSupplier(int ID)
        {
            int RowAffected = 0;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("Sp_DeleteSupplier", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID", ID);
                SqlParameter returnparameter = new SqlParameter();
                returnparameter.Direction = ParameterDirection.ReturnValue;
                command.Parameters.Add(returnparameter);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    RowAffected = (int)returnparameter.Value;
                }
                catch (Exception ex) { Debug.WriteLine(ex); }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return RowAffected > 0;
            }
        }
        public static int GetTotalSuppliers()
        {
            int TotalSupplier = 0;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("GetTotalSuppliers", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                var TotalSupplierParam = new SqlParameter("@totalSuppliers", SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(TotalSupplierParam);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    if (TotalSupplierParam.Value != DBNull.Value) TotalSupplier = Convert.ToInt32(TotalSupplierParam.Value);
                }
                catch (Exception ex) { Debug.WriteLine(ex); TotalSupplier = 0; }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return TotalSupplier;
            }
        }
    }
}
