using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using contester.Features.Users;
using Microsoft.IdentityModel.Tokens;

namespace contester.Features.UserGroups;

public class PrincipalDto
{
    public required string Type { get; set; }
    public Guid Id { get; set; }
    public required string DisplayName { get; set; }
    public required string ProfilePictureUrl { get; set; } 
}

public class PrincipalProfile : Profile
{
    private static string Md5Hash(string input)
    {
        var encodedInput = new UTF8Encoding().GetBytes(input); 
        var hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")!).ComputeHash(encodedInput);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
    
    public PrincipalProfile()
    {
        CreateMap<User, PrincipalDto>()
            .ForMember(p => p.Type, opt => opt.MapFrom(_ => "User"))
            .ForMember(p => p.DisplayName, opt => opt.MapFrom(u =>
                $"{u.LastName} {u.FirstName}{(u.Patronymic.IsNullOrEmpty() ? string.Empty : " " + u.Patronymic)}"))
            .ForMember(u => u.ProfilePictureUrl, opt => opt.MapFrom(u =>
                $"https://gravatar.com/avatar/{Md5Hash(u.Email.ToLowerInvariant())}?s=400&d=mm&r=g"));

        CreateMap<UserGroup, PrincipalDto>()
            .ForMember(p => p.Type, opt => opt.MapFrom(_ => "Group"))
            .ForMember(p => p.DisplayName, opt => opt.MapFrom(g => g.Name))
            .ForMember(p => p.ProfilePictureUrl, opt => opt.MapFrom(g =>
                $"https://gravatar.com/avatar/{Md5Hash(g.Name)}?s=400&d=mm&r=g"));
    }
}
