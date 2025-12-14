namespace Shared.Common.Domain.Contracts;

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTimeOffset LastModifiedAt { get; set; }
    string? LastModifiedBy { get; set; }
}