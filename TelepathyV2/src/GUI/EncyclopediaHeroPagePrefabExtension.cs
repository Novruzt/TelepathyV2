using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.Reflection;
using System.Xml;

[PrefabExtension("EncyclopediaHeroPage", "descendant::TextWidget[@Text='@SkillsText']")]
public sealed class EncyclopediaHeroPagePrefabExtension : PrefabExtensionInsertPatch
{
    public override InsertType Type => InsertType.Prepend;

    private readonly XmlDocument _document;

    public EncyclopediaHeroPagePrefabExtension()
    {
        _document = new XmlDocument();
        _document.LoadXml("<EncyclopediaHeroPageInject />");
    }

    [PrefabExtensionInsertPatch.PrefabExtensionXmlDocumentAttribute(false)]
    public XmlDocument GetPrefabExtension()
    {
        return this._document;
    }
}
