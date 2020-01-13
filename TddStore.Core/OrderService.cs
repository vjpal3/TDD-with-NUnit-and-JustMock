using System;
using TddStore.Core.Exceptions;

namespace TddStore.Core
{
    public class OrderService
    {
        private IOrderDataService _orderDataService;
        private ICustomerService _customerService;

        public OrderService(IOrderDataService orderDataService, ICustomerService customerService)
        {
            _orderDataService = orderDataService;
            _customerService = customerService;

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

            _customerService.GetCustomer(customerId);

            var order = new Order();
            return _orderDataService.Save(order);
            
        }
    }  
} 