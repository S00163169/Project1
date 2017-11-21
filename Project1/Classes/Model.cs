using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Net.Mail;
using System.Text;

namespace Project1
{
    public class Model
    {
        #region Properties
        public string Firstname { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string UserID { get; set; }
        #endregion

        #region Constructors
        public Model()
        {
        }

        public Model(string firstname, string surname, string email, string password, string phoneNumber) : this()
        {
            this.Firstname = firstname;
            this.Surname = surname;
            this.Email = email;
            this.Password = password;
            this.Phone = phoneNumber;
        }
        #endregion
    }

    public class Product
    {
        #region Properties
        public string ImageUrl { get; set; }
        public string ProductID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        // This is for the basket products only
        public int Quantity { get; set; }
        #endregion

        #region Constructors
        public Product()
        {

        }

        public Product(string imageUrl, string productID, decimal price, string name) : this()
        {
            this.ProductID = productID;
            this.Name = name;
            this.Price = price;
            this.ImageUrl = imageUrl;
        }
        #endregion
    }

    public class DatabaseControl
    {
        static public void AddBasketItem()
        {
            #region DB Connection and Inputs
            string connectionString = ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;

            SqlConnection db = new SqlConnection(connectionString);

            SqlCommand cmd = new SqlCommand("AddToBasket", db);
            cmd.CommandType = CommandType.StoredProcedure;

            SqlParameter inputUserID = cmd.Parameters.Add("@UserID", SqlDbType.NVarChar, 255);
            inputUserID.Direction = ParameterDirection.Input;

            SqlParameter inputProductID = cmd.Parameters.Add("@ProductID", SqlDbType.NVarChar, 255);
            inputProductID.Direction = ParameterDirection.Input;

            SqlParameter inputQuantity = cmd.Parameters.Add("@Quantity", SqlDbType.Int);
            inputQuantity.Direction = ParameterDirection.Input;

            //string UserId = Version["UserID"].ToString();


            inputUserID.Value = "";
            inputProductID.Value = "";
            inputQuantity.Value = 1;

            #endregion
        }

        static public List<Product> ReturnProducts()
        {
            List<Product> products = new List<Product>();

            using (SqlConnection boothtest = new SqlConnection("Server=tcp:boothserver.database.windows.net,1433;" +
                "Initial Catalog=boothtest;Persist Security Info=False;" +
                "User ID=S00163774;Password=BOOTHserver%163774;" +
                "MultipleActiveResultSets=False;Encrypt=True;" +
                "TrustServerCertificate=False;Connection Timeout=30;"))
            {
                using (SqlCommand ProductsCMD = new SqlCommand("ReturnProducts", boothtest))
                {
                    ProductsCMD.CommandType = CommandType.StoredProcedure;

                    boothtest.Open();

                    SqlDataReader myReader = ProductsCMD.ExecuteReader();

                    products = new List<Product>();

                    while (myReader.Read())
                    {
                        Product product = new Product();

                        product.ProductID = myReader["ProductID"].ToString();
                        product.Name = myReader["ProductName"].ToString();
                        //product.ProductDesc = myReader["ProductDesc"].ToString();
                        product.Price = decimal.Parse(myReader["ProductPrice"].ToString());
                        //product.ProductStock = int.Parse(myReader["ProductStock"].ToString());
                        product.ImageUrl = myReader["ProductURL"].ToString();
                        //product.ProductCategoryID = int.Parse(myReader["ProductCategoryID"].ToString());

                        products.Add(product);
                    }

                    myReader.Close();
                    boothtest.Close();

                    return products;
                }
            }
        }

        static public Product GetBasketProducts(int productID)
        {
            Product product = new Product();

            using (SqlConnection boothtest = new SqlConnection("Server=tcp:boothserver.database.windows.net,1433;" +
                "Initial Catalog=boothtest;Persist Security Info=False;" +
                "User ID=S00163774;Password=BOOTHserver%163774;" +
                "MultipleActiveResultSets=False;Encrypt=True;" +
                "TrustServerCertificate=False;Connection Timeout=30;"))
            {

                using (SqlCommand BasketProds = new SqlCommand("BasketItem", boothtest))
                {
                    BasketProds.CommandType = CommandType.StoredProcedure;

                    SqlParameter inputEmail = BasketProds.Parameters.Add("@ProductID", SqlDbType.Int);
                    inputEmail.Direction = ParameterDirection.Input;

                    inputEmail.Value = productID;

                    boothtest.Open();

                    SqlDataReader myReader = BasketProds.ExecuteReader();

                    product = new Product();

                    while (myReader.Read())
                    {
                        product.ProductID = myReader["ProductID"].ToString();
                        product.Name = myReader["ProductName"].ToString();
                        product.Description = myReader["ProductDesc"].ToString();
                        product.Price = decimal.Parse(myReader["ProductPrice"].ToString());
                        product.ImageUrl = myReader["ProductURL"].ToString();
                    }

                    myReader.Close();
                    boothtest.Close();

                    return product;
                }
            }
        }

        static public void AddToBasket(string prodID,string UserID, string Quantity)
        {

        }

        //static public User LogIn(string email, string password)
        //{
        //    User user = new User();

        //    using (SqlConnection boothtest = new SqlConnection("Server=tcp:boothserver.database.windows.net,1433;" +
        //        "Initial Catalog=boothtest;Persist Security Info=False;" +
        //        "User ID=S00163774;Password=BOOTHserver%163774;" +
        //        "MultipleActiveResultSets=False;Encrypt=True;" +
        //        "TrustServerCertificate=False;Connection Timeout=30;"))
        //    {
        //        using (SqlCommand LogInCMD = new SqlCommand("LogIn", boothtest)) // Create New Command calling 'LogIn' stored procedure in boothtest database
        //        {
        //            LogInCMD.CommandType = CommandType.StoredProcedure;

        //            // Input Variables
        //            SqlParameter inputEmail = LogInCMD.Parameters.Add("@ExUserEmail", SqlDbType.NVarChar, 30);
        //            inputEmail.Direction = ParameterDirection.Input;

        //            SqlParameter inputPassword = LogInCMD.Parameters.Add("@ExuserPassword", SqlDbType.NVarChar, 30);
        //            inputPassword.Direction = ParameterDirection.Input;

        //            inputEmail.Value = email;
        //            inputPassword.Value = password;

        //            boothtest.Open(); // Open Database Connection

        //            SqlDataReader myReader = LogInCMD.ExecuteReader(); // Execute Command

        //            while (myReader.Read())
        //            {
        //                user.UserID = int.Parse(myReader["UserID"].ToString());
        //                user.UserFirstName = myReader["UserFirstName"].ToString();
        //                user.UserSurname = myReader["UserSurname"].ToString();
        //                user.UserMobileNumber = myReader["UserMobileNumber"].ToString();
        //                user.UserEmail = myReader["UserEmail"].ToString();
        //                user.UserType = myReader["UserType"].ToString();
        //            }

        //            myReader.Close(); // Close Command

        //            boothtest.Close(); // Close Database Connection
        //        }
        //    }

        //    return user;
        //}

        //static public string Register(string firstname, string surname, string mobilenumber, string email, string password)
        //{
        //    string emailExists = null;

        //    using (SqlConnection boothtest = new SqlConnection("Server=tcp:boothserver.database.windows.net,1433;" +
        //        "Initial Catalog=boothtest;Persist Security Info=False;" +
        //        "User ID=S00163774;Password=BOOTHserver%163774;" +
        //        "MultipleActiveResultSets=False;Encrypt=True;" +
        //        "TrustServerCertificate=False;Connection Timeout=30;"))
        //    {
        //        using (SqlCommand RegisterCMD = new SqlCommand("Register", boothtest))
        //        {
        //            RegisterCMD.CommandType = CommandType.StoredProcedure;

        //            // Input Variables
        //            SqlParameter inputFirstName = RegisterCMD.Parameters.Add("@UserFirstName", SqlDbType.NVarChar, 20);
        //            inputFirstName.Direction = ParameterDirection.Input;

        //            SqlParameter inputSurname = RegisterCMD.Parameters.Add("@UserSurname", SqlDbType.NVarChar, 30);
        //            inputSurname.Direction = ParameterDirection.Input;

        //            SqlParameter inputMobileNumber = RegisterCMD.Parameters.Add("@UserMobileNumber", SqlDbType.NVarChar, 10);
        //            inputMobileNumber.Direction = ParameterDirection.Input;

        //            SqlParameter inputEmail = RegisterCMD.Parameters.Add("@UserEmail", SqlDbType.NVarChar, 50);
        //            inputEmail.Direction = ParameterDirection.Input;

        //            SqlParameter inputPassword = RegisterCMD.Parameters.Add("@UserPassword", SqlDbType.VarChar, 50);
        //            inputPassword.Direction = ParameterDirection.Input;

        //            SqlParameter returnEmailExists = RegisterCMD.Parameters.Add("Result", SqlDbType.Int);
        //            returnEmailExists.Direction = ParameterDirection.ReturnValue;

        //            inputFirstName.Value = firstname;
        //            inputSurname.Value = surname;
        //            inputMobileNumber.Value = mobilenumber;
        //            inputEmail.Value = email;
        //            inputPassword.Value = password;

        //            boothtest.Open(); // Open Database Connection

        //            SqlDataReader myReader = RegisterCMD.ExecuteReader(); // Execute Command

        //            emailExists = Convert.ToString(returnEmailExists.Value);

        //            myReader.Close(); // Close Command

        //            boothtest.Close(); // Close Database Connection
        //        }

        //        return emailExists;
        //    }
        //}
    }

    public class Basket
    {
        #region Properties
        static List<Product> basketProducts = new List<Product>();
        #endregion

        static public List<Product> BasketProducts(Product[] products)
        {
            // Check if array isn't null
            if (products != null)
            {
                // Go through products and get all their details from the database and add the products to a new list
                for (int i = 0; i < products.Length; i++)
                {
                    Product prod = DatabaseControl.GetBasketProducts(Convert.ToInt32(products[i].ProductID));
                    prod.Quantity = products[i].Quantity;
                    basketProducts.Add(prod);
                }
                // Return the new list of products in the basket
                return basketProducts;
            }
            else
            {
                // If the array is null return null
                return null;
            }

        }
    }

    public class Checkout
    {

    }
}