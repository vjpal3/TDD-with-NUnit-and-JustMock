using System;

namespace TddStore.Core
{
    public interface IOrderDataService
    {
        Guid Save(Order order);
    }
}