using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLayer
{
    public class Products
    {
        public enum enMode { AddNew = 0, Update = 1 }
        public enMode Mode;
        public ProductDTO PDTO {  get { return new ProductDTO(this.ID, this.Name,this.Price,this.Quantity,this.Supplier_ID); }  }
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int Supplier_ID { get; set; }
        public Products()
        {
            this.ID = -1;
            this.Name = string.Empty;
            this.Price = 0;
            this.Supplier_ID = -1;
            Mode = enMode.AddNew;
        }
        public Products(ProductDTO productDTO)
        {
            this.ID = productDTO.ID;
            this.Name = productDTO.Name;
            this.Price = productDTO.Price;
            this.Quantity = productDTO.Quantity;
            this.Supplier_ID = productDTO.Supplier_ID;
            Mode = enMode.Update;
        }
        public static Products Find(int ID)
        {
            ProductDTO product = new ProductDTO();
            bool isFound = ProductData.GetProductByID(ID, ref product);
            if (isFound)
            {
                return new Products(product);
            }
            else
            {
                return null;
            }
        }
        public static Products Find(string ProductName)
        {
            ProductDTO product = new ProductDTO();
            bool isFound = ProductData.getProductByName(ProductName, ref product);
            if (isFound)
            {
                return new Products(product);
            }
            else
            {
                return null;
            }
        }
        public static List<ProductDTO> GetAllProducts()
        {
            return ProductData.GetAllProducts();
        }
        private bool _Addnewproduct()
        {
            this.ID = ProductData.AddNewProduct(PDTO);
            return (this.ID != -1);
        }
        private bool _UpdateProduct()
        {
            return ProductData.UpdateProduct(PDTO);
        }
        public static bool DeleteProduct(int ID)
        {
            return ProductData.DeleteProduct(ID);
        }
        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_Addnewproduct())
                    {

                        Mode = enMode.Update;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case enMode.Update:

                    return _UpdateProduct();

            }

            return false;
        }
        public static int getLowProduct()
        {
            return ProductData.GetTotalLowProducts();
        }
        public static int GetTotalProdutct()
        {
            return ProductData.GetTotalProducts();
        }
    }
}
