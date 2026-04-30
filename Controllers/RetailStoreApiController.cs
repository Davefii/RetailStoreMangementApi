using BussinessLayer;
using DataAccessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace RetailStroeManagmentAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/RetailStoreManagmentApi")]
    public class RetailStoreApiController : ControllerBase
    {
        //User Section
        [Authorize(Roles = "Admin")]
        [HttpGet("ListUsers", Name = "GetAllUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<IEnumerable<UserDTO>> GetAllUsers()
        {
            var userTable = Users.GetAllUsers();
            if (userTable.Count == 0)
            {
                return NotFound("No Users Found!");
            }
            return Ok(userTable);
        }

        [HttpGet("FindUserByID/{ID:int}", Name = "FindUserById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<UserDTO> FindUserByID(int ID)
        {
            if (ID < 1)
            {
                return BadRequest($"Not accepted ID {ID}");
            }

            // Ownership Logic: Only Admin can view any user, others can only view themselves
            if (!User.IsInRole("Admin"))
            {
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId) || currentUserId != ID)
                {
                    return Forbid();
                }
            }

            UserDTO userDTO = new UserDTO();

            if (DataUsers.GetUserByID(ID, ref userDTO))
            {
                return Ok(userDTO);
            }


            return NotFound($"User with ID {ID} not found.");
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("AddUser", Name = "AddUser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<UserDTO> AddUser(UserDTO NewUser)
        {
            if (NewUser == null ||
                string.IsNullOrEmpty(NewUser.UserName) ||
                string.IsNullOrEmpty(NewUser.Password))
            {
                return BadRequest("Invalid User data. Username and Password are required.");
            }

            // Create User object using constructor with DTO
            Users user = new Users(new global::DataAccessLayer.UserDTO(NewUser.ID, NewUser.UserName, NewUser.Password, NewUser.Permition, NewUser.IsActive));

            // Call AddUser function
            int newUserId = user.Addnewuser();

            if (newUserId <= 0)
            {
                return BadRequest("Failed to add user.");
            }

            NewUser.ID = newUserId;
            return CreatedAtAction(nameof(FindUserByID), new { ID = NewUser.ID }, NewUser);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateUser/{id}", Name = "UpdateUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<UserDTO> UpdateUser(int id, UserDTO updatedUser)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }
            if (updatedUser == null ||
                string.IsNullOrEmpty(updatedUser.UserName) ||
                string.IsNullOrEmpty(updatedUser.Password))
            {
                return BadRequest("Invalid User data. Username and Password are required.");
            }
            UserDTO existingUser = new UserDTO();
            if (!DataUsers.GetUserByID(id, ref existingUser))
            {
                return NotFound($"User with ID {id} not found.");
            }
            updatedUser.ID = id;
            if (DataUsers.UpdateUser(updatedUser))
            {
                return Ok(updatedUser);
            }
            return BadRequest("Failed to update user.");
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteUser/{id}", Name = "DeleteUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult DeleteUser(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }
            if (DataUsers.DeleteUser(id))
            {
                return Ok($"User with ID {id} has been deleted.");
            }
            return NotFound($"User with ID {id} not found. no rows deleted!");
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("ChangePassword/{id}", Name = "ChangePassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult ChangePassword(int id, [FromBody] string newPassword)
        {
            UserDTO userDTO = new UserDTO();
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }
            if (string.IsNullOrEmpty(newPassword))
            {
                return BadRequest("New password is required.");
            }
            if (Users.ChangepasswordAnyone(id, newPassword))
            {
                return Ok($"Password for User with ID {id} has been changed successfully.");
            }
            Debug.WriteLine(userDTO.Password.Length);
            Debug.WriteLine("[" + userDTO.Password + "]");
            return BadRequest("Failed to change password.");
        }


        //Supplier Section
        [Authorize(Roles = "Admin,StaffOrCashier,Viewer")]
        [HttpGet("Listsuppliers", Name = "GetAllSuppliers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<IEnumerable<SupplierDTO>> GetAllSuppliers()
        {
            List<SupplierDTO> SupplierList = Supplier.GetSuppliers();
            if (SupplierList.Count == 0)
            {
                return NotFound("No Suppliers Found!");
            }
            return Ok(SupplierList);
        }
        [Authorize(Roles = "Admin,StaffOrCashier")]
        //[HttpGet("{ID}", Name = "FindSupplierById")]
        [HttpGet("FindsuppliersByID/{ID:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<SupplierDTO> FindSupplierByID(int ID)
        {
            if (ID < 1)
            {
                return BadRequest($"Not accepted ID {ID}");
            }
            Supplier supplier = Supplier.Find(ID);
            if(supplier == null)
            {
                return NotFound($"Supplier with ID {ID} not found.");
            }
            SupplierDTO supplierDTO = supplier.SDTO;
            return Ok(supplierDTO);
        }
        [Authorize(Roles = "Admin,StaffOrCashier")]
        //[HttpGet("{SupplierName}", Name = "FindSupplierByName")]
        [HttpGet("Findsuppliers/by-name/{SupplierName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<SupplierDTO> FindSupplierByName(string SupplierName)
        {
            if (SupplierName == "")
            {
                return BadRequest($"Please Write Supplier Name");
            }
            Supplier supplier = Supplier.Find(SupplierName);
            if (supplier == null)
            {
                return NotFound($"Supplier with Name {SupplierName} not found.");
            }
            SupplierDTO supplierDTO = supplier.SDTO;
            return Ok(supplierDTO);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("Addsuppliers", Name = "AddSupplier")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<SupplierDTO> AddSupplier(SupplierDTO NewSupplier)
        {
            if (NewSupplier == null ||
                string.IsNullOrEmpty(NewSupplier.Supplier_Name) ||
                string.IsNullOrEmpty(NewSupplier.Phone_Number) ||
                string.IsNullOrEmpty(NewSupplier.Contact_Person) ||
                string.IsNullOrEmpty(NewSupplier.Email) ||
                string.IsNullOrEmpty(NewSupplier.Address)||
                string.IsNullOrEmpty(NewSupplier.Supplier_Name) &&
                string.IsNullOrEmpty(NewSupplier.Phone_Number) &&
                string.IsNullOrEmpty(NewSupplier.Contact_Person) &&
                string.IsNullOrEmpty(NewSupplier.Email) &&
                string.IsNullOrEmpty(NewSupplier.Address)
                )
            {
                return BadRequest("Invalid Supplier data.");
            }
            Supplier supplier = new Supplier( new SupplierDTO(NewSupplier.ID, NewSupplier.Supplier_Name, NewSupplier.Contact_Person, NewSupplier.Phone_Number, NewSupplier.Address, NewSupplier.Email, NewSupplier.Status));
            supplier.Save();
            NewSupplier.ID = supplier.ID;
            return CreatedAtAction("FindSupplierById", new {ID = NewSupplier.ID}, NewSupplier);
        }
        [Authorize(Roles = "Admin")]
        //here we use http put method for update
        [HttpPut("Updatesuppliers/{id}", Name = "UpdateSupplier")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<SupplierDTO> UpdateSupplier(int id, SupplierDTO updatedSupplier)
        {
            if (updatedSupplier == null ||
                string.IsNullOrEmpty(updatedSupplier.Supplier_Name) ||
                string.IsNullOrEmpty(updatedSupplier.Phone_Number) ||
                string.IsNullOrEmpty(updatedSupplier.Contact_Person) ||
                string.IsNullOrEmpty(updatedSupplier.Email) ||
                string.IsNullOrEmpty(updatedSupplier.Address) ||
                string.IsNullOrEmpty(updatedSupplier.Supplier_Name) &&
                string.IsNullOrEmpty(updatedSupplier.Phone_Number) &&
                string.IsNullOrEmpty(updatedSupplier.Contact_Person) &&
                string.IsNullOrEmpty(updatedSupplier.Email) &&
                string.IsNullOrEmpty(updatedSupplier.Address)
                )
            {
                return BadRequest("Invalid Supplier data.");
            }

            Supplier supplier = Supplier.Find(id);


            if (supplier == null)
            {
                return NotFound($"Supplier with ID {id} not found.");
            }


            supplier.Supplier_Name = updatedSupplier.Supplier_Name;
            supplier.Phone_Number = updatedSupplier.Phone_Number;
            supplier.Contact_Person = updatedSupplier.Contact_Person;
            supplier.Email = updatedSupplier.Email;
            supplier.Address = updatedSupplier.Address;
            supplier.Status = updatedSupplier.Status;
            supplier.Save();

            //we return the DTO not the full student object.
            return Ok(supplier.SDTO);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("Deletesuppliers/{id}", Name = "DeleteSupplier")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult DeleteSpplier(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            if (Supplier.DeleteSupplier(id))

                return Ok($"Supplier with ID {id} has been deleted.");
            else
                return NotFound($"Supplier with ID {id} not found. no rows deleted!");
        }
        [Authorize(Roles = "Admin,StaffOrCashier,Viewer")]
        //Product Section
        [HttpGet("ListProducts", Name = "GetAllProducts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<IEnumerable<ProductDTO>> GetAllProducts()
        {
            List<ProductDTO> ProdcutsList = Products.GetAllProducts();
            if (ProdcutsList.Count == 0)
            {
                return NotFound("No Products Found!");
            }
            return Ok(ProdcutsList);
        }
        [Authorize(Roles = "Admin")]
        [Authorize(Roles = "StaffOrCashier")]
        [HttpGet("FindproductsByID/{ID:int}", Name = "FindProductById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<ProductDTO> FindProductByID(int ID)
        {
            if (ID < 1)
            {
                return BadRequest($"Not accepted ID {ID}");
            }
            Products product = Products.Find(ID);
            if (product == null)
            {
                return NotFound($"Product with ID {ID} not found.");
            }
            ProductDTO ProductDTO = product.PDTO;
            return Ok(ProductDTO);
        }
        [Authorize(Roles = "Admin,StaffOrCashier")]
        [HttpGet("Findproducts/by-name/{ProductName}", Name = "FindProductByName")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<ProductDTO> FindProductByName(string ProductName)
        {
            if (string.IsNullOrWhiteSpace(ProductName))
            {
                return BadRequest($"Please Write Product Name");
            }
            Products product = Products.Find(ProductName);
            if (product == null)
            {
                return NotFound($"Product with Name {ProductName} not found.");
            }
            ProductDTO ProductDTO = product.PDTO;
            return Ok(ProductDTO);
        }
        [Authorize(Roles = "Admin,StaffOrCashier")]
        [HttpPost("Addproducts",Name = "AddProduct")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<ProductDTO> AddProduct(ProductDTO NewProduct)
        {
            if (NewProduct == null ||
                string.IsNullOrEmpty(NewProduct.Name) ||
                NewProduct.Price == 0 ||
                NewProduct.Quantity == 0 ||
                NewProduct.Supplier_ID == 0 ||
                string.IsNullOrEmpty(NewProduct.Name) &&
                NewProduct.Price == 0 &&
                NewProduct.Quantity == 0 &&
                NewProduct.Supplier_ID == 0)
            {
                return BadRequest("Invalid Product data.");
            }
            Products product = new Products(new ProductDTO(NewProduct.ID, NewProduct.Name,NewProduct.Price,NewProduct.Quantity,NewProduct.Supplier_ID));
            product.Save();
            NewProduct.ID = product.ID;
            return CreatedAtAction(nameof(FindSupplierByID), new { ID = NewProduct.ID }, NewProduct);
        }

        //here we use http put method for update
        [Authorize(Roles = "Admin")]
        [HttpPut("Updateproducts/{id}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<ProductDTO> UpdateProduct(int ID, ProductDTO updatedProduct)
        {
            if (updatedProduct == null ||
                string.IsNullOrEmpty(updatedProduct.Name) ||
                updatedProduct.Price <= 0 ||
                updatedProduct.Quantity <= 0 ||
                updatedProduct.Supplier_ID <= 0 ||
                string.IsNullOrEmpty(updatedProduct.Name) &&
                updatedProduct.Price <= 0 &&
                updatedProduct.Quantity <= 0 &&
                updatedProduct.Supplier_ID <= 0)
            {
                return BadRequest("Invalid Product data.");
            }

            Products product = Products.Find(ID);
            if (product == null)
            {
                return NotFound($"Product with ID {ID} not found.");
            }
            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            product.Quantity = updatedProduct.Quantity;
            product.Supplier_ID = updatedProduct.Supplier_ID;
            product.Save();

            return Ok(product.PDTO);
        }
        [Authorize(Roles = "Admin,StaffOrCashier")]
        [HttpDelete("Deleteproducts/{id}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult DeleteProduct(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            if (Products.DeleteProduct(id))

                return Ok($"Product with ID {id} has been deleted.");
            else
                return NotFound($"Product with ID {id} not found. no rows deleted!");
        }


        //Sales Section
        [Authorize(Roles = "Admin,StaffOrCashier,Viewer")]
        [HttpGet("Listsales",Name = "GetAllSales")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<IEnumerable<SalesDTO>> GetAllSales()
        {
            List<SalesDTO> SalesList = Sales.GetAllSeles();
            if (SalesList.Count == 0)
            {
                return NotFound("No Sales Found!");
            }
            return Ok(SalesList);
        }
        [Authorize(Roles = "Admin,StaffOrCashier,Viewer")]
        [HttpGet("FindsalesByID/{ID:int}", Name = "FindSaleById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<SalesDTO> FindSaleByID(int ID)
        {
            if (ID < 1)
            {
                return BadRequest($"Not accepted ID {ID}");
            }
            Sales sale = Sales.Find(ID);
            if (sale == null)
            {
                return NotFound($"Sale with ID {ID} not found.");
            }
            SalesDTO salesDTO = sale.SalesDTO;
            return Ok(salesDTO);
        }
        [Authorize(Roles = "Admin,StaffOrCashier")]
        [HttpPost("AddSale",Name = "AddSale")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<SalesDTO> AddSale(SalesDTO NewSale)
        {
            if (NewSale == null || NewSale.Product_ID == 0 && NewSale.Quantity == 0 && NewSale.User_ID == 0 || NewSale.Product_ID == 0 || NewSale.Quantity == 0 || NewSale.User_ID == 0)
            {
                return BadRequest("Invalid Sale data.");
            }
            Sales sale = new Sales(new SalesDTO(NewSale.ID, NewSale.Product_ID, NewSale.Quantity, NewSale.SaleDate, NewSale.User_ID, NewSale.TotalPrice));
            sale.Save();
            NewSale.ID = sale.ID;
            return CreatedAtAction(nameof(FindSaleByID), new { ID = NewSale.ID }, NewSale);
        }
        
        [HttpGet("sales/total", Name = "Get_Total_Sales")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<int> GetTotalSales()
        {
            int total = Sales.GetTotalSales();
            return Ok(total);
        }
    }
}
