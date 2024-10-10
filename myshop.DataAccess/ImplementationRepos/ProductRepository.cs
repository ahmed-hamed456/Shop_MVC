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
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Product product)
        {
            var productInDb = _context.Products.FirstOrDefault(p=>p.ProductId == product.ProductId);

            if (productInDb != null)
            {
                productInDb.Name = product.Name;
                productInDb.Description = product.Description;
                productInDb.Price = product.Price;
                productInDb.Image = product.Image;
                productInDb.CategoryId = product.CategoryId;
            }
        }
    }
}
