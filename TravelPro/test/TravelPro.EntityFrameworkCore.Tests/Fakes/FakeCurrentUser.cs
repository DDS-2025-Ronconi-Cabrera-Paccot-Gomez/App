using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Volo.Abp.Users;
using Volo.Abp.DependencyInjection;

namespace TravelPro.EntityFrameworkCore.Tests.Fakes
{
    public class FakeCurrentUser : ICurrentUser, ISingletonDependency
    {
        private Guid? _id;
        private List<Claim> _claims;

        public FakeCurrentUser(Guid? userId = null)
        {
            _id = userId;
            _claims = new List<Claim>();
        }

        public Guid? Id => _id;
        public string? UserName => "testuser";
        public string? Name => "Test";
        public string? SurName => "User";
        public string? Email => "test@example.com";
        public bool EmailVerified => true;
        public string? PhoneNumber => null;
        public bool PhoneNumberVerified => false;
        public Guid? TenantId => null;
        public string[] Roles => Array.Empty<string>();
        public IEnumerable<Claim> Claims => _claims;
        public bool IsAuthenticated => _id.HasValue;

        // Métodos requeridos por la interfaz (devuelven valores básicos)
        public Claim? FindClaim(string claimType)
            => _claims.FirstOrDefault(c => c.Type == claimType);

        public Claim[] FindClaims(string claimType)
            => _claims.Where(c => c.Type == claimType).ToArray();

        public Claim[] GetAllClaims() => _claims.ToArray();

        public bool IsInRole(string roleName)
            => Roles.Contains(roleName);

        // Métodos utilitarios para test
        public void SetId(Guid? userId)
        {
            _id = userId;
            
        }

        public void SetClaims(IEnumerable<Claim> claims)
        {
            _claims = claims.ToList();
        }
    }
}