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
    public class ProductDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int Supplier_ID { get; set; }
        public ProductDTO()
        {
            this.ID = -1;
            this.Name = string.Empty;
            this.Price = 0;
            this.Supplier_ID = -1;
        }
        public ProductDTO(int ID, string Name, decimal Price, int Quantity, int Supplier_ID)
        {
            this.ID = ID;
            this.Name = Name;
            this.Price = Price;
            this.Quantity = Quantity;
            this.Supplier_ID = Supplier_ID;
        }
    }
    public class ProductData
    {

        public static List<ProductDTO> GetAllProducts()
        {
            var ProductList = new List<ProductDTO>();

            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetAllProductsForApi", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ProductList.Add(new ProductDTO
                                (
                                    reader.GetInt32(reader.GetOrdinal("ID")),
                                    reader.GetString(reader.GetOrdinal("Name")),
                                    reader.GetDecimal(reader.GetOrdinal("Price")),
                                    reader.GetInt32(reader.GetOrdinal("Quantity")),
                                    reader.GetInt32(reader.GetOrdinal("Supplier_ID"))
                                ));
                            }
                        }
                    }
                    catch (Exception ex) { Debug.WriteLine(ex); }
                    finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                }
                return ProductList;
            }
        }
        public static bool GetProductByID(int ID, ref ProductDTO productDTO)
        {
            bool isFound = false;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("SP_GetProductByID", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID", ID);
                var NameParam = new SqlParameter("@Name", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                var PriceParam = new SqlParameter("@Price", SqlDbType.SmallMoney) { Direction = ParameterDirection.Output };
                var QuantityParam = new SqlParameter("@Quantity", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var SupplierIDParam = new SqlParameter("@Supplier_ID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var IsFoundParam = new SqlParameter("@IsFound", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                command.Parameters.Add(NameParam);
                command.Parameters.Add(PriceParam);
                command.Parameters.Add(QuantityParam);
                command.Parameters.Add(SupplierIDParam);
                command.Parameters.Add(IsFoundParam);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    productDTO.ID = ID;
                    if (NameParam.Value != DBNull.Value) productDTO.Name = NameParam.Value.ToString();
                    if (PriceParam.Value != DBNull.Value) productDTO.Price = Convert.ToInt32(PriceParam.Value);
                    if (QuantityParam.Value != DBNull.Value) productDTO.Quantity = Convert.ToInt32(QuantityParam.Value);
                    if (SupplierIDParam.Value != DBNull.Value) productDTO.Supplier_ID = Convert.ToInt32(SupplierIDParam.Value);
                    isFound = (IsFoundParam.Value != DBNull.Value) && Convert.ToBoolean(IsFoundParam.Value);
                }
                catch (Exception ex) { Debug.WriteLine(ex); isFound = false; }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return isFound;
            }
        }
        public static bool getProductByName(string Name, ref ProductDTO productDTO)
        {
            bool isFound = false;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("Sp_GetProductByName", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Name", Name);
                var IDParam = new SqlParameter("@ID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var PriceParam = new SqlParameter("@Price", SqlDbType.SmallMoney) { Direction = ParameterDirection.Output };
                var QuantityParam = new SqlParameter("@Quantity", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var SupplierIDParam = new SqlParameter("@Supplier_ID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var IsFoundParam = new SqlParameter("@IsFound", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                command.Parameters.Add(IDParam);
                command.Parameters.Add(PriceParam);
                command.Parameters.Add(QuantityParam);
                command.Parameters.Add(SupplierIDParam);
                command.Parameters.Add(IsFoundParam);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    if (IDParam.Value != DBNull.Value) productDTO.ID = Convert.ToInt32(IDParam.Value);
                    productDTO.Name = Name;
                    if (PriceParam.Value != DBNull.Value) productDTO.Price = Convert.ToInt32(PriceParam.Value);
                    if (QuantityParam.Value != DBNull.Value) productDTO.Quantity = Convert.ToInt32(QuantityParam.Value);
                    if (SupplierIDParam.Value != DBNull.Value) productDTO.Supplier_ID = Convert.ToInt32(SupplierIDParam.Value);
                    isFound = (IsFoundParam.Value != DBNull.Value) && Convert.ToBoolean(IsFoundParam.Value);
                }
                catch (Exception ex) { Debug.WriteLine(ex); isFound = false; }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return isFound;
            }
        }
        public static int AddNewProduct(ProductDTO productDTO)
        {
            int? NewID = null;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("Sp_AddNewProduct", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Name", productDTO.Name);
                command.Parameters.AddWithValue("@Price", productDTO.Price);
                command.Parameters.AddWithValue("@Quantity", productDTO.Quantity);
                command.Parameters.AddWithValue("@Supplier_ID", productDTO.Supplier_ID);
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
        public static bool UpdateProduct(ProductDTO productDTO)
        {
            int RowAffected = 0;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("Sp_UpdateProduct", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID", productDTO.ID);
                command.Parameters.AddWithValue("@Name", productDTO.Name);
                command.Parameters.AddWithValue("@Price", productDTO.Price);
                command.Parameters.AddWithValue("@Quantity", productDTO.Quantity);
                command.Parameters.AddWithValue("@Supplier_ID", productDTO.Supplier_ID);
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
        public static bool DeleteProduct(int ID)
        {
            int RowAffected = 0;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("Sp_DeleteProduct", connection))
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
        public static int GetTotalProducts()
        {
            int TotalProducts = 0;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("GetTotalProducts", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                var TotalLowProductsParam = new SqlParameter("@totalProducts", SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(TotalLowProductsParam);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    if (TotalLowProductsParam.Value != DBNull.Value) TotalProducts = Convert.ToInt32(TotalLowProductsParam.Value);
                }
                catch (Exception ex) { Debug.WriteLine(ex); TotalProducts = 0; }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return TotalProducts;
            }
        }
        public static int GetTotalLowProducts()
        {
            int TotalLowProducts = 0;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("GetLowQuantityProducts", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                var TotalLowProductsParam = new SqlParameter("@LowQuantityProducts", SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(TotalLowProductsParam);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    if (TotalLowProductsParam.Value != DBNull.Value) TotalLowProducts = Convert.ToInt32(TotalLowProductsParam.Value);
                }
                catch (Exception ex) { Debug.WriteLine(ex); TotalLowProducts = 0; }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return TotalLowProducts;
            }
        }
    }
}
