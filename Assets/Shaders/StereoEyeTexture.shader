Shader "StereoEyeTexture"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			// texture arrayが有効なプラットフォームのみで子のシェーダーが使用可能であることの宣言
            #pragma require 2darray
            
            #include "UnityCG.cginc"

			/*
			unity_StereoEyeIndexについて

			unity_StereoEyeIndexはXR環境であれば基本的に`#include`経由で変数宣言される。
			実行中のシェーディング処理が左目もしくは右目のどっち向けかの判定に使用される。
			値が 0: 左目、1: 右目。
			*/

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;

				// instanceID変数の宣言
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{

				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;

				// instanceIDをfragに送る場合は必要(今回は不要)
				// UNITY_VERTEX_INPUT_INSTANCE_ID

				// fragにunity_StereoEyeIndexなどの情報を流すための変数を追加
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert (appdata v)
			{
				v2f output;

				// instanceIDおよびunity_StereoEyeIndexの取得
				UNITY_SETUP_INSTANCE_ID(v);

				// instanceIDをfragに送る場合は必要(今回は不要)
				// UNITY_TRANSFER_INSTANCE_ID(v, o);

				// outputの初期化
				UNITY_INITIALIZE_OUTPUT(v2f, output);

				// unity_StereoEyeIndexをoutput内の変数に付与
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				
				// vertexとuvは通常通り
				output.vertex = UnityObjectToClipPos(v.vertex);
				output.uv = v.uv;

				return output;
			}

			// _MainTexをsampler2DArrayとして宣言
			UNITY_DECLARE_TEX2DARRAY(_MainTex);

			fixed4 frag (v2f input) : SV_Target
			{
				// vertから流れてきたinputからunity_StereoEyeIndexを更新
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				// inputを使用しなくても、以下の関数でunity_StereoEyeIndexを更新可能(今回は不使用)
				// UNITY_SETUP_INSTANCE_ID(i);
				
				// unit_StereoEyeIndex番目のテクスチャをサンプリング
				return UNITY_SAMPLE_TEX2DARRAY(_MainTex,
					float3((input.uv).xy, (float)unity_StereoEyeIndex));
			}

            ENDCG
        }
    }
}