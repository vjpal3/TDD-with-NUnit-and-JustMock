using System;
using System.Collections.Generic;
using TddStore.Core.Exceptions;

namespace TddStore.Core
{
    public class OrderService
    {
        private IOrderDataService _orderDataService;
        private ICustomerService _customerService;
        private IOrderFulfillmentService _orderFulfillmentService;
        private const string USERNAME = "Bob";
        private const string PASSWORD = "Foo";

        public OrderService(IOrderDataService orderDataService, ICustomerService customerService, IOrderFulfillmentService orderFulfillmentService)
        {
            _orderDataService = orderDataService;
            _customerService = customerService;
            _orderFulfillmentService = orderFulfillmentService;

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

            var customer = _customerService.GetCustomer(customerId);

            //Open session
            var orderFulfillmentSessionId = _orderFulfillmentService.OpenSession(USERNAME, PASSWORD);

            var firstItemId = shoppingCart.Items[0].ItemId;
            var firstItemQuantity = shoppingCart.Items[0].Quantity;


            // check inventory
            var itemIsInInventory = _orderFulfillmentService.IsInInventory(orderFulfillmentSessionId, firstItemId, firstItemQuantity);

            // Place order
            var orderForFulfillmentService = new Dictionary<Guid, int>();
            orderForFulfillmentService.Add(firstItemId, firstItemQuantity);

            var orderPlaced = _orderFulfillmentService
                .PlaceOrder(orderFulfillmentSessionId, orderForFulfillmentService, customer.ShippingAddress.ToString());

            // Close session
            _orderFulfillmentService.CloseSession(orderFulfillmentSessionId);

            var order = new Order();
            return _orderDataService.Save(order);
            
        }
    }  
} 