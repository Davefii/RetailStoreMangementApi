using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLayer
{
    public class Sales
    {
        public enum enMode { AddNew = 0, Update = 1 }
        public int ID { get; set; }
        public int Product_ID { get; set; }
        public int Quantity { get; set; }
        public DateTime SaleDate { get; set; }
        public int User_ID { get; set; }
        public decimal TotalPrice { get { Products product = Products.Find(this.Product_ID); return product.Price * Quantity; } }
        public enMode Mode;
        public SalesDTO SalesDTO { get { return new SalesDTO(this.ID, this.Product_ID, this.Quantity, this.SaleDate, this.User_ID, (int)this.TotalPrice); }  }
        public Sales()
        {
            this.ID = -1;
            this.Product_ID = -1;
            this.Quantity = 0;
            this.SaleDate = DateTime.MinValue;
            this.User_ID = 0;
            Mode = enMode.AddNew;
        }
        public Sales(SalesDTO SDTO)
        {
            this.ID = SDTO.ID;
            this.Product_ID = SDTO.Product_ID;
            this.Quantity = SDTO.Quantity;
            this.SaleDate = SDTO.SaleDate;
            this.User_ID = SDTO.User_ID;
            Mode = enMode.Update;
        }
        public static int GetTotalSales()
        {
            return SalesData.GetTotalSales();
        }
        public static List<SalesDTO> GetAllSeles()
        {
            return SalesData.GetAllSeles();
        }
        public static Sales Find(int ID)
        {
            SalesDTO SDTO = new SalesDTO();
            bool isFound = SalesData.getSaleByID(ID,ref SDTO);
            if (isFound)
                return new Sales(SDTO);
            else
                return null;
        }
        private byte _reducesqutityproduct(int ID, int Quantity)
        {
            return SalesData.ReduceQuantity(ID, Quantity);
        }
        private bool AddNewSale()
        {
            this.ID = SalesData.AddNewSale(this.SalesDTO);
            byte rowaffected = _reducesqutityproduct(this.Product_ID, this.Quantity);
            return (this.ID != -1 && rowaffected > 0);
        }
        private bool UpdateSale()
        {
            return SalesData.UpdateSale(this.SalesDTO);
        }
        public static bool DeleteSale(int ID)
        {
            return SalesData.DeleteSale(ID);
        }
        public bool Save()
        {
            switch (this.Mode)
            {
                case enMode.AddNew:
                    {
                        return AddNewSale();
                    }
                case enMode.Update:
                    {
                        return UpdateSale();
                    }
                default:
                    return false;
            }
        }
    }
}
