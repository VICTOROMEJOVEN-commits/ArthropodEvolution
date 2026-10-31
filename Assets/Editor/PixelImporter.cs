using UnityEditor;
public class PixelImporter:AssetPostprocessor {
 void OnPreprocessTexture(){if(!assetPath.Contains("Resources/Art/"))return;var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.isReadable=true;t.filterMode=UnityEngine.FilterMode.Point;t.mipmapEnabled=false;t.textureCompression=TextureImporterCompression.Uncompressed;t.alphaIsTransparency=true;}
}
