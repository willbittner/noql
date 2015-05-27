using DashX.DTO.Internal.OrderRouter;
using DashX.DTO.Shared.Trading.Enums;


namespace Dashx.CEP.NewExpressions
{
    public class OrderDetailsWrapper
    {
        public Side Side;
        public double Price;
        public OrdType OrderType;
        public int Qty;
        public Route Route;
        public string Symbol;

        public OrderDetailsWrapper LimitOrder(Side side, double price, int qty, Route route = null, string symbol = "IWM")
        {
            OrderType = OrdType.Limit;
            OrderHelper(side,price,qty,symbol);
            if (route != null) Route = route;
            return this;
        }

        public OrderDetailsWrapper MarketOrder(Side side, int qty, Route route = null, string symbol = "IWM")
        {
            OrderType = OrdType.Market;
            OrderHelper(side, 0.00, qty, symbol);
            if (route != null) Route = route;
            return this;
        }

        private void OrderHelper(Side side, double price, int qty, string symbol = "IWM")
        {
            Side = side;
            Price = price;
            Qty = qty;
            Symbol = symbol;
        }


    }
}