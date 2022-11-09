using System.ComponentModel.DataAnnotations;

namespace RestaurantSideApplication.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public string CustomerName { get; set; }

        public string FoodItem { get; set; }

        public int Quantity { get; set; }

        public int Price { get; set; }


        public Order()
        {

        }
        public Order(int orderId, string customerName, string foodItem, int quantity, int price)
        {
            OrderId = orderId;
            CustomerName = customerName;
            FoodItem = foodItem;
            Quantity = quantity;
            Price = price;
        }
    }
}
