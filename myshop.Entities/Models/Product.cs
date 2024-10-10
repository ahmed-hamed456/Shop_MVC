using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Entities.Models
{
    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [DisplayName("Image")]
        [ValidateNever]
        public string? Image { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        [DisplayName("Category")]
        public int CategoryId { get; set; }

        [ValidateNever]
        public Category Category { get; set; }
    }
}
