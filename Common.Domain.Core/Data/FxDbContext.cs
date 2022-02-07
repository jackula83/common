﻿using Common.Domain.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Common.Domain.Core.Data
{
    public abstract class FxDbContext : DbContext
    {
        protected abstract void Setup<TEntity>(ModelBuilder builder) where TEntity : FxEntity;

        public FxDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var propertyTypes = this.GetType().GetProperties()
               .Select(p => p.PropertyType);

            var entityTypes = propertyTypes
               ?.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(DbSet<>))
               ?.Select(p => p.GetGenericArguments().FirstOrDefault());

            foreach (var entityType in entityTypes ?? Enumerable.Empty<Type>())
            {
                var setupMethod = this.GetType()
                    !.GetMethod(nameof(Setup), BindingFlags.Instance | BindingFlags.NonPublic)
                    !.MakeGenericMethod(new Type[] { entityType! })
                    !.Invoke(this, new object[] { modelBuilder });
            }
        }
    }
}
