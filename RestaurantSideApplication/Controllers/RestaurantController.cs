using Microsoft.AspNetCore.Mvc;
using RestaurantSideApplication.Models;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;

namespace RestaurantSideApplication.Controllers
{
    public class RestaurantController : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ILogger<RestaurantController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        static int resId = 0;
        static List<FoodItem> foodItemsList;
        List<LoginDetails> Users = new List<LoginDetails>();
        public RestaurantController(IWebHostEnvironment hostEnvironment, ILogger<RestaurantController> logger, IHttpContextAccessor httpContextAccessor)
        {
            this._hostEnvironment = hostEnvironment;
            _logger = logger;
            this._httpContextAccessor = httpContextAccessor;


            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand("select * from RestaurantLoginDetails", conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            while (sr.Read())
            {
                LoginDetails user = new LoginDetails(sr["RestaurantName"].ToString(), sr["UserName"].ToString(),sr["Email"].ToString(), sr["Password"].ToString());
                Users.Add(user);
            }
        }
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult CreateAccount()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateAccount(SignUp signup )
        {

            var user = Users.Find(e => e.UserName == signup.UserName);
            var Restaurant = Users.Find(e => e.RestaurantName == signup.RestaurantName);
            if (user != null)
            {
                _logger.LogInformation("User:{0} already Exist, unable to create new Account", signup.UserName);
                ViewBag.UserName = "UserName already Exist";
                return View();
            }
            if (Restaurant != null)
            {
                ViewBag.UserName = "Restaurant Name already Exist";
                _logger.LogInformation("Restaurant:{0} already Exist, unable to create new Account", signup.RestaurantName);
                return View();
            }
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            
            SqlCommand cmd = new SqlCommand(String.Format("insert into RestaurantLoginDetails values('{0}','{1}','{2}','{3}')", signup.RestaurantName, signup.UserName, signup.Password,signup.Email), conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            _logger.LogInformation(String.Format("A new Account Created with UserName {0} and RestaurantName {1}", signup.UserName, signup.RestaurantName));

            SqlCommand cmd1 = new SqlCommand(String.Format("insert into Restaurants values('{0}','{1}','{2}')", signup.RestaurantName, signup.RestaurantImage,signup.Cuisine), conn);
            conn.Open();
            cmd1.ExecuteNonQuery();
            conn.Close();


            return RedirectToAction("Login");
        }


        public IActionResult Logout()
        {
            _logger.LogInformation("{0} logged out from the Restaurant {1}", _httpContextAccessor.HttpContext.Session.GetString("UserName"), _httpContextAccessor.HttpContext.Session.GetString("RestaurantName"));
            _httpContextAccessor.HttpContext.Session.Clear();
            return RedirectToAction("Index","Home");
        }

        public IActionResult Login()
        {
            _logger.LogInformation("Login Triggered");
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginDetails login)
        {
            var user = Users.Find(e => e.UserName == login.UserName);
            var restuarant = Users.Find(e => e.RestaurantName == login.RestaurantName);
            if (user == null)
            {
                ViewBag.NotExist = "NotExist";
                _logger.LogInformation("User does not Exist");
                return View();
            }
            if (restuarant == null)
            {
                ViewBag.ResNotExist = "NotExist";
                _logger.LogInformation("Restaurant does not Exist");
                return View();
            }
            foreach (var i in Users)
            {
                if (i.UserName == login.UserName && i.Password == login.Password)
                {
                    _httpContextAccessor.HttpContext.Session.SetString("RestaurantName", login.RestaurantName);
                    _httpContextAccessor.HttpContext.Session.SetString("UserName", login.UserName);
                    _logger.LogInformation(String.Format("User {0} logged into the Restaurant {1}", login.UserName, login.RestaurantName));

                    return RedirectToAction("DisplayOrders");
                }
            }
            _logger.LogError("Entered Incorrect Password");
            ViewBag.IncorrectPassword = "Incorrect Password";
            return View();
        }


        public IActionResult DisplayOrders()
        {
            if (_httpContextAccessor.HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out from the Restaurant {1}", _httpContextAccessor.HttpContext.Session.GetString("UserName"), _httpContextAccessor.HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Index","Home");
            }


            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("select * from PlacedOrderDetail PO inner join Restaurants R on R.Restaurant_Id = PO.RestaurantId  where R.Restaurant_Name = '{0}' order by PO.OrderTime desc", _httpContextAccessor.HttpContext.Session.GetString("RestaurantName")), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            List<Order> orderDetails = new List<Order>();
            while (sr.Read())
            {
                Order order = new Order((int)sr["InVoiceNo"], sr["UserName"].ToString(), sr["FoodItem"].ToString(), (int)sr["Quantity"], (int)sr["Price"], (DateTime)sr["OrderTime"], (int)sr["OrderNo"],sr["status"].ToString());
                orderDetails.Add(order);
            }

            conn.Close();


            return View("DisplayOrders",orderDetails);
        }

    
        public IActionResult CompletedOrder(int Id)
        {
            if (_httpContextAccessor.HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out, Owner of Restaurant {1}", _httpContextAccessor.HttpContext.Session.GetString("UserName"), _httpContextAccessor.HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Login");
            }

            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("select * from PlacedOrderDetail where OrderNo = '{0}'", Id), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();

            List<CompletedOrder> completedOrders = new List<CompletedOrder>();
            while (sr.Read())
            {
                CompletedOrder compOrder = new CompletedOrder((int)sr["InVoiceNo"], sr["UserName"].ToString(), sr["FoodItem"].ToString(), (int)sr["Quantity"], (int)sr["Price"],(int)sr["RestaurantId"],sr["status"].ToString());
                completedOrders.Add(compOrder);
            }
            conn.Close();
            DateTime utc = DateTime.UtcNow;

            DateTime temp = new DateTime(utc.Ticks, DateTimeKind.Utc);
            DateTime ist = TimeZoneInfo.ConvertTimeFromUtc(temp, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            foreach (var obj in completedOrders)
            {
                SqlCommand cmd1 = new SqlCommand(String.Format("insert into CompletedOrder values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')", obj.InVoiceNo, obj.CustomerName, obj.FoodItem, obj.Quantity, obj.Price,ist.ToString("yyyy-MM-dd HH:mm:ss"),obj.RestaurantId,"Delivered"), conn);
                conn.Open();
                _logger.LogDebug("Order Delivered to user {0}, Item {1} with Invoice No {2}", obj.CustomerName, obj.FoodItem, obj.InVoiceNo);
                cmd1.ExecuteNonQuery();
                conn.Close();

                SqlConnection conn2 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
                SqlCommand cmd3 = new SqlCommand(string.Format("select Restaurant_Name from restaurants where Restaurant_Id='{0}'", obj.RestaurantId), conn2);
                conn2.Open();
                string RestaurantName = "";
                SqlDataReader sr2 = cmd3.ExecuteReader();
                while (sr2.Read())
                {
                    RestaurantName = sr2["Restaurant_Name"].ToString();
                }

                conn2.Close();

                SqlConnection conn3 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
                SqlCommand cmd5 = new SqlCommand(string.Format("select Email from Users where UserName='{0}'", obj.CustomerName), conn3);
                conn3.Open();
                SqlDataReader sr3 = cmd5.ExecuteReader();
                string userMail = "";
                while (sr3.Read())
                {
                    userMail = sr3["Email"].ToString();
                }
                conn3.Close();
                

                SqlCommand cmd7 = new SqlCommand(string.Format("select * from Orders where InVoiceNo ='{0}'", obj.InVoiceNo), conn3);
                conn3.Open();
                SqlDataReader sr5 = cmd7.ExecuteReader();
                string address = "";
                string city = "";
                string state = "";
                string zipcode = "";
                string orderTime = "";
                while (sr5.Read())
                {
                    orderTime = sr5["OrderTime"].ToString();
                    address = sr5["Address"].ToString();
                    city = sr5["City"].ToString();
                    state = sr5["State"].ToString();
                    zipcode = sr5["ZipCode"].ToString();
                }
                conn3.Close();
                string fromMail = "update.justeat@gmail.com";
                string fromPassword = "vswveeeasgbytbyi";

                string fp = string.Format("<h2 style=\"color:orange; text-align:center; font-size:25px;\">JustEat</h2><hr/><p>Dear <span style=\"font-weight:bold;\">{0}</span>,</p><p>Greetings from JustEat<br/>Your order was delivered to your address. Thanks for choosing JustEat</p><p>Your Order Summary:</p><p>Invoice No: <span style=\"font-weight:bold;\">{1}</span></p><p>Restaurant: <span style=\"font-weight:bold;\">{2}</span></p><p>Order Placed at: <span style=\"font-weight:bold;\">{3}</span></p><p>Order Delivered at: <span style=\"font-weight:bold;\">{4}</span></p><p>Order Status: <span style=\"font-weight:bold; color:#3c9961;\"> Delivered</span></p><p>Delivery To:</p><p><span style=\"font-weight:bold; text-transform: uppercase;\">{5}</span></p><p>{6}, {7}<br/> {8} {9}, India</p> </body></html>", obj.CustomerName, obj.InVoiceNo, RestaurantName, orderTime, ist.ToString("dd-MM-yyyy HH:mm:ss"), obj.CustomerName, address, city, state,zipcode);
                string lp1 = "<table style=\"border-collapse: collapse; width:65vw;\"><tr ><th style=\"background: #eee; border: 1px solid #777; padding: 0.5rem; text-align: center;\">Item Name</th><th style=\"background: #eee; border: 1px solid #777; padding: 0.5rem; text-align: center;\">Quantity</th><th style=\"background: #eee; border: 1px solid #777; padding: 0.5rem; text-align: center;\">Price</th></tr>";
                int totalPrice = obj.Price*obj.Quantity;
                
                    lp1 += string.Format("<tr ><td style=\" border: 1px solid #777; padding: 0.5rem; text-align: center;\">{0}</td><td style=\" border: 1px solid #777; padding: 0.5rem; text-align: center;\">{1}</td><td style=\" border: 1px solid #777; padding: 0.5rem; text-align: center;\">₹ {2}</td></tr>", obj.FoodItem,obj.Quantity,obj.Price);
                
                string lp2 = "</table>";
                string lp3 = string.Format("<p style=\"width:65vw; color:#3c9961; text-align:end; background-color:#f0f5f1; padding:7px; font-weight:bold;\">Order Total :  <span style=\"padding-left:10px;\">₹ {0}</span></p><hr/><p style=\"padding:10px;\">Hope you have a good experience with us. Thankyou and have a nice day</p><hr/>", totalPrice);
                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromMail);
                //string n = "j\"fdf\"";
                message.Subject = String.Format("Your JustEat Order #{0} was delivered superfast", obj.InVoiceNo);
                message.To.Add(new MailAddress(userMail));
                message.Body = string.Format("<html><body>{0}{1}{2}{3} </body></html>", fp, lp1, lp2, lp3);
                message.IsBodyHtml = true;

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, fromPassword),
                    EnableSsl = true,
                };

                smtpClient.Send(message);


                SqlCommand cmd2 = new SqlCommand(String.Format("delete from PlacedOrderDetail where OrderNo = '{0}'", Id), conn);
                conn.Open();
                cmd2.ExecuteNonQuery();
                conn.Close();
            }
            SqlConnection conn4 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd4 = new SqlCommand(String.Format("update Orders set status='Delivered' where OrderId = '{0}'", Id), conn4);
            conn4.Open();
            cmd4.ExecuteNonQuery();
            conn4.Close();



            return RedirectToAction("DeliverableOrders");

            
        }

        public IActionResult AcceptOrder(int Id)
        {
            if (_httpContextAccessor.HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out, Owner of Restaurant {1}", _httpContextAccessor.HttpContext.Session.GetString("UserName"), _httpContextAccessor.HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Login");
            }

            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("update PlacedOrderDetail set status ='{0}' where OrderNo = '{1}'","Accepted", Id), conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            return RedirectToAction("DisplayOrders");
        }
        
        public IActionResult PreparingOrders()
        {
            if (HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out from theRestaurant {1}", HttpContext.Session.GetString("UserName"), HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Login");
            }
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("select * from PlacedOrderDetail PO inner join Restaurants R on R.Restaurant_Id = PO.RestaurantId  where R.Restaurant_Name = '{0}' order by PO.OrderTime desc", _httpContextAccessor.HttpContext.Session.GetString("RestaurantName")), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            List<Order> orderDetails = new List<Order>();
            while (sr.Read())
            {
                Order order = new Order((int)sr["InVoiceNo"], sr["UserName"].ToString(), sr["FoodItem"].ToString(), (int)sr["Quantity"], (int)sr["Price"], (DateTime)sr["OrderTime"], (int)sr["OrderNo"], sr["status"].ToString());
                orderDetails.Add(order);
            }
            conn.Close();
            return View(orderDetails);
        }
        public IActionResult PreparedOrder(int Id)
        {
            if (_httpContextAccessor.HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out, Owner of Restaurant {1}", _httpContextAccessor.HttpContext.Session.GetString("UserName"), _httpContextAccessor.HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Login");
            }

            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("update PlacedOrderDetail set status ='{0}' where OrderNo = '{1}'", "Prepared", Id), conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            return RedirectToAction("PreparingOrders");

        }
        public IActionResult DeliverableOrders()
        {
            if (HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out from theRestaurant {1}", HttpContext.Session.GetString("UserName"), HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Login");
            }
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("select * from PlacedOrderDetail PO inner join Restaurants R on R.Restaurant_Id = PO.RestaurantId  where R.Restaurant_Name = '{0}' order by PO.OrderTime desc", _httpContextAccessor.HttpContext.Session.GetString("RestaurantName")), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            List<Order> orderDetails = new List<Order>();
            while (sr.Read())
            {
                Order order = new Order((int)sr["InVoiceNo"], sr["UserName"].ToString(), sr["FoodItem"].ToString(), (int)sr["Quantity"], (int)sr["Price"], (DateTime)sr["OrderTime"], (int)sr["OrderNo"], sr["status"].ToString());
                orderDetails.Add(order);
            }
            conn.Close();
            return View(orderDetails);
        }
        public IActionResult CancelOrder(int Id)
        {
            if (HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out from theRestaurant {1}", HttpContext.Session.GetString("UserName"), HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Login");
            }
            ViewBag.Id = Id;
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("select * from PlacedOrderDetail PO inner join Restaurants R on R.Restaurant_Id = PO.RestaurantId  where R.Restaurant_Name = '{0}' AND PO.OrderNo='{1}'", _httpContextAccessor.HttpContext.Session.GetString("RestaurantName"), Id), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            Order orderDetails = new Order();
            while (sr.Read())
            {
                orderDetails = new Order((int)sr["InVoiceNo"], sr["UserName"].ToString(), sr["FoodItem"].ToString(), (int)sr["Quantity"], (int)sr["Price"], (DateTime)sr["OrderTime"], (int)sr["OrderNo"], sr["status"].ToString());

            }
            conn.Close();

            SqlCommand cmd1 = new SqlCommand(String.Format("select * from Orders where OrderId ='{0}'", Id), conn);
            conn.Open();
            SqlDataReader sr1 = cmd1.ExecuteReader();
       
            while (sr1.Read())
            {
                ViewBag.address = sr1["Address"].ToString();
            }
            conn.Close();

            return View(orderDetails);
        }
        public IActionResult CancelConfirmed(int Id)
        {
            if (HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out from theRestaurant {1}", HttpContext.Session.GetString("UserName"), HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Login");
            }
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("select * from PlacedOrderDetail where OrderNo = '{0}'", Id), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();

            List<CompletedOrder> completedOrders = new List<CompletedOrder>();
            while (sr.Read())
            {
                CompletedOrder compOrder = new CompletedOrder((int)sr["InVoiceNo"], sr["UserName"].ToString(), sr["FoodItem"].ToString(), (int)sr["Quantity"], (int)sr["Price"], (int)sr["RestaurantId"], sr["status"].ToString());
                completedOrders.Add(compOrder);
            }
            conn.Close();
            DateTime utc = DateTime.UtcNow;

            DateTime temp = new DateTime(utc.Ticks, DateTimeKind.Utc);
            DateTime ist = TimeZoneInfo.ConvertTimeFromUtc(temp, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            foreach (var obj in completedOrders)
            {
                SqlCommand cmd1 = new SqlCommand(String.Format("insert into CompletedOrder values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')", obj.InVoiceNo, obj.CustomerName, obj.FoodItem, obj.Quantity, obj.Price, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), obj.RestaurantId, "Cancelled"), conn);
                conn.Open();
                _logger.LogDebug("Order Delivered to user {0}, Item {1} with Invoice No {2}", obj.CustomerName, obj.FoodItem, obj.InVoiceNo);
                cmd1.ExecuteNonQuery();
                conn.Close();

                SqlConnection conn2 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
                SqlCommand cmd3 = new SqlCommand(string.Format("select Restaurant_Name from restaurants where Restaurant_Id='{0}'", obj.RestaurantId), conn2);
                conn2.Open();
                string RestaurantName = "";
                SqlDataReader sr2 = cmd3.ExecuteReader();
                while (sr2.Read())
                {
                    RestaurantName = sr2["Restaurant_Name"].ToString();
                }

                conn2.Close();

                SqlConnection conn3 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
                SqlCommand cmd5 = new SqlCommand(string.Format("select Email from Users where UserName='{0}'", obj.CustomerName), conn3);
                conn3.Open();
                SqlDataReader sr3 = cmd5.ExecuteReader();
                string userMail = "";
                while (sr3.Read())
                {
                    userMail = sr3["Email"].ToString();
                }
                conn3.Close();


                SqlCommand cmd7 = new SqlCommand(string.Format("select * from Orders where InVoiceNo ='{0}'", obj.InVoiceNo), conn3);
                conn3.Open();
                SqlDataReader sr5 = cmd7.ExecuteReader();
                string address = "";
                string city = "";
                string state = "";
                string zipcode = "";
                string orderTime = "";
                while (sr5.Read())
                {
                    orderTime = sr5["OrderTime"].ToString();
                    address = sr5["Address"].ToString();
                    city = sr5["City"].ToString();
                    state = sr5["State"].ToString();
                    zipcode = sr5["ZipCode"].ToString();
                }
                conn3.Close();
                string fromMail = "update.justeat@gmail.com";
                string fromPassword = "vswveeeasgbytbyi";

                string fp = string.Format("<h2 style=\"color:orange; text-align:center; font-size:25px;\">JustEat</h2><hr/><p>Dear <span style=\"font-weight:bold;\">{0}</span>,</p><p>We regret to inform you that due to some unavoidable circumstances your order was cancelled by <span style=\"font-weight:bold;\">{1}</span>.</p><p>Your Order Summary:</p><p>Invoice No: <span style=\"font-weight:bold;\">{2}</span></p><p>Restaurant: <span style=\"font-weight:bold;\">{3}</span></p><p>Order Placed at: <span style=\"font-weight:bold;\">{4}</span></p><p>Order Cancelled at: <span style=\"font-weight:bold;\">{5}</span></p><p>Order Status: <span style=\"font-weight:bold; color:red;\"> Cancelled</span></p><p>Delivery To:</p><p><span style=\"font-weight:bold; text-transform: uppercase;\">{6}</span></p><p>{7}, {8}<br/> {9} {10}, India</p> </body></html>", obj.CustomerName,RestaurantName, obj.InVoiceNo, RestaurantName, orderTime, ist.ToString("dd-MM-yyyy HH:mm:ss"), obj.CustomerName, address, city, state, zipcode);
                string lp1 = "<table style=\"border-collapse: collapse; width:65vw;\"><tr ><th style=\"background: #eee; border: 1px solid #777; padding: 0.5rem; text-align: center;\">Item Name</th><th style=\"background: #eee; border: 1px solid #777; padding: 0.5rem; text-align: center;\">Quantity</th><th style=\"background: #eee; border: 1px solid #777; padding: 0.5rem; text-align: center;\">Price</th></tr>";
                int totalPrice = obj.Price * obj.Quantity;

                lp1 += string.Format("<tr ><td style=\" border: 1px solid #777; padding: 0.5rem; text-align: center;\">{0}</td><td style=\" border: 1px solid #777; padding: 0.5rem; text-align: center;\">{1}</td><td style=\" border: 1px solid #777; padding: 0.5rem; text-align: center;\">₹ {2}</td></tr>", obj.FoodItem, obj.Quantity, obj.Price);

                string lp2 = "</table>";
                string lp3 = string.Format("<p style=\"width:65vw; color:#3c9961; text-align:end; background-color:#f0f5f1; padding:7px; font-weight:bold;\">Order Total :  <span style=\"padding-left:10px;\">₹ {0}</span></p><hr/><p style=\"padding:10px;\">If you already paid then your amount will be refunded within 24 hours. Thankyou for choosing JustEat and have a nice day</p><hr/>", totalPrice);
                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromMail);
                //string n = "j\"fdf\"";
                message.Subject = String.Format("Your JustEat Order #{0} was cancelled", obj.InVoiceNo);
                message.To.Add(new MailAddress(userMail));
                message.Body = string.Format("<html><body>{0}{1}{2}{3} </body></html>", fp, lp1, lp2, lp3);
                message.IsBodyHtml = true;

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, fromPassword),
                    EnableSsl = true,
                };

                smtpClient.Send(message);




                SqlCommand cmd2 = new SqlCommand(String.Format("delete from PlacedOrderDetail where OrderNo = '{0}'", Id), conn);
                conn.Open();
                cmd2.ExecuteNonQuery();
                conn.Close();
            }
            SqlConnection conn4 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd4 = new SqlCommand(String.Format("update Orders set status='Cancelled' where OrderId = '{0}'", Id), conn4);
            conn4.Open();
            cmd4.ExecuteNonQuery();
            conn4.Close();

            return RedirectToAction("DisplayOrders");
        }
        public IActionResult AddFoodItem()
        {
            if (HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out from theRestaurant {1}", HttpContext.Session.GetString("UserName"), HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Login");
            }
            return View();

        }

        [HttpPost]
        public IActionResult AddFoodItem(FoodItem foodItem)
        {
            if (HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out from theRestaurant {1}", HttpContext.Session.GetString("UserName"), HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Login");
            }
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");

            SqlCommand sqlCommand = new SqlCommand(String.Format("select Restaurant_Id from Restaurants where Restaurant_Name='{0}'", HttpContext.Session.GetString("RestaurantName")), conn);
            conn.Open();
            SqlDataReader sr = sqlCommand.ExecuteReader();
            while (sr.Read())
            {
                resId = (int)sr["Restaurant_Id"];
            }

            conn.Close();

            SqlCommand sqlCommand1 = new SqlCommand(String.Format("insert into Food values('{0}','{1}','{2}','{3}','{4}')", foodItem.FoodItemImage, foodItem.FoodItemName, foodItem.Price, resId,foodItem.Type), conn);
            conn.Open();
            sqlCommand1.ExecuteNonQuery();
            conn.Close();

            return RedirectToAction("DisplayFoodItems");
        }

        public IActionResult DisplayFoodItems()
        {
            
            if (HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out from theRestaurant {1}", HttpContext.Session.GetString("UserName"), HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Login");
            }
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");

            SqlCommand sqlCommand = new SqlCommand(String.Format("select Restaurant_Id from Restaurants where Restaurant_Name='{0}'", HttpContext.Session.GetString("RestaurantName")), conn);
            conn.Open();
            SqlDataReader sr = sqlCommand.ExecuteReader();
            while (sr.Read())
            {
                resId = (int)sr["Restaurant_Id"];
            }

            conn.Close();
            List<FoodItem> foodItems = new List<FoodItem>();
            SqlConnection conn1 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");

            SqlCommand sqlCommand1 = new SqlCommand(String.Format("select * from Food where Restaurant_Id='{0}'", resId), conn);
            conn.Open();
            SqlDataReader sr1 = sqlCommand1.ExecuteReader();
            while (sr1.Read())
            {
                foodItems.Add(new FoodItem((int)sr1["Id"],sr1["Food_Item"].ToString(),sr1["Food_Image"].ToString(),(int)sr1["Price"]));
            }

            conn1.Close();

            return View(foodItems);
        }

        public IActionResult EditFoodItem(int id)
        {
            if (HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out from theRestaurant {1}", HttpContext.Session.GetString("UserName"), HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Login");
            }
            foodItemsList = new List<FoodItem>();
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");

            SqlCommand sqlCommand = new SqlCommand(String.Format("select Restaurant_Id from Restaurants where Restaurant_Name='{0}'", HttpContext.Session.GetString("RestaurantName")), conn);
            conn.Open();
            SqlDataReader sr = sqlCommand.ExecuteReader();
            while (sr.Read())
            {
                resId = (int)sr["Restaurant_Id"];
            }

            conn.Close();

            SqlConnection conn1 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");

            SqlCommand sqlCommand1 = new SqlCommand(String.Format("select * from Food where Restaurant_Id='{0}'", resId), conn);
            conn.Open();
            SqlDataReader sr1 = sqlCommand1.ExecuteReader();
            while (sr1.Read())
            {
                foodItemsList.Add(new FoodItem((int)sr1["Id"], sr1["Food_Item"].ToString(), sr1["Food_Image"].ToString(), (int)sr1["Price"], sr1["FoodType"].ToString()));
            }

            conn1.Close();
            var item = foodItemsList.Find(e => e.FoodItemId == id);
            return View(item);
        }
        [HttpPost]
        public IActionResult EditFoodItem(int id, FoodItem f)
        {
            if (HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out from theRestaurant {1}", HttpContext.Session.GetString("UserName"), HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Login");
            }
            var fi = foodItemsList.Find(e => e.FoodItemId == id);
            if (fi != null)
            {
                SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
                SqlCommand cmd = new SqlCommand(String.Format("update Food set Food_Image='{0}',Food_Item='{1}',Price='{2}',FoodType='{3}' where Id = '{4}'", f.FoodItemImage, f.FoodItemName, f.Price, f.Type, id), conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            return RedirectToAction("DisplayFoodItems");
        }

        public IActionResult DeleteFoodItem(int id)
        {
            if (HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out from theRestaurant {1}", HttpContext.Session.GetString("UserName"), HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Login");
            }
            foodItemsList = new List<FoodItem>();
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");

            SqlCommand sqlCommand = new SqlCommand(String.Format("select Restaurant_Id from Restaurants where Restaurant_Name='{0}'", HttpContext.Session.GetString("RestaurantName")), conn);
            conn.Open();
            SqlDataReader sr = sqlCommand.ExecuteReader();
            while (sr.Read())
            {
                resId = (int)sr["Restaurant_Id"];
            }

            conn.Close();

            SqlConnection conn1 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");

            SqlCommand sqlCommand1 = new SqlCommand(String.Format("select * from Food where Restaurant_Id='{0}'", resId), conn);
            conn.Open();
            SqlDataReader sr1 = sqlCommand1.ExecuteReader();
            while (sr1.Read())
            {
                foodItemsList.Add(new FoodItem((int)sr1["Id"], sr1["Food_Item"].ToString(), sr1["Food_Image"].ToString(), (int)sr1["Price"]));
            }

            conn1.Close();
            var item = foodItemsList.Find(e => e.FoodItemId == id);
            return View(item);
            
        }
        [HttpPost]
        public IActionResult DeleteFoodItem(int id, FoodItem f)
        {
            if (HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out from theRestaurant {1}", HttpContext.Session.GetString("UserName"), HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Login");
            }
           
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd = new SqlCommand(String.Format("Delete from Food where Id = '{0}'", id), conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            

            return RedirectToAction("DisplayFoodItems");
           
        }
        public IActionResult History()
        {
            if (HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out from theRestaurant {1}", HttpContext.Session.GetString("UserName"), HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Login");
            }
            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");

            SqlCommand sqlCommand = new SqlCommand(String.Format("select Restaurant_Id from Restaurants where Restaurant_Name='{0}'", HttpContext.Session.GetString("RestaurantName")), conn);
            conn.Open();
            SqlDataReader sr = sqlCommand.ExecuteReader();
            while (sr.Read())
            {
                resId = (int)sr["Restaurant_Id"];
            }

            conn.Close();

            List<CompletedOrder> completedOrders = new List<CompletedOrder>();
            SqlConnection conn1 = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
            SqlCommand cmd1 = new SqlCommand(String.Format("select * from CompletedOrder where RestaurantId='{0}' order by OrderCompletionTime desc",resId), conn1);
            conn1.Open();
            SqlDataReader sr1 = cmd1.ExecuteReader();
            while(sr1.Read())
            {
                completedOrders.Add(new CompletedOrder((int)sr1["InVoiceNo"],sr1["UserName"].ToString(),sr1["FoodItem"].ToString(),(int)sr1["Quantity"],(int)sr1["Price"],(int)sr1["RestaurantId"],Convert.ToDateTime(sr1["OrderCompletionTime"]),sr1["status"].ToString()));
            }
            conn1.Close();
            return View(completedOrders);
        }

        /* public IActionResult Delete(int Id)
         {
             SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
             SqlCommand cmd = new SqlCommand(String.Format("delete from ConfirmOrder where OrderId = '{0}'",Id), conn);
             conn.Open();
             cmd.ExecuteNonQuery();
             conn.Close();

             return RedirectToAction("DisplayOrders");
         }*/

        /*[HttpPost]

        public async Task<IActionResult> Create([Bind("FoodItemName,FoodItemImage,Price,ImageFile")] FoodItem foodItem)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Path.GetFileNameWithoutExtension(foodItem.ImageFile.FileName);
            string extension = Path.GetExtension(foodItem.ImageFile.FileName);
            foodItem.FoodItemName = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
            string path = Path.Combine(wwwRootPath + "/Image/", fileName);
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await foodItem.ImageFile.CopyToAsync(fileStream);
            }
            //Insert record






            SqlConnection conn = new SqlConnection("Data Source = fooddeliverydatabase.ctzhubalbjxo.ap-south-1.rds.amazonaws.com,1433 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=False; User ID=admin; Password=surya1997;");
                int resId;
                SqlCommand sqlCommand = new SqlCommand(String.Format("select Restaurant_Id from Restaurants where Restaurant_Name='{0}'", _httpContextAccessor.HttpContext.Session.GetString("RestaurantName")), conn);
                conn.Open();
                SqlDataReader sr = sqlCommand.ExecuteReader();
                resId = (int)sr["Restaurant_Id"];
                conn.Close();

                SqlCommand sqlCommand1 = new SqlCommand(String.Format("insert into Food values('{0}','{1}','{2}','{3}')", foodItem.FoodItemImage, foodItem.FoodItemName, foodItem.Price, resId), conn);
                conn.Open();
                sqlCommand1.ExecuteNonQuery();
                conn.Close();


                return View("Logged");

        }*/





    }
}
