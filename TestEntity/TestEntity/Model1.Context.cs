﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class StartupEntities : DbContext
    {
        public StartupEntities()
            : base("name=StartupEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<V_CATEGORY> V_CATEGORY { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<CategoryRecipes> CategoryRecipes { get; set; }
        public virtual DbSet<CostProduct> CostProduct { get; set; }
        public virtual DbSet<Ingredients> Ingredients { get; set; }
        public virtual DbSet<Recipes> Recipes { get; set; }
        public virtual DbSet<Structure> Structure { get; set; }
        public virtual DbSet<V_INGREDIENTPRODUCT> V_INGREDIENTPRODUCT { get; set; }
    
        public virtual int PV_INS_CATEGORY(string catname, string catparentname, string caturl)
        {
            var catnameParameter = catname != null ?
                new ObjectParameter("catname", catname) :
                new ObjectParameter("catname", typeof(string));
    
            var catparentnameParameter = catparentname != null ?
                new ObjectParameter("catparentname", catparentname) :
                new ObjectParameter("catparentname", typeof(string));
    
            var caturlParameter = caturl != null ?
                new ObjectParameter("caturl", caturl) :
                new ObjectParameter("caturl", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("PV_INS_CATEGORY", catnameParameter, catparentnameParameter, caturlParameter);
        }
    
        public virtual int P_INS_COSTPRODUCT(Nullable<int> product_id, Nullable<decimal> cost)
        {
            var product_idParameter = product_id.HasValue ?
                new ObjectParameter("product_id", product_id) :
                new ObjectParameter("product_id", typeof(int));
    
            var costParameter = cost.HasValue ?
                new ObjectParameter("cost", cost) :
                new ObjectParameter("cost", typeof(decimal));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("P_INS_COSTPRODUCT", product_idParameter, costParameter);
        }
    
        public virtual int PV_INS_INGREDIENTPRODUCT(Nullable<int> in_ingredientid, Nullable<int> in_productid)
        {
            var in_ingredientidParameter = in_ingredientid.HasValue ?
                new ObjectParameter("in_ingredientid", in_ingredientid) :
                new ObjectParameter("in_ingredientid", typeof(int));
    
            var in_productidParameter = in_productid.HasValue ?
                new ObjectParameter("in_productid", in_productid) :
                new ObjectParameter("in_productid", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("PV_INS_INGREDIENTPRODUCT", in_ingredientidParameter, in_productidParameter);
        }
    }
}
