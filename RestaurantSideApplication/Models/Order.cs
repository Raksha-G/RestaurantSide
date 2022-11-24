using System.ComponentModel.DataAnnotations;

namespace RestaurantSideApplication.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int InVoiceNo { get; set; }

        public string CustomerName { get; set; }

        public string FoodItem { get; set; }

        public int Quantity { get; set; }

        public int Price { get; set; }

        public DateTime OrderTime  { get; set; }

        public int OrderNo { get; set; }


        public Order()
        {

        }
      
        public Order(int inVoiceNo, string customerName, string foodItem, int quantity, int price, DateTime orderTime, int orderNo)
        {

            InVoiceNo = inVoiceNo;
            CustomerName = customerName;
            FoodItem = foodItem;
            Quantity = quantity;
            Price = price;
            OrderTime = orderTime;
            OrderNo = orderNo;
        }
    }
}
