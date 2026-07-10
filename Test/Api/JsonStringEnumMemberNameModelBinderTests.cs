using System.Globalization;
using Api.ModelBinding;
using Contracts.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace Test.Api;

public sealed class JsonStringEnumMemberNameModelBinderTests
{
    [Fact]
    public async Task BindModelAsync_AcceptsMemberNameEnumValues()
    {
        var binder = new JsonStringEnumMemberNameModelBinder(typeof(UserSortBy));
        var context = CreateContext(typeof(UserSortBy), "sortBy", "created_at");

        await binder.BindModelAsync(context);

        context.Result.IsModelSet.Should().BeTrue();
        context.Result.Model.Should().Be(UserSortBy.CreatedAt);
        context.ModelState.ErrorCount.Should().Be(0);
    }

    [Fact]
    public async Task BindModelAsync_RejectsValuesWithoutMemberName()
    {
        var binder = new JsonStringEnumMemberNameModelBinder(typeof(UserSortBy));
        var context = CreateContext(typeof(UserSortBy), "sortBy", "createdAt");

        await binder.BindModelAsync(context);

        context.Result.IsModelSet.Should().BeFalse();
        context.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task BindModelAsync_AcceptsNumericEnumValues()
    {
        var binder = new JsonStringEnumMemberNameModelBinder(typeof(SortOrder));
        var context = CreateContext(typeof(SortOrder), "sortOrder", "1");

        await binder.BindModelAsync(context);

        context.Result.IsModelSet.Should().BeTrue();
        context.Result.Model.Should().Be(SortOrder.Desc);
        context.ModelState.ErrorCount.Should().Be(0);
    }

    private static DefaultModelBindingContext CreateContext(Type modelType, string modelName, string value)
    {
        return new DefaultModelBindingContext
        {
            ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(modelType),
            ModelName = modelName,
            ModelState = new ModelStateDictionary(),
            ValueProvider = new TestValueProvider(modelName, value),
        };
    }

    private sealed class TestValueProvider(string modelName, string value) : IValueProvider
    {
        public bool ContainsPrefix(string prefix)
        {
            return string.Equals(prefix, modelName, StringComparison.Ordinal);
        }

        public ValueProviderResult GetValue(string key)
        {
            return string.Equals(key, modelName, StringComparison.Ordinal)
                ? new ValueProviderResult(new StringValues(value), CultureInfo.InvariantCulture)
                : ValueProviderResult.None;
        }
    }
}
