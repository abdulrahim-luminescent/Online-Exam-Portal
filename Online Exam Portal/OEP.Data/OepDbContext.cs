﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using OEP.Core.DomainModels;
using OEP.Core.DomainModels.EducationModels;
using OEP.Core.DomainModels.Identity;
using OEP.Data.Configuration;

namespace OEP.Data
{
    public class OepDbContext:IdentityDbContext<ApplicationUser>,IEntitiesContext
    {
        private ObjectContext _objectContext;
        private DbTransaction _transaction;
        private static readonly object Lock = new object();
        private static bool _databaseInitialized;

        public OepDbContext()
            : base("OepDbConnection")
        {

        }

        public OepDbContext(string nameOrConnectionString):base(nameOrConnectionString)
        {
            if (_databaseInitialized)
            {
                return;
            }
            lock (Lock)
            {
                if (!_databaseInitialized)
                {
                    // Set the database intializer which is run once during application start
                    // This seeds the database with admin user credentials and admin role
                    Database.SetInitializer(new ApplicationDbInitializer());
                    _databaseInitialized = true;
                }
            }
        }

        public static OepDbContext Create()
        {
            return new OepDbContext(nameOrConnectionString: "OepDbConnection");
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<OepDbContext>(null);
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new CategoryConfig());
            modelBuilder.Configurations.Add(new SubCAtegoryConfig());
            modelBuilder.Configurations.Add(new EducationConfig());
            modelBuilder.Configurations.Add(new EducationYearConfig());
            modelBuilder.Configurations.Add(new EducationDetailsConfig());
            modelBuilder.Configurations.Add(new PackageConfig());
            modelBuilder.Configurations.Add(new ExamConfig());
            modelBuilder.Configurations.Add(new ExamTypeConfig());
            modelBuilder.Configurations.Add(new ExamQuestionConfig());
            modelBuilder.Configurations.Add(new QuestionsConfig());
            modelBuilder.Configurations.Add(new PackageSelectedConfig());
            modelBuilder.Configurations.Add(new QuestionsTypeConfig());
            modelBuilder.Configurations.Add(new ResultConfig());
            modelBuilder.Configurations.Add(new StudyMaterialConfig());
            modelBuilder.Configurations.Add(new QuestionsLocalizedConfig());
            modelBuilder.Configurations.Add(new LanguageConfig());
            
            base.OnModelCreating(modelBuilder);
        }

        public new IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        public void SetAsAdded<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            UpdateEntityState(entity, EntityState.Added);
        }

        public void SetAsModified<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            UpdateEntityState(entity, EntityState.Modified);
        }

        public void SetAsDeleted<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            UpdateEntityState(entity, EntityState.Deleted);
        }

        public void BeginTransaction()
        {
            _objectContext = ((IObjectContextAdapter)this).ObjectContext;
            if (_objectContext.Connection.State == ConnectionState.Open)
            {
                return;
            }
            _objectContext.Connection.Open();
            _transaction = _objectContext.Connection.BeginTransaction();
        }

        public int Commit()
        {
            try
            {
                BeginTransaction();
                var saveChanges = SaveChanges();
                _transaction.Commit();

                return saveChanges;
            }
            catch (Exception)
            {
                Rollback();
                throw;
            }
            finally
            {
                Dispose();
            }
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }

        public async Task<int> CommitAsync()
        {
            try
            {
                BeginTransaction();
                var saveChangesAsync = await SaveChangesAsync();
                _transaction.Commit();

                return saveChangesAsync;
            }
            catch (Exception)
            {
                Rollback();
                throw;
            }
            finally
            {
                Dispose();
            }
        }

        private void UpdateEntityState<TEntity>(TEntity entity, EntityState entityState) where TEntity : BaseEntity
        {
            var dbEntityEntry = GetDbEntityEntrySafely(entity);
            dbEntityEntry.State = entityState;
        }

        private DbEntityEntry GetDbEntityEntrySafely<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            var dbEntityEntry = Entry<TEntity>(entity);
            if (dbEntityEntry.State == EntityState.Detached)
            {
                Set<TEntity>().Attach(entity);
            }
            return dbEntityEntry;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_objectContext != null && _objectContext.Connection.State == ConnectionState.Open)
                {
                    _objectContext.Connection.Close();
                }
                if (_objectContext != null)
                {
                    _objectContext.Dispose();
                }
                if (_transaction != null)
                {
                    _transaction.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        
    }
}
