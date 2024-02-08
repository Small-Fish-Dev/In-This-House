HEADER
{
	Description = "Default Shader";
}

FEATURES
{
    #include "vr_common_features.fxc"
	Feature( F_TRANSPARENCY, 0..1, "Rendering" );
	Feature( F_TINT_MASK, 0..1, "Rendering" );
    Feature( F_ADDITIVE_BLEND, 0..1, "Blending" );
}

MODES
{
	VrForward();
	ToolsVis( S_MODE_TOOLS_VIS );
	Depth( S_MODE_DEPTH );
}

COMMON
{
	#define S_TRANSLUCENT 0
	#include "common/shared.hlsl"
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs(  VS_INPUT i  )
	{
		PixelInput o = ProcessVertex( i );
		return FinalizeVertex( o );
	}
}

PS
{ 
	StaticCombo( S_TRANSPARENCY, F_TRANSPARENCY, Sys( ALL ) );
	StaticCombo( S_TINT_MASK, F_TINT_MASK, Sys( ALL ) );
	
	#define CUSTOM_TEXTURE_FILTERING
    SamplerState Sampler < Filter( POINT ); AddressU( WRAP ); AddressV( WRAP ); >;

	StaticCombo( S_MODE_DEPTH, 0..1, Sys( ALL ) );

	#define CUSTOM_MATERIAL_INPUTS
	CreateInputTexture2D( Color, Srgb, 8, "", "_color", "Material,10/10", Default3( 1.0, 1.0, 1.0 ) );
	CreateTexture2DWithoutSampler( g_tColor ) < Channel( RGB, Box( Color ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); Filter( POINT ); >;

	float3 g_vColorTint < UiType( Color ); Default3( 1.0, 1.0, 1.0 ); UiGroup( "Material,10/20" ); >;

    CreateInputTexture2D( Normal, Linear, 8, "NormalizeNormals", "_normal", "Material,10/30", Default3( 0.5, 0.5, 1.0 ) );
	CreateTexture2DWithoutSampler( g_tNormal ) < Channel( RGB, Box( Normal ), Linear ); OutputFormat( DXT5 ); SrgbRead( false ); >;

	CreateInputTexture2D( Metalness, Linear, 8, "", "_metal", "Material,10/40", Default( 0 ) );
	CreateInputTexture2D( Roughness, Linear, 8, "", "_rough", "Material,10/50", Default( 1 ) );
	CreateTexture2DWithoutSampler( g_tRm ) < Channel( R, Box( Roughness ), Linear ); Channel( G, Box( Metalness ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;


    #include "sbox_pixel.fxc"
    #include "common/pixel.hlsl"

	#if ( S_TINT_MASK )
		CreateInputTexture2D( TintMask, Srgb, 8, "", "_tint", "Material,10/60", Default( 1 ) );
		CreateTexture2DWithoutSampler( g_tTintMask ) < Channel( R, Box( TintMask ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); Filter( POINT ); >;
	#endif
    
	#if ( S_TRANSPARENCY )
		#if( !F_RENDER_BACKFACES )
			#define BLEND_MODE_ALREADY_SET
			RenderState( BlendEnable, true );
			RenderState( SrcBlend, SRC_ALPHA );
			RenderState( DstBlend, INV_SRC_ALPHA);
		#endif

		BoolAttribute( translucent, true );

		CreateInputTexture2D( TransparencyMask, Linear, 8, "", "_trans", "Transparency,10/10", Default( 1 ) );
		CreateTexture2DWithoutSampler( g_tTransparencyMask ) < Channel( R, Box( TransparencyMask ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	
		float TransparencyRounding< Default( 0.0f ); Range( 0.0f, 1.0f ); UiGroup( "Transparency,10/20" ); >;
	#endif

	RenderState( CullMode, F_RENDER_BACKFACES ? NONE : DEFAULT );

	#if ( S_MODE_DEPTH )
        #define MainPs Disabled
    #endif

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		float2 UV = i.vTextureCoords.xy;

        Material m;
		float3 color = Tex2DS( g_tColor, Sampler, UV.xy ).rgb;
		m.Albedo = 0;
		#if ( S_TINT_MASK )
			float mask = Tex2DS( g_tTintMask, Sampler, UV.xy ).r;
			m.Albedo = lerp( color, color * g_vColorTint.rgb, mask );
		#else
			m.Albedo = color * g_vColorTint.rgb;
		#endif

        m.Normal = TransformNormal( DecodeNormal( Tex2DS( g_tNormal, Sampler, UV.xy ).rgb ), i.vNormalWs, i.vTangentUWs, i.vTangentVWs );

		float2 rm = Tex2DS( g_tRm, Sampler, UV.xy ).rg;
        m.Roughness = rm.r;
        m.Metalness = rm.g;
        m.AmbientOcclusion = 1;
        m.TintMask = 0;
        m.Opacity = 1;
		m.Emission = 0;
       	m.Emission = 0;
        m.Transmission = 0;

		float4 result = ShadingModelStandard::Shade( i, m );
		#if( S_TRANSPARENCY )
			float alpha = Tex2DS( g_tTransparencyMask, Sampler, UV.xy ).r;
			result.a = max( alpha, floor( alpha + TransparencyRounding ) );
		#endif

		return result;
	}
}