using myshop.DataAccess.Data;
using myshop.Entities.Models;
using myshop.Entities.Repositories.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.DataAccess.ImplementationRepos
{
	public class OrderHeaderRepository : GenericRepository<OrderHeader>, IOrderHeaderRepository
	{
		private readonly ApplicationDbContext _context;
		public OrderHeaderRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}

		public void Update(OrderHeader orderHeader)
		{
			_context.OrderHeaders.Update(orderHeader);
		}

		public void UpdateOrderStatus(int id, string? orderStatus, string? paymentStatus)
		{
			var orderfromDB = _context.OrderHeaders.FirstOrDefault(x => x.Id == id);
			if (orderfromDB != null)
			{
				orderfromDB.OrderStatus = orderStatus;
				orderfromDB.PaymentDate = DateTime.Now;
				if (paymentStatus != null)
				{
					orderfromDB.PaymentStatus = paymentStatus; 
				}
			}
		}
	}
}
