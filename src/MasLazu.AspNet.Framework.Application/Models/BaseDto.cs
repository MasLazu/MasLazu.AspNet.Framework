namespace MasLazu.AspNet.Framework.Application.Models;

public record BaseDto(Guid Id, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);