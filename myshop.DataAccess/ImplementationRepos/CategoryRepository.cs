using Microsoft.EntityFrameworkCore;
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
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Category category)
        {
            var categoryInDb = _context.Categories.FirstOrDefault(x => x.Id == category.Id);
            if (categoryInDb != null)
            {
                categoryInDb.Name = category.Name;
                categoryInDb.Description = category.Description;
                categoryInDb.CreatedDate = DateTime.Now;
            }
        }
    }
}
