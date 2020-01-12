using System;
using TddStore.Core.Exceptions;

namespace TddStore.Core
{
    public class OrderService
    {
        private IOrderDataService _orderDataService;

        public OrderService(IOrderDataService orderDataService)
        {
            _orderDataService = orderDataService;
        }

        public Guid PlaceOrder(Guid customerId, ShoppingCart shoppingCart)
        {
            foreach (var item in shoppingCart.Items)
            {
                if (item.Quantity == 0)
                {
                    throw new InvalidOrderException();
                }
            }

            var order = new Order();
            return _orderDataService.Save(order);
            
        }
    }  
} 