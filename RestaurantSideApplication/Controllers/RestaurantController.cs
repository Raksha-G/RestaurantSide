using Microsoft.AspNetCore.Mvc;
using RestaurantSideApplication.Models;
using System.Data.SqlClient;

namespace RestaurantSideApplication.Controllers
{
    public class RestaurantController : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;

        List<LoginDetails> Users = new List<LoginDetails>();
        public RestaurantController(IWebHostEnvironment hostEnvironment)
        {
            this._hostEnvironment = hostEnvironment;


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
                ViewBag.UserName = "UserName already Exist";
                return View();
            }
            if (Restaurant != null)
            {
                ViewBag.UserName = "Restaurant Name already Exist";
                return View();
            }
            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ; Initial Catalog = FoodDeliveryApplication; Integrated Security = True;");
            
            SqlCommand cmd = new SqlCommand(String.Format("insert into RestaurantLoginDetails values('{0}','{1}','{2}')", signup.RestaurantName, signup.UserName, signup.Password), conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            SqlCommand cmd1 = new SqlCommand(String.Format("insert into Restaurants values('{0}','{1}')", signup.RestaurantName, signup.RestaurantImage), conn);
            conn.Open();
            cmd1.ExecuteNonQuery();
            conn.Close();


            return View("Login");
        }

        public IActionResult Login()
        {
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
                return View();
            }
            if (restuarant == null)
            {
                ViewBag.ResNotExist = "NotExist";
                return View();
            }
            foreach (var i in Users)
            {
                if (i.UserName == login.UserName && i.Password == login.Password)
                {
                    HttpContext.Session.SetString("RestaurantName", login.RestaurantName);
                 
                    return RedirectToAction("DisplayOrders");
                }
            }
            ViewBag.IncorrectPassword = "Incorrect Password";
            return View();
        }


        public IActionResult DisplayOrders()
        {
            if (HttpContext.Session.GetString("RestaurantName") == null)
            {
                return RedirectToAction("Login");
            }
            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ;Initial Catalog=FoodDeliveryApplication; Integrated Security = True;");
            SqlCommand cmd = new SqlCommand(String.Format("select CO.OrderId, CO.UserName, CO.FoodItem, CO.Price , CO.Quantity from ConfirmOrder CO inner join Restaurants R on R.Restaurant_Id = CO.RestaurantId  where R.Restaurant_Name = '{0}'", HttpContext.Session.GetString("RestaurantName")), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();

            List<Order> orderDetails = new List<Order>();
            while(sr.Read())
            {
                Order order = new Order((int)sr["OrderId"],sr["UserName"].ToString(), sr["FoodItem"].ToString(), (int)sr["Quantity"], (int)sr["Price"]);
                orderDetails.Add(order);
            }

            return View("DisplayOrders",orderDetails);
        }

       
        //[HttpPost]
        public IActionResult CompletedOrder(int Id)
        {
            if (HttpContext.Session.GetString("RestaurantName") == null)
            {
                return RedirectToAction("Login");
            }
            SqlConnection conn = new SqlConnection("Data Source = PSL-28MH6Q3 ;Initial Catalog=FoodDeliveryApplication; Integrated Security = True;");
            //SqlCommand cmd = new SqlCommand(String.Format("select CO.OrderId, CO.UserName, CO.FoodItem, CO.Price, CO.Quantity from ConfirmOrder CO inner Join Restaurants R on CO.RestaurantId = R.Restaurant_Id where R.Restaurant_Name = '{0}'", HttpContext.Session.GetString("RestaurantName")), conn);
            SqlCommand cmd = new SqlCommand(String.Format("select OrderId,UserName,FoodItem,Price,Quantity from ConfirmOrder where OrderId = '{0}'",Id), conn);
            conn.Open();
            SqlDataReader sr = cmd.ExecuteReader();

            List<CompletedOrder> completedOrders = new List<CompletedOrder>();
            while(sr.Read())
            {
                CompletedOrder compOrder = new CompletedOrder((int)sr["OrderId"], sr["UserName"].ToString(), sr["FoodItem"].ToString(), (int)sr["Quantity"], (int)sr["Price"]);
                completedOrders.Add(compOrder);
            }
            conn.Close();

           foreach(var obj in completedOrders)
            {
                SqlCommand cmd1 = new SqlCommand(String.Format("insert into CompletedOrders values('{0}','{1}','{2}','{3}','{4}')",obj.OrderId,obj.CustomerName,obj.FoodItem,obj.Price,obj.Quantity), conn);
                conn.Open();
                cmd1.ExecuteNonQuery();
                conn.Close();

                SqlCommand cmd2 = new SqlCommand(String.Format("delete from ConfirmOrder where OrderId = '{0}'", obj.OrderId), conn);
                conn.Open();
                cmd2.ExecuteNonQuery();
                conn.Close();


                //return View("DisplayOrders");

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
