using Application.Models;
using Application.Models.Dtos.RestDtos;

namespace Application.Interfaces;

public interface ISecurityService
{
    public string HashPassword(string password);
    public void VerifyPasswordOrThrow(string password, string hashedPassword);
    public string GenerateSalt();
    public string GenerateJwt(JwtClaims claims);
    public Task<AuthResponseDto> Login(AuthLoginDto dto);
    public Task<AuthResponseDto> Register(AuthRegisterDto dto);
    public JwtClaims VerifyJwtOrThrow(string jwt);
}