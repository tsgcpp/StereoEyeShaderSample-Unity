using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Renderer))]
public class StereoTextureGenerator : MonoBehaviour
{
    [SerializeField] private Texture _leftEyeTexture;
    [SerializeField] private Texture _rightEyeTexture;

    [Tooltip("Generate RenderTexture with \"vrUsage\" as \"VRTextureUsage.TwoEyes\".\nFYI: ")]
    [SerializeField] private CopyType _copyType = CopyType.VRUsageAsNone;

    private Renderer _renderer;

    private RenderTexture _stereoTexture;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _stereoTexture = GenerateStereoTexture(_leftEyeTexture, _rightEyeTexture);

        // _MainTexはTex2DArrayとして受け取ることを前提
        _renderer.sharedMaterial.mainTexture = _stereoTexture;
    }

    private RenderTexture GenerateStereoTexture(
        Texture leftEyeTexture, Texture rightEyeTexture)
    {
        switch (_copyType)
        {
            case CopyType.VRUsageAsTwoEyes:
                return GenerateStereoTextureAsTwoEyes(leftEyeTexture, rightEyeTexture);
            case CopyType.VRUsageAsTwoEyesAndCopyDirect:
                return GenerateStereoTextureAsTwoEyesAndCopyDirect(leftEyeTexture, rightEyeTexture);
            default:
                return GenerateStereoTextureAsNone(leftEyeTexture, rightEyeTexture);
        }
    }

    /// <summary>
    /// Tex2DArray型のRenderTextureを生成し、
    /// 両目用テクスチャをそれぞれのテクスチャにコピーして返す
    /// </summary>
    /// <param name="leftEyeTexture">左目用テクスチャ</param>
    /// <param name="rightEyeTexture">右目用テクスチャ</param>
    /// <param name="targetRT">コピー先のRenderTexture(Tex2DArrayかつvolumeであることが前提)</param>
    private static RenderTexture GenerateStereoTextureAsNone(
        Texture leftEyeTexture, Texture rightEyeTexture)
    {
        var desc = new RenderTextureDescriptor(1024, 1024);

        // RenderTextureをTex2DArrayとして生成
        desc.dimension = TextureDimension.Tex2DArray;

        // Textureは2つ(0番目は左目用、1番目は右目用)
        desc.volumeDepth = 2;

        desc.vrUsage = VRTextureUsage.None;

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

    private static RenderTexture GenerateStereoTextureAsTwoEyes(
        Texture leftEyeTexture, Texture rightEyeTexture)
    {
        var desc = new RenderTextureDescriptor(1024, 1024);

        desc.dimension = TextureDimension.Tex2DArray;
        desc.volumeDepth = 2;
        desc.vrUsage = VRTextureUsage.TwoEyes;

        var stereoTexture = RenderTexture.GetTemporary(desc);

        // バグ対策のためのコピー用
        // 詳細: https://tsgcpp.hateblo.jp/entry/2020/06/27/200431
        desc.vrUsage = VRTextureUsage.None;
        var copyTexture = RenderTexture.GetTemporary(desc);

        // 両目用テクスチャをTex2DArray型のRenderTextureにコピー
        Graphics.Blit(
            source: leftEyeTexture, dest: copyTexture,
            sourceDepthSlice: 0, destDepthSlice: 0);
        Graphics.Blit(
            source: rightEyeTexture, dest: copyTexture,
            sourceDepthSlice: 0, destDepthSlice: 1);

        Graphics.Blit(source: copyTexture, dest: stereoTexture);

        RenderTexture.ReleaseTemporary(copyTexture);

        return stereoTexture;
    }

    /// <summary>
    /// This function will cause a bug that "leftEyeTexture" (destDepthSlice: 0) is not copied correctly.
    /// </summary>
    private static RenderTexture GenerateStereoTextureAsTwoEyesAndCopyDirect(
        Texture leftEyeTexture, Texture rightEyeTexture)
    {
        var desc = new RenderTextureDescriptor(1024, 1024);

        desc.dimension = TextureDimension.Tex2DArray;
        desc.volumeDepth = 2;
        desc.vrUsage = VRTextureUsage.TwoEyes;

        var stereoTexture = RenderTexture.GetTemporary(desc);

        // Copy textures to stereoTexture which vrUsage is TwoEyes
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

    public enum CopyType
    {
        VRUsageAsNone = 0,
        VRUsageAsTwoEyes = 1,
        VRUsageAsTwoEyesAndCopyDirect = 2,
    }
}
