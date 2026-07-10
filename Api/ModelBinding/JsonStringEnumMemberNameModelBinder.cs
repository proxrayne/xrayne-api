using Contracts.Utilities;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.ModelBinding;

/// <summary>
/// Binds public API enum query values from JSON enum member names.
/// </summary>
public sealed class JsonStringEnumMemberNameModelBinder(Type enumType) : IModelBinder
{
    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
        var value = valueProviderResult.FirstValue;

        if (string.IsNullOrWhiteSpace(value))
        {
            if (Nullable.GetUnderlyingType(bindingContext.ModelType) is not null)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }
            else
            {
                AddError(bindingContext, value ?? string.Empty);
            }

            return Task.CompletedTask;
        }

        if (EnumWireNames.TryParse(enumType, value, out var result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }

        AddError(bindingContext, value);

        return Task.CompletedTask;
    }

    private void AddError(ModelBindingContext bindingContext, string value)
    {
        var allowedValues = string.Join(", ", EnumWireNames.GetNames(enumType).Order());
        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            $"The value '{value}' is not valid for {enumType.Name}. Allowed values: {allowedValues}.");
    }
}
