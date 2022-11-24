using Microsoft.AspNetCore.Mvc;
using RestaurantSideApplication.Models;
using System.Data.SqlClient;

namespace RestaurantSideApplication.Controllers
{
    public class RestaurantController : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ILogger<RestaurantController> _logger;

        List<LoginDetails> Users = new List<LoginDetails>();
        public RestaurantController(IWebHostEnvironment hostEnvironment, ILogger<RestaurantController> logger)
        {
            this._hostEnvironment = hostEnvironment;
            _logger = logger;


            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ; Initial Catalog = FoodDeliveryApplication ; Integrated Security=True;");
            SqlCommand cmd = new SqlCommand("select * from RestaurantLoginDetails", conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            while (sr.Read())
            {
                LoginDetails user = new LoginDetails(sr["RestaurantName"].ToString(), sr["UserName"].ToString(), sr["Password"].ToString());
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
            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ; Initial Catalog = FoodDeliveryApplication; Integrated Security = True;");
            
            SqlCommand cmd = new SqlCommand(String.Format("insert into RestaurantLoginDetails values('{0}','{1}','{2}')", signup.RestaurantName, signup.UserName, signup.Password), conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            _logger.LogInformation(String.Format("A new Account Created with UserName {0} and RestaurantName {1}", signup.UserName, signup.RestaurantName));

            SqlCommand cmd1 = new SqlCommand(String.Format("insert into Restaurants values('{0}','{1}')", signup.RestaurantName, signup.RestaurantImage), conn);
            conn.Open();
            cmd1.ExecuteNonQuery();
            conn.Close();


            return View("Login");
        }


        public IActionResult Logout()
        {
            _logger.LogInformation("{0} logged out from the Restaurant {1}", HttpContext.Session.GetString("userName"), HttpContext.Session.GetString("RestaurantName"));
            return View("Login");
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
                    HttpContext.Session.SetString("RestaurantName", login.RestaurantName);
                    HttpContext.Session.SetString("userName", login.UserName);
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
            if (HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out from theRestaurant {1}", HttpContext.Session.GetString("UserName"), HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Login");
            }


            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ;Initial Catalog=FoodDeliveryApplication; Integrated Security = True;");
            SqlCommand cmd = new SqlCommand(String.Format("select * from PlacedOrderDetail PO inner join Restaurants R on R.Restaurant_Id = PO.RestaurantId  where R.Restaurant_Name = '{0}'", HttpContext.Session.GetString("RestaurantName")), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();
            List<Order> orderDetails = new List<Order>();
            while (sr.Read())
            {
                Order order = new Order((int)sr["InVoiceNo"], sr["UserName"].ToString(), sr["FoodItem"].ToString(), (int)sr["Quantity"], (int)sr["Price"], (DateTime)sr["OrderTime"], (int)sr["OrderNo"]);
                orderDetails.Add(order);
            }




            return View("DisplayOrders",orderDetails);
        }

    
        public IActionResult CompletedOrder(int Id)
        {
            if (HttpContext.Session.GetString("RestaurantName") == null)
            {
                _logger.LogInformation("{0} logged out, Owner of Restaurant {1}", HttpContext.Session.GetString("UserName"), HttpContext.Session.GetString("RestaurantName"));
                return RedirectToAction("Login");
            }

            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ;Initial Catalog=FoodDeliveryApplication; Integrated Security = True;");
            SqlCommand cmd = new SqlCommand(String.Format("select * from PlacedOrderDetail where OrderNo = '{0}'", Id), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();

            List<CompletedOrder> completedOrders = new List<CompletedOrder>();
            while (sr.Read())
            {
                CompletedOrder compOrder = new CompletedOrder((int)sr["InVoiceNo"], sr["UserName"].ToString(), sr["FoodItem"].ToString(), (int)sr["Quantity"], (int)sr["Price"]);
                completedOrders.Add(compOrder);
            }
            conn.Close();

            foreach (var obj in completedOrders)
            {
                SqlCommand cmd1 = new SqlCommand(String.Format("insert into CompletedOrder values('{0}','{1}','{2}','{3}','{4}','{5}')", obj.InVoiceNo, obj.CustomerName, obj.FoodItem, obj.Price, obj.Quantity,DateTime.Now), conn);
                conn.Open();
                _logger.LogDebug("Order Delivered to user {0}, Item {1} with Invoice No {2}", obj.CustomerName, obj.FoodItem, obj.InVoiceNo);
                cmd1.ExecuteNonQuery();
                conn.Close();

                SqlCommand cmd2 = new SqlCommand(String.Format("delete from PlacedOrderDetail where OrderNo = '{0}'", Id), conn);
                conn.Open();
                cmd2.ExecuteNonQuery();
                conn.Close();
            }




         return RedirectToAction("DisplayOrders");

            
        }

        public IActionResult AddFoodItem()
        {
            return View();
        }

        //[HttpPost]
        /*  public IActionResult AddFoodItem(FoodItem foodItem)
          {
              SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ;Initial Catalog=FoodDeliveryApplication; Integrated Security = True;");
              int resId;
              SqlCommand sqlCommand = new SqlCommand(String.Format("select Restaurant_Id from Restaurants where Restaurant_Name='{0}'", HttpContext.Session.GetString("RestaurantName")),conn);
              conn.Open();
              SqlDataReader sr = sqlCommand.ExecuteReader();
              resId = (int)sr["Restaurant_Id"];
              conn.Close();

              SqlCommand sqlCommand1 = new SqlCommand(String.Format("insert into Food values('{0}','{1}','{2}','{3}')",foodItem.FoodItemImage,foodItem.FoodItemName,foodItem.Price,resId), conn);
              conn.Open();
              sqlCommand1.ExecuteNonQuery();
              conn.Close();

              return View("DisplayOrder");
          }*/

        /* public IActionResult Delete(int Id)
         {
             SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ;Initial Catalog=FoodDeliveryApplication; Integrated Security = True;");
             SqlCommand cmd = new SqlCommand(String.Format("delete from ConfirmOrder where OrderId = '{0}'",Id), conn);
             conn.Open();
             cmd.ExecuteNonQuery();
             conn.Close();

             return RedirectToAction("DisplayOrders");
         }*/

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FoodItemName,FoodItemImage,Price,ImageFile")] FoodItem foodItem)
        {
            //if (ModelState.IsValid)
            //{
                //Save image to wwwroot/image
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string fileName = Path.GetFileNameWithoutExtension(foodItem.ImageFile.FileName);
                string extension = Path.GetExtension(foodItem.ImageFile.FileName);
                foodItem.FoodItemName = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                string path = Path.Combine(wwwRootPath + "/Image/", fileName);
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await foodItem.ImageFile.CopyToAsync(fileStream);
                }
             


                SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ;Initial Catalog=FoodDeliveryApplication; Integrated Security = True;");
                int resId;
                SqlCommand sqlCommand = new SqlCommand(String.Format("select Restaurant_Id from Restaurants where Restaurant_Name='{0}'", HttpContext.Session.GetString("RestaurantName")), conn);
                conn.Open();
                SqlDataReader sr = sqlCommand.ExecuteReader();
                resId = (int)sr["Restaurant_Id"];
                conn.Close();

                SqlCommand sqlCommand1 = new SqlCommand(String.Format("insert into Food values('{0}','{1}','{2}','{3}')", foodItem.FoodItemImage, foodItem.FoodItemName, foodItem.Price, resId), conn);
                conn.Open();
                sqlCommand1.ExecuteNonQuery();
                conn.Close();


                //return RedirectToAction("Logged");
                return View("Logged");
            //}
           // return View(foodItem);
        }

    }
}
