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
    
    public partial class Ingredients
    {
        public Ingredients()
        {
            this.Structure = new HashSet<Structure>();
        }
    
        public int F_INGREDIENT_ID { get; set; }
        public string F_NAME { get; set; }
    
        public virtual ICollection<Structure> Structure { get; set; }
    }
}