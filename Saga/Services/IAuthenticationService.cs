using Saga.Models;

namespace Saga.Services
{
    public interface IAuthenticationService
    {
        // Basic Authentication
        Task<AuthenticationResult> LoginAsync(string serverUrl, string username, string password);
        Task<AuthenticationResult> LoginWithSSOAsync(AuthenticationProvider provider);
        Task<bool> LogoutAsync();
        Task<bool> RefreshTokenAsync();
        
        // User Management
        Task<UserProfile> GetCurrentUserAsync();
        Task<bool> UpdateUserProfileAsync(UserProfile user);
        Task<List<UserProfile>> GetAvailableUsersAsync();
        Task<bool> SwitchUserAsync(string userId);
        Task<bool> AddUserAsync(UserProfile user);
        Task<bool> RemoveUserAsync(string userId);
        
        // Token Management  
        Task<string> GetValidTokenAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<bool> ValidateTokenAsync(string token);
        
        // Multi-User Session
        Task<MultiUserSession> GetCurrentSessionAsync();
        Task<bool> UpdateSessionAsync(MultiUserSession session);
        
        // Events
        event EventHandler<AuthenticationResult> AuthenticationChanged;
        event EventHandler<UserProfile> UserSwitched;
    }
}