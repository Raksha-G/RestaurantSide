using System.ComponentModel.DataAnnotations;

namespace RestaurantSideApplication.Models
{
    public class LoginDetails
    {
        [Required]
        public string RestaurantName { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        public LoginDetails()
        {

        }
        public LoginDetails(string restaurantName, string userName, string password)
        {
            RestaurantName = restaurantName;
            UserName = userName;
            Password = password;
        }
    }
}
