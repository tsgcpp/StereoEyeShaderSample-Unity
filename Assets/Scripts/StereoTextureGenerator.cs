using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Renderer))]
public class StereoTextureGenerator : MonoBehaviour
{
    public Texture leftEyeTexture;
    public Texture rightEyeTexture;

    private Renderer _renderer;

    private RenderTexture _stereoTexture;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _stereoTexture = GenerateStereoTexture(
            leftEyeTexture, rightEyeTexture);

        // _MainTexはTex2DArrayとして受け取ることを前提
        _renderer.sharedMaterial.mainTexture = _stereoTexture;
    }

    /// <summary>
    /// Tex2DArray型のRenderTextureを生成し、
    /// 両目用テクスチャをそれぞれのテクスチャにコピーして返す
    /// </summary>
    /// <param name="leftEyeTexture">左目用テクスチャ</param>
    /// <param name="rightEyeTexture">右目用テクスチャ</param>
    /// <param name="targetRT">コピー先のRenderTexture(Tex2DArrayかつvolumeであることが前提)</param>
    private static RenderTexture GenerateStereoTexture(
        Texture leftEyeTexture, Texture rightEyeTexture)
    {
        var desc = new RenderTextureDescriptor(1024, 1024);

        // RenderTextureをTex2DArrayとして生成
        desc.dimension = TextureDimension.Tex2DArray;

        // Textureは2つ(0番目は左目用、1番目は右目用)
        desc.volumeDepth = 2;
        
        // RenderTextureDescriptorからテクスチャを生成
        var stereoTexture = RenderTexture.GetTemporary(desc);

        // 両目用テクスチャをTex2DArray型のRenderTextureにコピー
        Graphics.Blit(
            source: leftEyeTexture, dest: stereoTexture,
            sourceDepthSlice: 0, destDepthSlice: 0);
        Graphics.Blit(
            source: rightEyeTexture, dest: stereoTexture,
            sourceDepthSlice: 0, destDepthSlice: 1);

        return stereoTexture;
    }

    private void OnDestroy()
    {
        _renderer.sharedMaterial.mainTexture = null;
        RenderTexture.ReleaseTemporary(_stereoTexture);
        _stereoTexture = null;
    }
}
