using System.Collections.Generic;

namespace BasicDLL.Models
{
    /// <summary>
    /// Represents a user entity with basic properties and behavior
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public List<string> Roles { get; set; } = new();

        public User()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public User(string name, string email) : this()
        {
            Name = name;
            Email = email;
        }

        public void AddRole(string role)
        {
            if (!string.IsNullOrWhiteSpace(role) && !Roles.Contains(role))
            {
                Roles.Add(role);
            }
        }

        public void RemoveRole(string role)
        {
            Roles.Remove(role);
        }

        public bool HasRole(string role)
        {
            return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public override string ToString()
        {
            return $"{Name} ({Email}) - Active: {IsActive}";
        }
    }
}
