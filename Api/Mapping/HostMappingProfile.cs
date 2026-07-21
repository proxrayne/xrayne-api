using Api.Requests;
using Api.Responses;
using Api.Utilities;
using AutoMapper;
using Contracts.Enums;
using Contracts.Exceptions;
using Data.Entities;
using Data.Models;
using OptionalValues;
using Xray.Config.Enums;

namespace Api.Mapping;

/// <summary>
/// Maps subscription host API models.
/// </summary>
public sealed class HostMappingProfile : Profile
{
    /// <summary>
    /// Creates subscription host API mapping rules.
    /// </summary>
    public HostMappingProfile()
    {
        CreateMap<CreateHostRequest, HostEntity>()
            .ConvertUsing<CreateHostRequestConverter>();
        CreateMap<UpdateHostRequest, HostEntity>()
            .ConvertUsing<UpdateHostRequestConverter>();
        CreateMap<PatchHostRequest, HostPatch>()
            .ConvertUsing<PatchHostRequestConverter>();

        CreateMap<InboundEntity, HostInboundDto>()
            .ForCtorParam(
                nameof(HostInboundDto.Port),
                options => options.MapFrom(inbound => inbound.Port.ToString()))
            .ForCtorParam(
                nameof(HostInboundDto.NodeName),
                options => options.MapFrom(inbound => inbound.Node.Name))
            .ForCtorParam(
                nameof(HostInboundDto.ServerName),
                options => options.MapFrom(inbound => GetServerName(inbound)));

        CreateMap<HostInboundGroup, HostNodeInboundsDto>();
        CreateMap<HostEntity, HostListItemDto>();

        CreateMap<HostEntity, HostDto>()
            .ForCtorParam(
                nameof(HostDto.Security),
                options => options.MapFrom(host => HostWireValues.ToSecurityName(host.Security)))
            .ForCtorParam(
                nameof(HostDto.Alpn),
                options => options.MapFrom(host => HostWireValues.ToAlpnNames(host.ALPN)))
            .ForCtorParam(
                nameof(HostDto.Fingerprint),
                options => options.MapFrom(host => HostWireValues.ToFingerprintName(host.Fingerprint)));
    }

    private static string? GetServerName(InboundEntity inbound)
    {
        var stream = inbound.Config.StreamSettings;
        var tlsServerName = Normalize(stream?.TlsSettings?.ServerName);
        if (tlsServerName is not null)
        {
            return tlsServerName;
        }

        var realityServerName = Normalize(stream?.RealitySettings?.ServerName);
        if (realityServerName is not null)
        {
            return realityServerName;
        }

        return stream?.RealitySettings?.ServerNames?
            .Select(Normalize)
            .FirstOrDefault(value => value is not null);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static HostEntity ToEntity(CreateHostRequest request)
    {
        return new HostEntity
        {
            Name = NormalizeRequiredText(request.Name, "Host name is required."),
            Address = NormalizeRequiredText(request.Address, "Host address is required."),
            CountryAlpha2Code = NormalizeCountry(request.CountryAlpha2Code),
            InboundId = request.InboundId,
            Port = ValidatePort(request.Port),
            ServerName = Normalize(request.ServerName),
            Host = Normalize(request.Host),
            Path = Normalize(request.Path),
            Security = ParseSecurityOrDefault(request.Security),
            ALPN = HostWireValues.ParseAlpn(request.Alpn),
            Fingerprint = HostWireValues.ParseFingerprint(request.Fingerprint),
            FragmentTemplate = Normalize(request.FragmentTemplate),
            NoiseTemplate = Normalize(request.NoiseTemplate),
            Enabled = request.Enabled,
            IsMuxEnabled = request.IsMuxEnabled,
            IsUseServerNameAsHost = request.IsUseServerNameAsHost,
            IsRandomUseragent = request.IsRandomUseragent,
            AllowIncrease = request.AllowIncrease
        };
    }

    private static HostEntity ToEntity(UpdateHostRequest request)
    {
        return new HostEntity
        {
            Name = NormalizeRequiredText(request.Name, "Host name is required."),
            Address = NormalizeRequiredText(request.Address, "Host address is required."),
            CountryAlpha2Code = NormalizeCountry(request.CountryAlpha2Code),
            InboundId = request.InboundId,
            Port = ValidatePort(request.Port),
            ServerName = Normalize(request.ServerName),
            Host = Normalize(request.Host),
            Path = Normalize(request.Path),
            Security = ParseSecurityOrDefault(request.Security),
            ALPN = HostWireValues.ParseAlpn(request.Alpn),
            Fingerprint = HostWireValues.ParseFingerprint(request.Fingerprint),
            FragmentTemplate = Normalize(request.FragmentTemplate),
            NoiseTemplate = Normalize(request.NoiseTemplate),
            Enabled = request.Enabled,
            IsMuxEnabled = request.IsMuxEnabled,
            IsUseServerNameAsHost = request.IsUseServerNameAsHost,
            IsRandomUseragent = request.IsRandomUseragent,
            AllowIncrease = request.AllowIncrease
        };
    }

    private static HostPatch ToPatch(PatchHostRequest request)
    {
        return new HostPatch
        {
            Name = MapOptional<string?, string?>(
                request.Name,
                value => NormalizeRequiredText(value, "Host name is required.")),
            Address = MapOptional<string?, string?>(
                request.Address,
                value => NormalizeRequiredText(value, "Host address is required.")),
            CountryAlpha2Code = MapOptional<string?, string?>(request.CountryAlpha2Code, NormalizeCountry),
            InboundId = PassOptional(request.InboundId),
            Port = MapOptional(request.Port, ValidatePort),
            ServerName = MapOptional(request.ServerName, Normalize),
            Host = MapOptional(request.Host, Normalize),
            Path = MapOptional(request.Path, Normalize),
            Security = MapOptional(request.Security, HostWireValues.ParseSecurity),
            ALPN = MapOptional(request.Alpn, HostWireValues.ParseAlpn),
            Fingerprint = MapOptional(request.Fingerprint, HostWireValues.ParseFingerprint),
            FragmentTemplate = MapOptional(request.FragmentTemplate, Normalize),
            NoiseTemplate = MapOptional(request.NoiseTemplate, Normalize),
            Enabled = PassOptional(request.Enabled),
            IsMuxEnabled = PassOptional(request.IsMuxEnabled),
            IsUseServerNameAsHost = PassOptional(request.IsUseServerNameAsHost),
            IsRandomUseragent = PassOptional(request.IsRandomUseragent),
            AllowIncrease = PassOptional(request.AllowIncrease)
        };
    }

    private static OptionalValue<T> MapOptional<TSource, T>(
        OptionalValue<TSource> source,
        Func<TSource, T> convert)
    {
        if (!source.IsSpecified)
        {
            return OptionalValue<T>.Unspecified;
        }

        return convert(source.SpecifiedValue);
    }

    private static OptionalValue<T> PassOptional<T>(OptionalValue<T> source)
    {
        if (!source.IsSpecified)
        {
            return OptionalValue<T>.Unspecified;
        }

        return source.SpecifiedValue;
    }

    private static HostSecurity ParseSecurityOrDefault(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? HostSecurity.InboundDefault
            : HostWireValues.ParseSecurity(value);
    }

    private static int? ValidatePort(int? port)
    {
        if (port is < 1 or > 65535)
        {
            throw new BadRequestException("Host port must be between 1 and 65535.");
        }

        return port;
    }

    private static string NormalizeRequiredText(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BadRequestException(message);
        }

        return value.Trim();
    }

    private static string NormalizeCountry(string? value)
    {
        var country = NormalizeRequiredText(value, "Country code is required.").ToUpperInvariant();
        if (country.Length != 2)
        {
            throw new BadRequestException("Country code must be an ISO 3166-1 alpha-2 value.");
        }

        return country;
    }

    private sealed class CreateHostRequestConverter : ITypeConverter<CreateHostRequest, HostEntity>
    {
        public HostEntity Convert(CreateHostRequest source, HostEntity destination, ResolutionContext context)
        {
            return ToEntity(source);
        }
    }

    private sealed class UpdateHostRequestConverter : ITypeConverter<UpdateHostRequest, HostEntity>
    {
        public HostEntity Convert(UpdateHostRequest source, HostEntity destination, ResolutionContext context)
        {
            return ToEntity(source);
        }
    }

    private sealed class PatchHostRequestConverter : ITypeConverter<PatchHostRequest, HostPatch>
    {
        public HostPatch Convert(PatchHostRequest source, HostPatch destination, ResolutionContext context)
        {
            return ToPatch(source);
        }
    }
}

internal sealed record HostInboundGroup(
    long NodeId,
    string NodeName,
    IReadOnlyList<InboundEntity> Inbounds);
