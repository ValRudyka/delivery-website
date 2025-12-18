namespace delivery_website
{
    /// <summary>
    /// Marker class for shared resources localization.
    /// This class is intentionally empty and serves as a type reference for IStringLocalizer.
    /// The class MUST be in the root namespace (not delivery_website.Resources) because
    /// when ResourcesPath = "Resources" is set, ASP.NET Core constructs the resource path as:
    /// {ResourcesPath}/{TypeFullName - RootNamespace}.resx
    /// So for delivery_website.SharedResources with ResourcesPath="Resources", it looks for:
    /// Resources/SharedResources.resx (correct!)
    /// But for delivery_website.Resources.SharedResources, it would look for:
    /// Resources/Resources/SharedResources.resx (wrong!)
    /// </summary>
    public class SharedResources
    {
    }
}
