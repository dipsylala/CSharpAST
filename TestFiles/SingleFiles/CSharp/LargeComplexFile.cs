using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace LargeComplexExample
{
    // Complex generic class with multiple constraints
    public abstract class BaseRepository<TEntity, TKey> : IRepository<TEntity, TKey> 
        where TEntity : class, IEntity<TKey>, new()
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        protected readonly List<TEntity> _entities = new();
        protected readonly Dictionary<TKey, TEntity> _entityCache = new();

        public virtual async Task<TEntity?> GetByIdAsync(TKey id)
        {
            if (_entityCache.TryGetValue(id, out var cached))
                return cached;

            await Task.Delay(Random.Shared.Next(10, 50));
            var entity = _entities.FirstOrDefault(e => e.Id.Equals(id));
            
            if (entity != null)
                _entityCache[id] = entity;
                
            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            await Task.Delay(Random.Shared.Next(20, 100));
            return _entities.AsEnumerable();
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await ValidateEntityAsync(entity);
            await Task.Delay(Random.Shared.Next(15, 75));
            
            _entities.Add(entity);
            _entityCache[entity.Id] = entity;
            
            await OnEntityAddedAsync(entity);
            return entity;
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var existing = await GetByIdAsync(entity.Id);
            if (existing == null)
                throw new InvalidOperationException($"Entity with ID {entity.Id} not found");

            await ValidateEntityAsync(entity);
            await Task.Delay(Random.Shared.Next(15, 75));

            var index = _entities.IndexOf(existing);
            _entities[index] = entity;
            _entityCache[entity.Id] = entity;

            await OnEntityUpdatedAsync(entity, existing);
            return entity;
        }

        public virtual async Task<bool> DeleteAsync(TKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;

            await Task.Delay(Random.Shared.Next(10, 50));
            
            _entities.Remove(entity);
            _entityCache.Remove(id);
            
            await OnEntityDeletedAsync(entity);
            return true;
        }

        protected virtual async Task ValidateEntityAsync(TEntity entity)
        {
            await Task.Delay(5);
            
            var validationContext = new ValidationContext(entity);
            var validationResults = new List<ValidationResult>();
            
            if (!Validator.TryValidateObject(entity, validationContext, validationResults, true))
            {
                var errors = validationResults.Select(r => r.ErrorMessage).ToList();
                throw new ValidationException($"Validation failed: {string.Join(", ", errors)}");
            }
        }

        protected virtual async Task OnEntityAddedAsync(TEntity entity)
        {
            await Task.Delay(1);
            // Override in derived classes for custom logic
        }

        protected virtual async Task OnEntityUpdatedAsync(TEntity newEntity, TEntity oldEntity)
        {
            await Task.Delay(1);
            // Override in derived classes for custom logic
        }

        protected virtual async Task OnEntityDeletedAsync(TEntity entity)
        {
            await Task.Delay(1);
            // Override in derived classes for custom logic
        }
    }

    // Complex interface hierarchy
    public interface IEntity<TKey>
    {
        TKey Id { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }

    public interface IRepository<TEntity, TKey>
    {
        Task<TEntity?> GetByIdAsync(TKey id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity> AddAsync(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task<bool> DeleteAsync(TKey id);
    }

    public interface IAuditableEntity<TKey> : IEntity<TKey>
    {
        string CreatedBy { get; set; }
        string UpdatedBy { get; set; }
        bool IsDeleted { get; set; }
    }

    // Complex entity with validation attributes
    public class ComplexEntity : IAuditableEntity<Guid>
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
        
        [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
        public int Age { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public string CreatedBy { get; set; } = string.Empty;
        
        [Required]
        public string UpdatedBy { get; set; } = string.Empty;
        
        public bool IsDeleted { get; set; }
        
        public Dictionary<string, object> CustomProperties { get; set; } = new();
        
        public List<Tag> Tags { get; set; } = new();
        
        public Address? Address { get; set; }
    }

    public class Tag
    {
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#000000";
        public int Priority { get; set; }
    }

    public class Address
    {
        [Required]
        public string Street { get; set; } = string.Empty;
        
        [Required]
        public string City { get; set; } = string.Empty;
        
        [Required]
        public string Country { get; set; } = string.Empty;
        
        [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid postal code format")]
        public string PostalCode { get; set; } = string.Empty;
    }

    // Complex repository implementation
    public class ComplexEntityRepository : BaseRepository<ComplexEntity, Guid>
    {
        private readonly INotificationService _notificationService;
        private readonly ICacheService _cacheService;
        private readonly ILogger _logger;

        public ComplexEntityRepository(
            INotificationService notificationService,
            ICacheService cacheService,
            ILogger logger)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ComplexEntity>> SearchAsync(SearchCriteria criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            await Task.Delay(Random.Shared.Next(50, 200));

            var query = _entities.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(criteria.Name))
            {
                query = query.Where(e => e.Name.Contains(criteria.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(criteria.Email))
            {
                query = query.Where(e => e.Email.Contains(criteria.Email, StringComparison.OrdinalIgnoreCase));
            }

            if (criteria.MinAge.HasValue)
            {
                query = query.Where(e => e.Age >= criteria.MinAge.Value);
            }

            if (criteria.MaxAge.HasValue)
            {
                query = query.Where(e => e.Age <= criteria.MaxAge.Value);
            }

            if (criteria.Tags?.Any() == true)
            {
                query = query.Where(e => e.Tags.Any(t => criteria.Tags.Contains(t.Name)));
            }

            if (!criteria.IncludeDeleted)
            {
                query = query.Where(e => !e.IsDeleted);
            }

            var results = query.ToList();

            await _logger.LogAsync($"Search returned {results.Count} results for criteria: {JsonSerializer.Serialize(criteria)}");

            return results;
        }

        public async Task<ComplexEntity> CloneAsync(Guid id, string newName)
        {
            var original = await GetByIdAsync(id);
            if (original == null)
                throw new InvalidOperationException($"Entity with ID {id} not found");

            var clone = new ComplexEntity
            {
                Id = Guid.NewGuid(),
                Name = newName,
                Email = original.Email,
                Age = original.Age,
                CreatedBy = original.CreatedBy,
                UpdatedBy = original.UpdatedBy,
                CustomProperties = new Dictionary<string, object>(original.CustomProperties),
                Tags = original.Tags.Select(t => new Tag 
                { 
                    Name = t.Name, 
                    Color = t.Color, 
                    Priority = t.Priority 
                }).ToList(),
                Address = original.Address != null ? new Address
                {
                    Street = original.Address.Street,
                    City = original.Address.City,
                    Country = original.Address.Country,
                    PostalCode = original.Address.PostalCode
                } : null
            };

            return await AddAsync(clone);
        }

        protected override async Task OnEntityAddedAsync(ComplexEntity entity)
        {
            await base.OnEntityAddedAsync(entity);
            
            await _notificationService.SendNotificationAsync(
                $"New entity created: {entity.Name}",
                NotificationType.EntityCreated);
                
            await _cacheService.InvalidateAsync($"entity_{entity.Id}");
            await _logger.LogAsync($"Entity added: {entity.Id} - {entity.Name}");
        }

        protected override async Task OnEntityUpdatedAsync(ComplexEntity newEntity, ComplexEntity oldEntity)
        {
            await base.OnEntityUpdatedAsync(newEntity, oldEntity);
            
            var changes = CompareEntities(oldEntity, newEntity);
            if (changes.Any())
            {
                await _notificationService.SendNotificationAsync(
                    $"Entity updated: {newEntity.Name}. Changes: {string.Join(", ", changes)}",
                    NotificationType.EntityUpdated);
            }
            
            await _cacheService.InvalidateAsync($"entity_{newEntity.Id}");
            await _logger.LogAsync($"Entity updated: {newEntity.Id} - {newEntity.Name}");
        }

        protected override async Task OnEntityDeletedAsync(ComplexEntity entity)
        {
            await base.OnEntityDeletedAsync(entity);
            
            await _notificationService.SendNotificationAsync(
                $"Entity deleted: {entity.Name}",
                NotificationType.EntityDeleted);
                
            await _cacheService.InvalidateAsync($"entity_{entity.Id}");
            await _logger.LogAsync($"Entity deleted: {entity.Id} - {entity.Name}");
        }

        private List<string> CompareEntities(ComplexEntity old, ComplexEntity updated)
        {
            var changes = new List<string>();
            
            if (old.Name != updated.Name)
                changes.Add($"Name: '{old.Name}' -> '{updated.Name}'");
                
            if (old.Email != updated.Email)
                changes.Add($"Email: '{old.Email}' -> '{updated.Email}'");
                
            if (old.Age != updated.Age)
                changes.Add($"Age: {old.Age} -> {updated.Age}");

            return changes;
        }
    }

    // Supporting classes and interfaces
    public class SearchCriteria
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public List<string>? Tags { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }

    public interface INotificationService
    {
        Task SendNotificationAsync(string message, NotificationType type);
    }

    public interface ICacheService
    {
        Task InvalidateAsync(string key);
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    }

    public interface ILogger
    {
        Task LogAsync(string message);
    }

    public enum NotificationType
    {
        EntityCreated,
        EntityUpdated,
        EntityDeleted
    }

    // Complex service layer
    public class ComplexEntityService
    {
        private readonly ComplexEntityRepository _repository;
        private readonly INotificationService _notificationService;
        private readonly ICacheService _cacheService;
        private readonly ILogger _logger;

        public ComplexEntityService(
            ComplexEntityRepository repository,
            INotificationService notificationService,
            ICacheService cacheService,
            ILogger logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ComplexEntity> CreateEntityWithValidationAsync(CreateEntityRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Complex validation logic
            await ValidateCreateRequestAsync(request);

            var entity = new ComplexEntity
            {
                Name = request.Name,
                Email = request.Email,
                Age = request.Age,
                CreatedBy = request.CreatedBy,
                UpdatedBy = request.CreatedBy,
                CustomProperties = request.CustomProperties ?? new Dictionary<string, object>(),
                Tags = request.Tags?.Select(t => new Tag
                {
                    Name = t.Name,
                    Color = t.Color ?? "#000000",
                    Priority = t.Priority
                }).ToList() ?? new List<Tag>(),
                Address = request.Address != null ? new Address
                {
                    Street = request.Address.Street,
                    City = request.Address.City,
                    Country = request.Address.Country,
                    PostalCode = request.Address.PostalCode
                } : null
            };

            try
            {
                entity = await _repository.AddAsync(entity);
                await _cacheService.SetAsync($"entity_{entity.Id}", entity, TimeSpan.FromHours(1));
                return entity;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync($"Failed to create entity: {ex.Message}");
                throw;
            }
        }

        private async Task ValidateCreateRequestAsync(CreateEntityRequest request)
        {
            await Task.Delay(10); // Simulate validation work

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Name))
                errors.Add("Name is required");

            if (string.IsNullOrWhiteSpace(request.Email))
                errors.Add("Email is required");
            else if (!IsValidEmail(request.Email))
                errors.Add("Invalid email format");

            if (request.Age < 18 || request.Age > 120)
                errors.Add("Age must be between 18 and 120");

            if (await IsEmailAlreadyUsedAsync(request.Email))
                errors.Add("Email is already in use");

            if (errors.Any())
                throw new ValidationException($"Validation errors: {string.Join(", ", errors)}");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> IsEmailAlreadyUsedAsync(string email)
        {
            var entities = await _repository.GetAllAsync();
            return entities.Any(e => e.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && !e.IsDeleted);
        }
    }

    public class CreateEntityRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Age { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public Dictionary<string, object>? CustomProperties { get; set; }
        public List<CreateTagRequest>? Tags { get; set; }
        public CreateAddressRequest? Address { get; set; }
    }

    public class CreateTagRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
        public int Priority { get; set; }
    }

    public class CreateAddressRequest
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
    }
}
