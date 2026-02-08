using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.Reflection;
using System.Xml;

[PrefabExtension(
    "EncyclopediaHeroPage",
    "descendant::RichTextWidget[@Text='@InformationText']"
)]
public sealed class EncyclopediaHeroPagePrefabExtension : PrefabExtensionInsertPatch
{
    public override InsertType Type => InsertType.Append;

    private readonly XmlDocument _document;

    public EncyclopediaHeroPagePrefabExtension()
    {
        _document = new XmlDocument();
        _document.LoadXml(
            ReadEmbeddedText("TelepathyV2.src.GUI.Prefabs.EncyclopediaHeroPageInject.xml")
        );
    }

    [PrefabExtensionXmlDocument(false)]
    public XmlDocument GetPrefabExtension() => _document;


    private static string ReadEmbeddedText(string resourceName)
    {
        var asm = Assembly.GetExecutingAssembly();

        var stream = asm.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new System.Exception("Embedded resource not found: " + resourceName);

        try
        {
            var reader = new System.IO.StreamReader(stream);
            try
            {
                return reader.ReadToEnd();
            }
            finally
            {
                reader.Dispose();
            }
        }
        finally
        {
            stream.Dispose();
        }
    }
}
