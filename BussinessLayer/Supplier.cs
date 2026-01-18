using DataAccessLayer;

namespace BussinessLayer
{
    public class Supplier
    {
        public enum enMode { AddNew = 0,Update = 1 }
        public enMode Mode;
        public SupplierDTO SDTO { get { return new SupplierDTO(this.ID, this.Supplier_Name, this.Contact_Person,
            this.Phone_Number, this.Address, this.Email, this.Status); } }
        public int ID { get; set; }
        public string Supplier_Name { get; set; }
        public string Contact_Person { get; set; }
        public string Phone_Number { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public bool Status { get; set; }
        public Supplier()
        {
            this.ID = 0;
            this.Supplier_Name = string.Empty;
            this.Contact_Person = string.Empty;
            this.Phone_Number = string.Empty;
            this.Address = string.Empty;
            this.Email = string.Empty;
            this.Status = false;
            Mode = enMode.AddNew;
        }
        public Supplier(SupplierDTO SDTO, enMode mode = enMode.AddNew)
        {
            this.ID = SDTO.ID;
            this.Supplier_Name = SDTO.Supplier_Name;
            this.Contact_Person = SDTO.Contact_Person;
            this.Phone_Number = SDTO.Phone_Number;
            this.Address = SDTO.Address;
            this.Email = SDTO.Email;
            this.Status = SDTO.Status;
        }
        public static Supplier Find(int ID)
        {
            SupplierDTO supplier = new SupplierDTO();
            bool isFound = SuppliersData.GetSupplierbyID(ID, ref supplier);
            if (isFound)
                return new Supplier(supplier, enMode.Update);
            else
                return null;
        }
        public static Supplier Find(string SupplierName)
        {
            SupplierDTO supplier = new SupplierDTO();
            bool isFound = SuppliersData.GetSupplierbyName(SupplierName, ref supplier);
            if (isFound)
                return new Supplier(supplier, enMode.Update);
            else
                return null;
        }
        public static List<SupplierDTO> GetSuppliers()
        {
            return SuppliersData.GetAllSuppliers();
        }
        private bool _AddSupplier()
        {
            this.ID = SuppliersData.AddSupplier(SDTO);
            return (this.ID != -1);
        }
        private bool _UpateSupplier()
        {
            return SuppliersData.UpdateSupplier(SDTO);
        }
        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddSupplier())
                    {

                        Mode = enMode.Update;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case enMode.Update:

                    return _UpateSupplier();

            }

            return false;
        }
        public static bool DeleteSupplier(int ID)
        {
            return SuppliersData.DeleteSupplier(ID);
        }
        public static int GelTotalSuppliers()
        {
            return SuppliersData.GetTotalSuppliers();
        }
    }
}
