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
    
    public partial class Structure
    {
        public int F_INGREDIENT_ID { get; set; }
        public decimal F_VES { get; set; }
        public int F_RECIPE_ID { get; set; }
        public long F_ID { get; set; }
    
        public virtual Ingredients Ingredients { get; set; }
        public virtual Recipes Recipes { get; set; }
    }
}