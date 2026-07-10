using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.ModelBinding;

/// <summary>
/// Provides JSON enum member-name query binders for enum values.
/// </summary>
public sealed class JsonStringEnumMemberNameModelBinderProvider : IModelBinderProvider
{
    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var enumType = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;

        return enumType.IsEnum ? new JsonStringEnumMemberNameModelBinder(enumType) : null;
    }
}
