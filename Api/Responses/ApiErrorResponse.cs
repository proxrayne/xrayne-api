using System.Text.Json.Serialization;

namespace Api.Responses;

public sealed record ApiErrorResponse([property: JsonIgnore] int Status, string Name, string Detail);
