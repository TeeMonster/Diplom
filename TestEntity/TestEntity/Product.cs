//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TestEntity
{
    using System;
    using System.Collections.Generic;
    
    public partial class Product
    {
        public Product()
        {
            this.CostProduct = new HashSet<CostProduct>();
            this.Ingredients = new HashSet<Ingredients>();
        }
    
        public int F_PRODUCT_ID { get; set; }
        public int F_CAT_ID { get; set; }
        public string F_NAME { get; set; }
        public float F_VES { get; set; }
        public string F_URL { get; set; }
    
        public virtual Category Category { get; set; }
        public virtual ICollection<CostProduct> CostProduct { get; set; }
        public virtual ICollection<Ingredients> Ingredients { get; set; }
    }
}
