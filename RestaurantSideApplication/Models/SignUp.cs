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
        public string Password { get; set; }

        public string RestaurantImage { get; set; }

        public SignUp()
        {

        }
        public SignUp(string restaurantName, string userName, string password, string restaurantImage)
        {
            RestaurantName = restaurantName;
            UserName = userName;
            Password = password;
            RestaurantImage = restaurantImage;
        }
    }
}
