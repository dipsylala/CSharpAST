using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SecurityTestExample
{
    // Vulnerable SQL injection example
    public class SqlInjectionExample
    {
        public string UnsafeQuery(string userInput)
        {
            // SECURITY RISK: SQL Injection vulnerability
            string query = "SELECT * FROM Users WHERE Name = '" + userInput + "'";
            return query;
        }

        public string SafeQuery(string userInput)
        {
            // SAFE: Using parameterized queries
            string query = "SELECT * FROM Users WHERE Name = @name";
            return query;
        }
    }

    // Cross-Site Scripting (XSS) vulnerability
    public class XssExample
    {
        public string UnsafeHtmlOutput(string userInput)
        {
            // SECURITY RISK: XSS vulnerability
            return $"<div>Hello {userInput}</div>";
        }

        public string SafeHtmlOutput(string userInput)
        {
            // SAFE: HTML encoding
            var encoded = System.Web.HttpUtility.HtmlEncode(userInput);
            return $"<div>Hello {encoded}</div>";
        }
    }

    // Insecure password handling
    public class PasswordSecurityExample
    {
        public bool UnsafePasswordCheck(string password, string userPassword)
        {
            // SECURITY RISK: Plain text password comparison
            return password == userPassword;
        }

        public bool SafePasswordCheck(string password, string hashedPassword, string salt)
        {
            // SAFE: Proper password hashing
            using (var sha256 = SHA256.Create())
            {
                var hashedInput = Convert.ToBase64String(
                    sha256.ComputeHash(Encoding.UTF8.GetBytes(password + salt)));
                return hashedInput == hashedPassword;
            }
        }

        public string CreateSecurePassword(string password, out string salt)
        {
            salt = GenerateSalt();
            using (var sha256 = SHA256.Create())
            {
                return Convert.ToBase64String(
                    sha256.ComputeHash(Encoding.UTF8.GetBytes(password + salt)));
            }
        }

        private string GenerateSalt()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] saltBytes = new byte[32];
                rng.GetBytes(saltBytes);
                return Convert.ToBase64String(saltBytes);
            }
        }
    }

    // Path traversal vulnerability
    public class PathTraversalExample
    {
        public string UnsafeFileAccess(string fileName)
        {
            // SECURITY RISK: Path traversal vulnerability
            string filePath = "/uploads/" + fileName;
            return filePath;
        }

        public string SafeFileAccess(string fileName)
        {
            // SAFE: Validate and sanitize file path
            if (string.IsNullOrWhiteSpace(fileName) || 
                fileName.Contains("..") || 
                fileName.Contains("/") || 
                fileName.Contains("\\"))
            {
                throw new ArgumentException("Invalid file name");
            }

            string filePath = Path.Combine("/uploads/", fileName);
            return Path.GetFullPath(filePath);
        }
    }

    // Information disclosure
    public class InformationDisclosureExample
    {
        public void UnsafeErrorHandling()
        {
            try
            {
                // Some operation that might fail
                throw new Exception("Database connection failed: server=prod-db-01, user=admin");
            }
            catch (Exception ex)
            {
                // SECURITY RISK: Exposing sensitive information
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public void SafeErrorHandling()
        {
            try
            {
                // Some operation that might fail
                throw new Exception("Database connection failed: server=prod-db-01, user=admin");
            }
            catch (Exception ex)
            {
                // SAFE: Log detailed error internally, show generic message to user
                LogError(ex);
                throw new Exception("An internal error occurred. Please try again later.");
            }
        }

        private void LogError(Exception ex)
        {
            // Log to secure internal system
            Console.WriteLine($"Internal Log: {ex}");
        }
    }

    // Weak cryptography
    public class CryptographyExample
    {
        public string WeakEncryption(string data)
        {
            // SECURITY RISK: Using MD5 (weak hash function)
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
        }

        public string StrongEncryption(string data)
        {
            // SAFE: Using SHA256
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
        }

        public string EncryptAES(string plainText, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }
    }

    // Authorization bypass
    public class AuthorizationExample
    {
        public bool UnsafeAuthorization(string userId, string requestedResource)
        {
            // SECURITY RISK: Client-side authorization check only
            if (userId == "admin")
                return true;

            return false;
        }

        public bool SafeAuthorization(string userId, string requestedResource)
        {
            // SAFE: Server-side validation with proper role checking
            var user = GetUserFromDatabase(userId);
            if (user == null)
                return false;

            var permissions = GetUserPermissions(user);
            return permissions.Contains(requestedResource);
        }

        private User GetUserFromDatabase(string userId)
        {
            // Simulate database lookup
            return new User { Id = userId, Role = "user" };
        }

        private List<string> GetUserPermissions(User user)
        {
            // Simulate permission lookup
            if (user.Role == "admin")
                return new List<string> { "read", "write", "delete", "admin" };
            
            return new List<string> { "read" };
        }
    }

    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // Session management
    public class SessionExample
    {
        public void UnsafeSessionHandling(string sessionId)
        {
            // SECURITY RISK: No session validation
            var userData = GetSessionData(sessionId);
            ProcessUserData(userData);
        }

        public void SafeSessionHandling(string sessionId)
        {
            // SAFE: Proper session validation
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new UnauthorizedAccessException("No session provided");

            if (!IsValidSession(sessionId))
                throw new UnauthorizedAccessException("Invalid session");

            if (IsSessionExpired(sessionId))
                throw new UnauthorizedAccessException("Session expired");

            var userData = GetSessionData(sessionId);
            ProcessUserData(userData);
        }

        private object GetSessionData(string sessionId)
        {
            return new { UserId = "123", Role = "user" };
        }

        private void ProcessUserData(object userData)
        {
            Console.WriteLine($"Processing data for: {userData}");
        }

        private bool IsValidSession(string sessionId)
        {
            return !string.IsNullOrWhiteSpace(sessionId) && sessionId.Length >= 32;
        }

        private bool IsSessionExpired(string sessionId)
        {
            // Check session expiration logic
            return false;
        }
    }
}
