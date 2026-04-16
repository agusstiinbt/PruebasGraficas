using System.ComponentModel;
using System.Resources;

namespace CigoWeb.Core.Localization;

[AttributeUsage(AttributeTargets.Field)]
public sealed class LocalizedDescriptionAttribute : DescriptionAttribute
{
    private readonly string _resourceKey;
    private readonly ResourceManager _resourceManager;
    private readonly string _iconClass;

    public LocalizedDescriptionAttribute(string resourceKey, Type resourceType, string iconClass)
    {
        _resourceKey = resourceKey;
        _resourceManager = new ResourceManager(resourceType);
        _iconClass = iconClass;
    }

    public string IconClass => _iconClass;

    public override string Description => _resourceManager.GetString(_resourceKey) ?? _resourceKey;
}
