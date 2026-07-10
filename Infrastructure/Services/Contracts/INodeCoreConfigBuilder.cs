using Data.Entities;
using RemoteNode.Models;

namespace Infrastructure.Services;

/// <summary>
/// Builds a complete xray-core configuration for a remote node.
/// </summary>
public interface INodeCoreConfigBuilder
{
    /// <summary>
    /// Builds a runtime xray-core configuration from a node template and managed entities.
    /// </summary>
    StartCoreRequest Build(NodeEntity node);
}
