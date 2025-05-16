using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Application.Models.Dtos.RestDtos;
using Application.Models.Enums;
using Core.Domain.Entities;
using FluentValidation;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;
using Microsoft.Extensions.Options;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace Application.Services;

public class SecurityService(
    IOptionsMonitor<AppOptions> optionsMonitor,
    IUserRepository repository,
    IUserSettingsRepository userSettingsRepository,
    IValidator<AuthLoginDto> loginValidator,
    IValidator<AuthRegisterDto> registerValidator) : ISecurityService
{
    
    public async Task<AuthResponseDto> Login(AuthLoginDto dto)
    {
        await loginValidator.ValidateAndThrowAsync(dto);
        var player = repository.GetUserOrNull(dto.Email) ?? throw new ValidationException("Username not found");
        VerifyPasswordOrThrow(dto.Password + player.Salt, player.Hash);
        
        return new AuthResponseDto
        {
            Jwt = GenerateJwt(new JwtClaims
            {
                Id = player.UserId.ToString(),
                Role = player.Role,
                Exp = DateTimeOffset.UtcNow.AddHours(1000)
                    .ToUnixTimeSeconds()
                    .ToString(),
                Email = dto.Email
            })
        };
    }

    public async Task<AuthResponseDto> Register(AuthRegisterDto dto)
    {
        await registerValidator.ValidateAndThrowAsync(dto);

        var salt = GenerateSalt();
        var hash = HashPassword(dto.Password + salt);
        var userId = Guid.NewGuid();

        var user = repository.AddUser(new User
        {
            UserId = userId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Birthday = DateTime.SpecifyKind(dto.Birthday, DateTimeKind.Utc),
            Country = dto.Country,
            Role = Constants.UserRole,
            Salt = salt,
            Hash = hash
        });
        
        userSettingsRepository.Add(new UserSettings
        {
            UserId = userId,
            Celsius = true,
            DarkTheme = false,
            ConfirmDialog = false,
            SecretMode = false
        });

        return new AuthResponseDto
        {
            Jwt = GenerateJwt(new JwtClaims
            {
                Id = user.UserId.ToString(),
                Role = user.Role,
                Exp = DateTimeOffset.UtcNow.AddHours(1000).ToUnixTimeSeconds().ToString(),
                Email = user.Email
            })
        };
    }


    /// <summary>
    ///     Gives hex representation of SHA512 hash
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    public string HashPassword(string password)
    {
        using var sha512 = SHA512.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha512.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    public void VerifyPasswordOrThrow(string password, string hashedPassword)
    {
        if (HashPassword(password) != hashedPassword)
            throw new AuthenticationException("Invalid login");
    }

    public string GenerateSalt()
    {
        return Guid.NewGuid().ToString();
    }

    public string GenerateJwt(JwtClaims claims)
    {
        var tokenBuilder = new JwtBuilder()
            .WithAlgorithm(new HMACSHA512Algorithm())
            .WithSecret(optionsMonitor.CurrentValue.JwtSecret)
            .WithUrlEncoder(new JwtBase64UrlEncoder())
            .WithJsonSerializer(new JsonNetSerializer());

        foreach (var claim in claims.GetType().GetProperties())
            tokenBuilder.AddClaim(claim.Name, claim.GetValue(claims)!.ToString());
        return tokenBuilder.Encode();
    }

    public JwtClaims VerifyJwtOrThrow(string jwt)
    {
        var token = new JwtBuilder()
            .WithAlgorithm(new HMACSHA512Algorithm())
            .WithSecret(optionsMonitor.CurrentValue.JwtSecret)
            .WithUrlEncoder(new JwtBase64UrlEncoder())
            .WithJsonSerializer(new JsonNetSerializer())
            .MustVerifySignature()
            .Decode<JwtClaims>(jwt);

        return token;
    }
}