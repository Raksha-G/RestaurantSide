using System.ComponentModel.DataAnnotations;

namespace RestaurantSideApplication.Models
{
    public class SignUp
    {

        [Required]
        public string RestaurantName { get; set; }

        [Required]
        public string UserName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

        public string RestaurantImage { get; set; }

        public string Cuisine { get; set; }

        public SignUp()
        {

        }
        public SignUp(string restaurantName, string userName,string email, string password, string restaurantImage, string cuisine)
        {
            RestaurantName = restaurantName;
            UserName = userName;
            Email = email;
            Password = password;
            RestaurantImage = restaurantImage;
            Cuisine = cuisine;
        }
    }
}
