using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class SalesDTO
    {
        public int ID { get; set; }
        public int Product_ID { get; set; }
        public int Quantity { get; set; }
        public DateTime SaleDate { get; set; }
        public int User_ID { get; set; }
        public int TotalPrice { get; set; }
        public SalesDTO()
        {
            this.ID = -1;
            this.Product_ID = -1;
            this.Quantity = 0;
            this.SaleDate = DateTime.MinValue;
            this.User_ID = 0;
        }
        public SalesDTO(int ID, int product_ID, int quantity, DateTime SaleDate, int user_ID, int TotalPrice)
        {
            this.ID = ID;
            this.Product_ID = product_ID;
            this.Quantity = quantity;
            this.SaleDate = SaleDate;
            this.User_ID = user_ID;
        }
    }
    public class SalesData
    {
        public static List<SalesDTO> GetAllSeles()
        {
            List<SalesDTO> salesList = new List<SalesDTO>();
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("GetAllSalesForApi", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            salesList.Add
                                (
                                    new SalesDTO
                                    (
                                        reader.GetInt32("ID"),
                                        reader.GetInt32("Product_ID"),
                                        reader.GetInt32("Quantity"),
                                        reader.GetDateTime("SaleDate"),
                                        reader.GetInt32("UserID"),
                                        reader.GetInt32("TotalPrice")
                                    )
                                );
                        }
                    }

                }
                catch (Exception ex) { Debug.WriteLine(ex); }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
            }
            return salesList;
        }
        public static int GetTotalSales()
        {
            int TotalSeles = 0;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("GETTotalSales", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                var TotalSelesParam = new SqlParameter("@TotalSeles", SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(TotalSelesParam);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    if (TotalSelesParam.Value != DBNull.Value) TotalSeles = Convert.ToInt32(TotalSelesParam.Value);
                }
                catch (Exception ex) { Debug.WriteLine(ex); TotalSeles = 0; }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return TotalSeles;
            }
        }
        public static bool getSaleByID(int ID, ref SalesDTO salesDTO)
        {
            bool isFound = false;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("SP_GetSaleByID", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID", ID);
                var Product_IDParam = new SqlParameter("@Product_ID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var QuantityParam = new SqlParameter("@Quantity", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var SaleDateParam = new SqlParameter("@SaleDate", SqlDbType.Date) { Direction = ParameterDirection.Output };
                var UserIDParam = new SqlParameter("@UserID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var TotalPriceParam = new SqlParameter("@TotalPrice", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var IsFoundParam = new SqlParameter("@IsFound", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                command.Parameters.Add(Product_IDParam);
                command.Parameters.Add(QuantityParam);
                command.Parameters.Add(SaleDateParam);
                command.Parameters.Add(UserIDParam);
                command.Parameters.Add(TotalPriceParam);
                command.Parameters.Add(IsFoundParam);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    salesDTO.ID = ID;
                    if (Product_IDParam.Value != DBNull.Value) salesDTO.Product_ID = Convert.ToInt32(Product_IDParam.Value);
                    if (SaleDateParam.Value != DBNull.Value) salesDTO.SaleDate = Convert.ToDateTime(SaleDateParam.Value);
                    if (QuantityParam.Value != DBNull.Value) salesDTO.Quantity = Convert.ToInt32(QuantityParam.Value);
                    if (UserIDParam.Value != DBNull.Value) salesDTO.User_ID = Convert.ToInt32(UserIDParam.Value);
                    if (TotalPriceParam.Value != DBNull.Value) salesDTO.TotalPrice = Convert.ToInt32(salesDTO.TotalPrice);
                    isFound = (IsFoundParam.Value != DBNull.Value) && Convert.ToBoolean(IsFoundParam.Value);
                }
                catch (Exception ex) { Debug.WriteLine(ex); isFound = false; }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return isFound;
            }
        }
        public static int AddNewSale(SalesDTO salesDTO)
        {
            int? NewID = null;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("Sp_AddNewSale", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Product_ID", salesDTO.Product_ID);
                command.Parameters.AddWithValue("@Quantity", salesDTO.Quantity);
                command.Parameters.AddWithValue("@SaleDate", salesDTO.SaleDate);
                command.Parameters.AddWithValue("@UserID", salesDTO.User_ID);
                command.Parameters.AddWithValue("@TotalPrice", salesDTO.TotalPrice);
                SqlParameter outputparameter = new SqlParameter("@NewID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputparameter);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    NewID = (int)command.Parameters["@NewID"].Value;
                }
                catch (Exception ex) { Debug.WriteLine(ex); }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return NewID ?? -1;
            }
        }
        public static bool UpdateSale(SalesDTO salesDTO)
        {
            int RowAffected = 0;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("Sp_UpdateSale", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID", salesDTO.ID);
                command.Parameters.AddWithValue("@Product_ID", salesDTO.Product_ID);
                command.Parameters.AddWithValue("@Product_ID", salesDTO.Product_ID);
                command.Parameters.AddWithValue("@Quantity", salesDTO.Quantity);
                command.Parameters.AddWithValue("@SaleDate", salesDTO.SaleDate);
                command.Parameters.AddWithValue("@UserID", salesDTO.User_ID);
                command.Parameters.AddWithValue("@TotalPrice", salesDTO.TotalPrice);
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
        public static bool DeleteSale(int ID)
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
        public static byte ReduceQuantity(int ID, int Quantity)
        {
            byte ReturnValue = 0;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("ReduceQuantityByID", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID", ID);
                command.Parameters.AddWithValue("@quantity", Quantity);
                SqlParameter outputparameter = new SqlParameter("@RowsAffected", SqlDbType.TinyInt)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputparameter);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    if (command.Parameters["@RowsAffected"].Value != DBNull.Value)
                    {
                        ReturnValue = Convert.ToByte(command.Parameters["@RowsAffected"].Value);
                    }

                }
                catch (Exception ex) { Debug.WriteLine(ex); }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return ReturnValue;
            }
        }
    }
}
