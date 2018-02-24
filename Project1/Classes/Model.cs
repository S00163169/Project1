using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Net.Mail;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System.Threading;
using System.Diagnostics;
using Stripe;

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

    public class Category
    {
        public string Name { get; set; }
        public string ID { get; set; }

        public Category()
        {

        }

        public Category(string _name, string _id) : this()
        {
            this.Name = _name;
            this.ID = _id;
        }
    }

    public class DatabaseControl
    {
        static string Db = ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
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

            string db = ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;

            using (SqlConnection boothtest = new SqlConnection(db))
            {
                //"Server=tcp:boothserver.database.windows.net,1433;" + "Initial Catalog=boothtest;Persist Security Info=False;" + "User ID=S00163774;Password=BOOTHserver%163774;" + "MultipleActiveResultSets=False;Encrypt=True;" + "TrustServerCertificate=False;Connection Timeout=30;"

                using (SqlCommand ProductsCMD = new SqlCommand("ReturnProducts", boothtest))
                {
                    ProductsCMD.CommandType = CommandType.StoredProcedure;

                    boothtest.Open();

                    SqlDataReader myReader = ProductsCMD.ExecuteReader();

                    products = new List<Product>();

                    while (myReader.Read())
                    {
                        Product product = new Product
                        {
                            ProductID = myReader["ProductID"].ToString(),
                            Name = myReader["ProductName"].ToString(),
                            Description = myReader["ProductDesc"].ToString(),
                            Price = decimal.Parse(myReader["ProductPrice"].ToString()),
                            //product.ProductStock = int.Parse(myReader["ProductStock"].ToString());
                            ImageUrl = myReader["ProductURL"].ToString()
                        };
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

        static public User LogIn(string email, string password)
        {
            User user = new User();

            using (SqlConnection boothtest = new SqlConnection("Server=tcp:boothserver.database.windows.net,1433;" +
                "Initial Catalog=boothtest;Persist Security Info=False;" +
                "User ID=S00163774;Password=BOOTHserver%163774;" +
                "MultipleActiveResultSets=False;Encrypt=True;" +
                "TrustServerCertificate=False;Connection Timeout=30;"))
            {
                using (SqlCommand LogInCMD = new SqlCommand("LogIn", boothtest)) // Create New Command calling 'LogIn' stored procedure in boothtest database
                {
                    LogInCMD.CommandType = CommandType.StoredProcedure;

                    // Input Variables
                    SqlParameter inputEmail = LogInCMD.Parameters.Add("@ExUserEmail", SqlDbType.NVarChar, 30);
                    inputEmail.Direction = ParameterDirection.Input;

                    SqlParameter inputPassword = LogInCMD.Parameters.Add("@ExuserPassword", SqlDbType.NVarChar, 30);
                    inputPassword.Direction = ParameterDirection.Input;

                    inputEmail.Value = email;
                    inputPassword.Value = password;

                    boothtest.Open(); // Open Database Connection

                    SqlDataReader myReader = LogInCMD.ExecuteReader(); // Execute Command

                    while (myReader.Read())
                    {
                        user.UserID = int.Parse(myReader["UserID"].ToString());
                        user.UserFirstName = myReader["UserFirstName"].ToString();
                        user.UserSurname = myReader["UserSurname"].ToString();
                        user.UserMobileNumber = myReader["UserMobileNumber"].ToString();
                        user.UserEmail = myReader["UserEmail"].ToString();
                        user.UserType = myReader["UserType"].ToString();
                    }

                    myReader.Close(); // Close Command

                    boothtest.Close(); // Close Database Connection
                }
            }

            return user;
        }

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

        public static List<Category> GetCategories()
        {
            List<Category> categories = new List<Category>();

            string db = ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;

            using (SqlConnection boothtest = new SqlConnection(db))
            {
                //"Server=tcp:boothserver.database.windows.net,1433;" + "Initial Catalog=boothtest;Persist Security Info=False;" + "User ID=S00163774;Password=BOOTHserver%163774;" + "MultipleActiveResultSets=False;Encrypt=True;" + "TrustServerCertificate=False;Connection Timeout=30;"

                using (SqlCommand CategoryCMD = new SqlCommand("ReturnCategories", boothtest))
                {
                    CategoryCMD.CommandType = CommandType.StoredProcedure;

                    boothtest.Open();

                    SqlDataReader myReader = CategoryCMD.ExecuteReader();

                    categories = new List<Category>();

                    while (myReader.Read())
                    {
                        Category category = new Category
                        {
                            ID = myReader["ID"].ToString(),
                            Name = myReader["CategoryName"].ToString(),
                        };

                        categories.Add(category);
                    }

                    myReader.Close();
                    boothtest.Close();
                }
            }
            return categories;
        }

        public static List<string> TimeSlots(DateTime date)
        {
            List<int> takenTimes = GetTakenTimes(date);
            List<string> availableTimes = GetFreeTimes(takenTimes);
            return availableTimes;
        }

        static List<int> GetTakenTimes(DateTime date)
        {
            using (SqlConnection db = new SqlConnection(Db))
            {
                List<int> slotsTaken = new List<int>();

                using (SqlCommand DatesCMD = new SqlCommand("GetDates", db))
                {
                    DatesCMD.CommandType = CommandType.StoredProcedure;

                    SqlParameter inputDate = DatesCMD.Parameters.Add("@EDate", SqlDbType.DateTime);
                    inputDate.Direction = ParameterDirection.Input;

                    inputDate.Value = date;

                    db.Open();

                    SqlDataReader myReader = DatesCMD.ExecuteReader();

                    while (myReader.Read())
                    {
                        int slot = int.Parse(myReader["Slot"].ToString());

                        slotsTaken.Add(slot);
                    }

                    myReader.Close();
                    db.Close();

                    return slotsTaken;
                }
            }
        }

        static List<string> GetFreeTimes(List<int> list)
        {
            List<string> times = new List<string>();

            if (!list.Contains(1)) { times.Add("09:00"); }
            if (!list.Contains(2)) { times.Add("09:30"); }
            if (!list.Contains(3)) { times.Add("10:00"); }
            if (!list.Contains(4)) { times.Add("10:30"); }
            if (!list.Contains(5)) { times.Add("11:00"); }
            if (!list.Contains(6)) { times.Add("11:30"); }
            if (!list.Contains(7)) { times.Add("12:00"); }
            if (!list.Contains(8)) { times.Add("12:30"); }
            if (!list.Contains(9)) { times.Add("13:00"); }
            if (!list.Contains(10)) { times.Add("13:30"); }
            if (!list.Contains(11)) { times.Add("14:00"); }
            if (!list.Contains(12)) { times.Add("14:30"); }
            if (!list.Contains(13)) { times.Add("15:00"); }
            if (!list.Contains(14)) { times.Add("15:30"); }
            if (!list.Contains(15)) { times.Add("16:00"); }
            if (!list.Contains(16)) { times.Add("16:30"); }
            if (!list.Contains(17)) { times.Add("17:00"); }
            if (!list.Contains(18)) { times.Add("17:30"); }

            return times;
        }

        static public int InsertBooking(string Name, string Category, int Slot, string email, DateTime date)
        {
            int result = 0;
            using (SqlConnection db = new SqlConnection(Db))
            {
                using (SqlCommand InsertBookingCMD = new SqlCommand("InsertBooking", db))
                {
                    InsertBookingCMD.CommandType = CommandType.StoredProcedure;

                    SqlParameter inputEmail = InsertBookingCMD.Parameters.Add("@EEmail", SqlDbType.NVarChar);
                    inputEmail.Direction = ParameterDirection.Input;

                    SqlParameter inputName = InsertBookingCMD.Parameters.Add("@EBookingName", SqlDbType.NVarChar, 50);
                    inputName.Direction = ParameterDirection.Input;

                    SqlParameter inputSlot = InsertBookingCMD.Parameters.Add("@ESlot", SqlDbType.Float);
                    inputSlot.Direction = ParameterDirection.Input;

                    SqlParameter inputProcedure = InsertBookingCMD.Parameters.Add("@EProcedure", SqlDbType.NVarChar, 50);
                    inputProcedure.Direction = ParameterDirection.Input;

                    SqlParameter InputDate = InsertBookingCMD.Parameters.Add("@EDate", SqlDbType.DateTime);
                    InputDate.Direction = ParameterDirection.Input;

                    SqlParameter Result = InsertBookingCMD.Parameters.Add("Result", SqlDbType.Bit);
                    Result.Direction = ParameterDirection.ReturnValue;

                    inputEmail.Value = email;
                    inputName.Value = Name;
                    inputSlot.Value = Slot;
                    inputProcedure.Value = Category;
                    InputDate.Value = date;

                    db.Open();

                    InsertBookingCMD.ExecuteNonQuery();

                    result = Convert.ToInt32(Result.Value);

                    return result;
                }
            }
        }

        public static void EditBooking(string Id, int slot)
        {
            using (SqlConnection db = new SqlConnection(Db))
            {
                using (SqlCommand InsertBookingCMD = new SqlCommand("UpdateBooking", db))
                {
                    InsertBookingCMD.CommandType = CommandType.StoredProcedure;

                    SqlParameter inputID = InsertBookingCMD.Parameters.Add("@EBookingID", SqlDbType.NVarChar);
                    inputID.Direction = ParameterDirection.Input;

                    SqlParameter inputSlot = InsertBookingCMD.Parameters.Add("@ESlot", SqlDbType.Int);
                    inputSlot.Direction = ParameterDirection.Input;

                    inputID.Value = Id;
                    inputSlot.Value = slot;

                    db.Open();

                    InsertBookingCMD.ExecuteNonQuery(); 
                }
            }
        }

        public static int GetBookingSlots(string bookingTime)
        {
            if (bookingTime == "09:00") { return 1; }
            if (bookingTime == "09:30") { return 2; }
            if (bookingTime == "10:00") { return 3; }
            if (bookingTime == "10:30") { return 4; }
            if (bookingTime == "11:00") { return 5; }
            if (bookingTime == "11:30") { return 6; }
            if (bookingTime == "12:00") { return 7; }
            if (bookingTime == "12:30") { return 8; }
            if (bookingTime == "13:00") { return 9; }
            if (bookingTime == "13:30") { return 10; }
            if (bookingTime == "14:00") { return 11; }
            if (bookingTime == "14:30") { return 12; }
            if (bookingTime == "15:00") { return 13; }
            if (bookingTime == "15:30") { return 14; }
            if (bookingTime == "16:00") { return 15; }
            if (bookingTime == "16:30") { return 16; }
            if (bookingTime == "17:00") { return 17; }
            if (bookingTime == "17:30") { return 18; }
            else { return 0; }
        }

        public static List<Booking> GetEditBookings(string email)
        {
            List<Booking> bookings = new List<Booking>();
            using (SqlConnection db = new SqlConnection(Db))
            {
                using (SqlCommand InsertBookingCMD = new SqlCommand("ReturnBooking", db))
                {
                    InsertBookingCMD.CommandType = CommandType.StoredProcedure;

                    SqlParameter inputEmail = InsertBookingCMD.Parameters.Add("@EEmail", SqlDbType.NVarChar);
                    inputEmail.Direction = ParameterDirection.Input;

                    SqlParameter Result = InsertBookingCMD.Parameters.Add("Result", SqlDbType.Bit);
                    Result.Direction = ParameterDirection.ReturnValue;

                    inputEmail.Value = email;

                    db.Open();

                    SqlDataReader rdr = InsertBookingCMD.ExecuteReader();

                    bookings = new List<Booking>();

                    while (rdr.Read())
                    {
                        Booking booking = new Booking
                        {
                            ID = rdr["ID"].ToString(),
                            Name = rdr["BookingName"].ToString(),
                            Date = Convert.ToDateTime(rdr["Date"].ToString()),
                            Email = rdr["Email"].ToString(),
                            Slot = int.Parse(rdr["Slot"].ToString()),
                            Procedure = rdr["Treatment"].ToString()
                        };

                        bookings.Add(booking);
                    }
                }
                return bookings;
            }
        }

        public static void PaymentMethod(int amount, string description)
        {
            // var amount = 100;
            //var payment = Stripe.StripeCharge.(amount);
            try
            {
                //use nu-get Stripe
                //set TLS 1.2 in androuid settings

                StripeConfiguration.SetApiKey("sk_test_BEPrGyKARA5fbK1rcLbAixdd");

                var chargeOptions = new StripeChargeCreateOptions()
                {
                    Amount = amount,
                    Currency = "usd",
                    Description = description,
                    SourceTokenOrExistingSourceId = "tok_visa",
                    Metadata = new Dictionary<String, String>()
                    {
                        { "OrderId", "6735" }
                    }
                };

                var chargeService = new StripeChargeService();
                StripeCharge charge = chargeService.Create(chargeOptions);
            }
            // Use Stripe's library to make request

            catch (StripeException ex)
            {
                switch (ex.StripeError.ErrorType)
                {
                    case "card_error":
                        System.Diagnostics.Debug.WriteLine("   Code: " + ex.StripeError.Code);
                        System.Diagnostics.Debug.WriteLine("Message: " + ex.StripeError.Message);
                        break;
                    case "api_connection_error":
                        System.Diagnostics.Debug.WriteLine(" apic  Code: " + ex.StripeError.Code);
                        System.Diagnostics.Debug.WriteLine("apic Message: " + ex.StripeError.Message);
                        break;
                    case "api_error":
                        System.Diagnostics.Debug.WriteLine("api   Code: " + ex.StripeError.Code);
                        System.Diagnostics.Debug.WriteLine("api Message: " + ex.StripeError.Message);
                        break;
                    case "authentication_error":
                        System.Diagnostics.Debug.WriteLine(" auth  Code: " + ex.StripeError.Code);
                        System.Diagnostics.Debug.WriteLine("auth Message: " + ex.StripeError.Message);
                        break;
                    case "invalid_request_error":
                        System.Diagnostics.Debug.WriteLine(" invreq  Code: " + ex.StripeError.Code);
                        System.Diagnostics.Debug.WriteLine("invreq Message: " + ex.StripeError.Message);
                        break;
                    case "rate_limit_error":
                        System.Diagnostics.Debug.WriteLine("  rl Code: " + ex.StripeError.Code);
                        System.Diagnostics.Debug.WriteLine("rl Message: " + ex.StripeError.Message);
                        break;
                    case "validation_error":
                        System.Diagnostics.Debug.WriteLine(" val  Code: " + ex.StripeError.Code);
                        System.Diagnostics.Debug.WriteLine("val Message: " + ex.StripeError.Message);
                        break;
                    default:
                        // Unknown Error Type
                        break;
                }
            }
        }
    }


    //public class Basket
    //{
    //    #region Properties
    //    static List<Product> basketProducts = new List<Product>();
    //    #endregion

    //    static public List<Product> BasketProducts(Product[] products)
    //    {
    //        // Check if array isn't null
    //        if (products != null)
    //        {
    //            // Go through products and get all their details from the database and add the products to a new list
    //            for (int i = 0; i < products.Length; i++)
    //            {
    //                Product prod = DatabaseControl.GetBasketProducts(Convert.ToInt32(products[i].ProductID));
    //                prod.Quantity = products[i].Quantity;
    //                basketProducts.Add(prod);
    //            }
    //            // Return the new list of products in the basket
    //            return basketProducts;
    //        }
    //        else
    //        {
    //            // If the array is null return null
    //            return null;
    //        }

    //    }
    //}

    public class Checkout
    {
        public static void SendEmail(string emailUser)
        {
            MailMessage Msg = new MailMessage();
            Msg.From = new MailAddress("owenexampletest@gmail.com");
            Msg.To.Add(emailUser);
            Msg.Subject = "This is the Test Subject";
            Msg.Body = "This is the Test Body for the Project 300 email confirmation.";
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.Credentials = new System.Net.NetworkCredential("project300email@gmail.com", "Project300");
            smtp.EnableSsl = true;
            smtp.Send(Msg);
            //Console.WriteLine("Email Sent!");
        }

        public static void SendTextMessage(string number)
        {
            if (number.Length == 10)
            {
                number = number.Remove(0, 1);
            }

            // Your Account SID from twilio.com/console
            var accountSid = "AC0870a5e30dece345258735402d9f8e33";
            // Your Auth Token from twilio.com/console
            var authToken = "3a211e065304d39b9d5f699449fe7bcd";

            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
                to: new PhoneNumber("+353" + number),
                from: new PhoneNumber("+353861802018"),
                body: "Mo n fi message se testing fun project 300 mi. L'agbara Olorun o ma sise.");

            Console.WriteLine(message.Sid);
            Console.Write("Press any key to continue.");
            Console.ReadKey();
        }
    }

    //class Calendar
    //{
    //    // If modifying these scopes, delete your previously saved credentials
    //    // at ~/.credentials/calendar-dotnet-quickstart.json
    //    static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
    //    static string ApplicationName = "Google Calendar API .NET Quickstart";

    //    static void Main(string[] args)
    //    {
    //        UserCredential credential;

    //        using (var stream =
    //            new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
    //        {
    //            string credPath = System.Environment.GetFolderPath(
    //                System.Environment.SpecialFolder.Personal);
    //            credPath = Path.Combine(credPath, ".credentials/calendar-dotnet-quickstart.json");

    //            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
    //                GoogleClientSecrets.Load(stream).Secrets,
    //                Scopes,
    //                "user",
    //                CancellationToken.None,
    //                new FileDataStore(credPath, true)).Result;
    //            Console.WriteLine("Credential file saved to: " + credPath);
    //        }

    //        // Create Google Calendar API service.
    //        var service = new CalendarService(new BaseClientService.Initializer()
    //        {
    //            HttpClientInitializer = credential,
    //            ApplicationName = ApplicationName,
    //        });

    //        // Define parameters of request.
    //        EventsResource.ListRequest request = service.Events.List("primary");
    //        request.TimeMin = DateTime.Now;
    //        request.ShowDeleted = false;
    //        request.SingleEvents = true;
    //        request.MaxResults = 10;
    //        request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

    //        // List events.
    //        Events events = request.Execute();
    //        Console.WriteLine("Upcoming events:");
    //        if (events.Items != null && events.Items.Count > 0)
    //        {
    //            foreach (var eventItem in events.Items)
    //            {
    //                string when = eventItem.Start.DateTime.ToString();
    //                if (String.IsNullOrEmpty(when))
    //                {
    //                    when = eventItem.Start.Date;
    //                }
    //                Console.WriteLine("{0} ({1})", eventItem.Summary, when);
    //            }
    //        }
    //        else
    //        {
    //            Console.WriteLine("No upcoming events found.");
    //        }
    //        Console.Read();

    //    }
    //}

    public class User
    {
        public int UserID { get; set; }
        public string UserFirstName { get; set; }
        public string UserSurname { get; set; }
        public string UserMobileNumber { get; set; }
        public string UserEmail { get; set; }
        public string UserType { get; set; }
    }

    public class Booking
    {
        public string Email { get; set; }
        public string Procedure { get; set; }
        public int Slot { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string ID { get; set; }
    }
}