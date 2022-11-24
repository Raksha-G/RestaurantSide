namespace RestaurantSideApplication.Models
{
    public class CompletedOrder
    {
        public int InVoiceNo { get; set; }

        public string CustomerName { get; set; }

        public string FoodItem { get; set; }

        public int Quantity { get; set; }

        public int Price { get; set; }

     

        public CompletedOrder(int inVoiceNo, string customerName, string foodItem, int quantity, int price)
        {
            InVoiceNo = inVoiceNo;
            CustomerName = customerName;
            FoodItem = foodItem;
            Quantity = quantity;
            Price = price;
        
        }
    }
}
