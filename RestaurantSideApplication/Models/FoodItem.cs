using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantSideApplication.Models
{
    public class FoodItem
    {
        /*[Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]*/
        
        public int FoodItemId { get; set; }
        public string FoodItemName { get; set; }

        public string FoodItemImage { get; set; }

        public int Price { get; set; }

        public  string Type { get; set; }

        [NotMapped]
        [DisplayName("Upload File")]
        public IFormFile ImageFile { get; set; }

        /*public int RestaurantId { get; set; }

        [NotMapped]
        [DisplayName("Upload File")]
        public IFormFile ImageFile { get; set; }*/

        public FoodItem(int foodItemId, string foodItemName, IFormFile imagefile, int price)
        {
            FoodItemId = foodItemId;
            FoodItemName = foodItemName;
            ImageFile = imagefile;
            Price = price;


        }
        public FoodItem(int foodItemId, string foodItemName, string foodimage, int price)
        {
            FoodItemId = foodItemId;
            FoodItemName = foodItemName;
            FoodItemImage= foodimage;
            Price = price;


        }
        public FoodItem(string foodItemName, IFormFile imagefile, int price)
        {
       
            FoodItemName = foodItemName;
            ImageFile=imagefile;
            Price = price;
          
        }


        public FoodItem(int foodItemId, string foodItemName, IFormFile imagefile, int price,string type)
        {
            FoodItemId = foodItemId;
            FoodItemName = foodItemName;
            ImageFile = imagefile;
            Price = price;
            Type = type;


        }
        public FoodItem(int foodItemId, string foodItemName, string image, int price, string type)
        {
            FoodItemId = foodItemId;
            FoodItemName = foodItemName;
            FoodItemImage = image;
            Price = price;
            Type = type;


        }


        public FoodItem()
        {

        }
    }
}
