HEADER
{
	Description = "Sauna Shader";
}

FEATURES
{
	// Basic stuff
	#include "vr_common_features.fxc"				// All shit falls apart without this .fxc include
	Feature( F_ADDITIVE_BLEND, 0..1, "Blending" );	// This feature is fucking GLUED to DstBlend, which is used in depth pass. Can't get rid of it apparently.

	// Transparency, alpha test & extra fancy bullshit for them
    Feature( F_ALPHA_TEST, 0..1, "Rendering" );
	Feature( F_TRANSPARENCY, 0..1, "Rendering" );
	Feature( F_DETAILED_ALPHA_SHADOWS, 0..1, "Rendering")
	FeatureRule( Requires1(F_ALPHA_TEST, F_TRANSPARENCY), "You might want to enable Transparency for this material first.");
	FeatureRule( Requires1(F_DETAILED_ALPHA_SHADOWS, F_ALPHA_TEST), "Detailed shadows for transparent materials. Very slow!");

	// Emissive texture
	Feature( F_EMISSIVE, 0..1, "Rendering" );

	// Vertex snapping 
	Feature( F_VERTEX_SNAPPING, 0..1, "Extra" );

	// UV scroll
	Feature( F_UV_SCROLL, 0..2(0="Disabled", 1="Scroll X axis", 2="Scroll Y axis"), "Extra" );

	// Texture layering
	Feature( F_TEXTURE_LAYER, 0..1, "Texture Layer" );
	Feature( F_LAYER_PROJECTION, 0..2(0="Project to X+ axis", 1="Project to Y+ axis", 2="Project to Z+ axis"), "Texture Layer" );
	Feature( F_LAYER_AXIS_INVERT, 0..1(0="Keep positive-oriented axis", 1="Invert the axis"), "Texture Layer" );
	FeatureRule( Allow1( F_TRANSPARENCY, F_TEXTURE_LAYER ), "Transparent materials can't work with texture layering enabled, sorry." );
	FeatureRule( Requires1( F_LAYER_PROJECTION, F_TEXTURE_LAYER ), "Please enable texture layering to select project orientation." );
	FeatureRule( Requires1( F_LAYER_AXIS_INVERT, F_TEXTURE_LAYER ), "Please enable texture layering to select project orientation." );
}

MODES
{
	VrForward();
	Depth( S_MODE_DEPTH );
}

//=========================================================================================================================
COMMON
{
	#define S_TRANSLUCENT 0	// <=== do we need this? we've pretty much hardcoded it to be always 'false'. 
	#include "common/shared.hlsl"
	StaticCombo( S_MODE_DEPTH, 0..1, Sys( ALL ) );
}

//=========================================================================================================================

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

//=========================================================================================================================

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

//=========================================================================================================================

VS
{
	#include "common/vertex.hlsl"

	StaticCombo( S_VERTEX_SNAPPING, F_VERTEX_SNAPPING, Sys( ALL ) );	// Probably should change F_VERTEX_SNAPPING state using C# attribute when settings are done?

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );

		#if S_VERTEX_SNAPPING
			float3 vPositionWs = o.vPositionWs.xyz;
			float dist = distance(g_vCameraPositionWs, vPositionWs);

			float scale = RemapValClamped( dist, 1000, 5000, 240, 800 );
			float4 vertex = Position3WsToPs( vPositionWs.xyz );
			vertex.xyz = vertex.xyz / vertex.w;
			vertex.xy = floor( scale * vertex.xy ) / scale;
			vertex.xyz *= vertex.w;

			o.vPositionPs = vertex;
		#endif

		return FinalizeVertex( o );
	}
}

//=========================================================================================================================

PS
{ 
	// Apparently this needs to be put before includes or else material shoves Color and Translucency inputs (...but why?)
	#define CUSTOM_MATERIAL_INPUTS

	// 
	// Includes
	//
    #include "sbox_pixel.fxc"
    #include "common/pixel.hlsl"
    
	// 
	// Declare all used static combos
	//
	StaticCombo( S_TRANSPARENCY, F_TRANSPARENCY, Sys( ALL ) );
    StaticCombo( S_ALPHA_TEST, F_ALPHA_TEST, Sys( ALL ) );
	StaticCombo( S_DETAILED_ALPHA_SHADOWS, F_DETAILED_ALPHA_SHADOWS, Sys( ALL ) );
	StaticCombo( S_DO_NOT_CAST_SHADOWS, F_DO_NOT_CAST_SHADOWS, Sys( ALL ) );
	StaticCombo( S_EMISSIVE, F_EMISSIVE, Sys( ALL ) );
	StaticCombo( S_TEXTURE_LAYER, F_TEXTURE_LAYER, Sys( ALL ) );
	StaticCombo( S_LAYER_PROJECTION, F_LAYER_PROJECTION, Sys( ALL ) );
	StaticCombo( S_LAYER_AXIS_INVERT, F_LAYER_AXIS_INVERT, Sys( ALL ) );
	StaticCombo( S_UV_SCROLL, F_UV_SCROLL, Sys( ALL ) );

	//
	// Prepare inputs for all textures
	//
	CreateInputTexture2D( Color, 		Srgb, 	8, "", "_color", "Material,10/10", Default3( 1.0, 1.0, 1.0 ) );						// Color map, RGB
	CreateInputTexture2D( ColorTintMask, 	Linear, 8, "", "_tint", "Material,10/20", Default3( 1.0, 1.0, 1.0 ) );					// Tint mask, goes into alpha channel of color map
	CreateInputTexture2D( Normal, 		Linear, 8, "NormalizeNormals", "_normal", "Material,10/30", Default3( 0.5, 0.5, 1.0 ) );	// Normal map, expects OpenGL format
	CreateInputTexture2D( Roughness, 	Linear, 8, "", "_rough", "Material,10/40", Default( 1 ) );									// Roughness map, goes to g_tRm in R channel	
	CreateInputTexture2D( Metalness, 	Linear, 8, "", "_metal",  "Material,10/50", Default( 0 ) );									// Metalness map, goes to g_tRm in G channel

	// 
	// Create Texture2D for all basic inputs (all extra textures from static combos are handled below)
	// 
	Texture2D ColorMap 		< Channel( RGB, Box( Color ), Srgb ); Channel( A, Box( ColorTintMask ), Linear ); OutputFormat( BC7 ); SrgbRead( true ); >;
	Texture2D NormalMap 	< Channel( RGB, Box( Normal), Linear ); OutputFormat( DXT5 ); SrgbRead( false ); >; 
	Texture2D g_tRm			< Channel( R, Box( Roughness), Linear ); Channel( G, Box( Metalness ), Linear); OutputFormat( BC7 ); SrgbRead ( false ); >; 
	
	// 
	// Prepare 2nd material for layering, add variables for blend intensity and mask contrast.
	//
	#if S_TEXTURE_LAYER
		CreateInputTexture2D ( LayerColor,		Srgb, 	8, "", 					"_color", 	"Material Layered,40/10", Default3( 1.0, 1.0, 1.0 ) );
		CreateInputTexture2D ( LayerNormal,		Linear, 8, "NormalizeNormals", 	"_normal", 	"Material Layered,40/20", Default3( 1.0, 1.0, 1.0 ) );
		CreateInputTexture2D ( LayerRoughness, 	Linear, 8, "", 					"_rough", 	"Material Layered,40/30", Default( 1 ) );
		CreateInputTexture2D ( LayerMetalness, 	Linear, 8, "", 					"_metal", 	"Material Layered,40/40", Default( 0 ) );

		Texture2D g_tColorMap2	< Channel( RGB, Box( LayerColor ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;
		Texture2D g_tNormalMap2	< Channel( RGB, Box( LayerNormal ), Linear ); OutputFormat( DXT5 ); SrgbRead( false ); >;
		Texture2D g_tRm2		< Channel( R, Box( LayerRoughness), Linear ); Channel( G, Box( LayerMetalness ), Linear); OutputFormat( BC7 ); SrgbRead ( false ); >;
	
		float g_flBlendStrength < Default( 2 ); Range( 0.1, 8 ); UiGroup( "Material Layered,40/50" ); >;	// How much area is covered by layer texture
		float g_flBlendContrast < Default( 1 ); Range( 0.1, 8 ); UiGroup( "Material Layered,40/60" ); >;	// Lower value = smoother edges of a mask
		float g_flBlendNormalsStrength < Default( 1 ); Range( 0, 3 ); UiGroup("Material Layered,40/70" ); >;	// Control over intensity of two normal map blending
	#endif

	// 
	// Variables
	//
	float3 g_flColorTint < Attribute( "g_flColorTint" ); UiType( Color ); Default3( 1.0, 1.0, 1.0 ); UiGroup( "Material,10/20" ); >;

	//
	// UV scroll properties
	//
	#if S_UV_SCROLL
		float g_flUvScrollSpeed < UiType( Slider ); Default( 1.0f ); Range( 0.1f, 32.0f ); UiGroup( "UV Scroll,30/10"); >;
	#endif

	//
	// Fucking VRAD nonsense
	// 
	#if S_ALPHA_TEST
		TextureAttribute( LightSim_Opacity_A, ColorMap );
	#endif

	// 
	// Emissive map
	// 
	#if ( S_EMISSIVE )
		float EmissionStrength < UiType( Slider ); Default( 1.0f ); Range( 0, 5.0 ); UiGroup( "Emission,20/10" );  >;

		CreateInputTexture2D( Emission, Srgb, 8, "", "", "Emission,20/20", Default3( 0, 0, 0 ) );
		Texture2D g_tEmission < Channel( RGB, Box( Emission ), Linear ); OutputFormat( BC7 ); SrgbRead( true ); >;
	#endif 

	//
	// Transparency & thousands of render states
	// 
	#if ( S_TRANSPARENCY )
		#if( !F_RENDER_BACKFACES && !S_ALPHA_TEST )
			#define BLEND_MODE_ALREADY_SET
			RenderState( BlendEnable, true );
			RenderState( SrcBlend, SRC_ALPHA );
			RenderState( DstBlend, INV_SRC_ALPHA);
			BoolAttribute( translucent, true );
		#endif

		#if( S_ALPHA_TEST && !S_MODE_DEPTH )
			RenderState( AlphaToCoverageEnable, true );
		#endif

		CreateInputTexture2D( TransparencyMask, Linear, 8, "", "_trans", "Transparency,10/10", Default( 1 ) );
		Texture2D g_tTransparencyMask < Channel( R, Box( TransparencyMask ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;

		float TransparencyRounding< Default( 0.0f ); Range( 0.0f, 1.0f ); UiGroup( "Transparency,10/20" ); >;
	#endif

	RenderState( CullMode, F_RENDER_BACKFACES ? NONE : DEFAULT );

	#if( S_MODE_DEPTH && !S_ALPHA_TEST )
		#define MainPs Disabled
	#endif

	float3 BlendNormals( float3 NrmA, float3 NrmB ) 
	{
		NrmA += float3(  0,  0, 1 );
		NrmB *= float3( -1, -1, 1 );

		return NrmA * dot( NrmA, NrmB ) / NrmA.z - NrmB;
	}

	float3 NormalIntensity( float3 normal, float intensity )
	{
		return float3( normal.rg * intensity, normal.b );
	}

	//
	// Main
	//
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		//
		// Very basic crap
		//
		float2 UV = i.vTextureCoords.xy;

		// Overwrite UV values if UV scrolling is enabled. This is messy. 
		#if S_UV_SCROLL
			UV = S_UV_SCROLL == 2 ? float2( i.vTextureCoords.x, i.vTextureCoords.y + ( g_flTime * g_flUvScrollSpeed ) ) : float2( i.vTextureCoords.x + ( g_flTime * g_flUvScrollSpeed ), i.vTextureCoords.y );
		#endif

		float3 colorTint = g_flColorTint * i.vVertexColor.rgb;

		//
		// Sample textures
		//
		float4 l_tColor = ColorMap.Sample( g_sPointWrap, UV ).rgba;
		float2 rm = g_tRm.Sample( g_sPointWrap, UV ).rg;
		float3 l_tNormal = NormalMap.Sample( g_sPointWrap, UV ).rgb;

		// 
		// Set up first material (that's the only material created if S_TEXTURE_LAYER is not enabled)
		//
        Material m = Material::Init();
        m.Albedo = lerp( l_tColor.rgb, l_tColor.rgb * colorTint, l_tColor.a );
        m.Normal = TransformNormal( DecodeNormal( l_tNormal ), i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		
        m.Roughness = rm.r;
        m.Metalness = rm.g;
        m.AmbientOcclusion = 1;
        m.TintMask = 0;
        m.Opacity = 1;
		m.Emission = 0;
		#if( S_EMISSIVE )
       	 	m.Emission = g_tEmission.Sample( g_sPointWrap, UV ).rgb * EmissionStrength;
		#endif
        m.Transmission = 0;

		// 
		// Set up 2nd material if S_TEXTURE_LAYER is enabled, then combine it with the main material. 
		// 
		#if ( S_TEXTURE_LAYER )
			// Sample roughness&metalness maps, initialize second layer material
			float2 rm2 = g_tRm2.Sample( g_sPointWrap, UV ).rg;
			Material l = Material::Init();

			// Sample and assign textures to the material
			l.Albedo = g_tColorMap2.Sample( g_sPointWrap, UV ).rgb;
			l.Normal = TransformNormal( DecodeNormal( g_tNormalMap2.Sample( g_sPointWrap, UV ).rgb ), i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
			l.Roughness = rm2.r;
			l.Metalness = rm2.g;

			// Get transformed normal map depending on selected axis in material settings, then invert it if S_LAYER_AXIS_INVERT is checked:
			float projectionNormal = saturate( ( ( S_LAYER_PROJECTION == 0 ? m.Normal.r : ( S_LAYER_PROJECTION == 1 ? m.Normal.g : m.Normal.b ) ) * g_flBlendStrength - 0.5f) * max( g_flBlendContrast, 0 ) + 0.5f );

			l.Normal = BlendNormals( NormalIntensity( m.Normal, g_flBlendNormalsStrength ), l.Normal );
			m = Material::lerp( m, l, S_LAYER_AXIS_INVERT ? 1.0 - projectionNormal : projectionNormal );	// Update primary material with a result of lerping between two mats
		#endif

		float4 result = ShadingModelStandard::Shade( i, m );

		#if( S_TRANSPARENCY )
			float alpha = g_tTransparencyMask.Sample( g_sPointWrap, UV ).r; 
			result.a = max( alpha, floor( alpha + TransparencyRounding ) );
		#endif

		// Definitely should revisit this thing someday because it is quite a mess :S
		#if( S_MODE_DEPTH )
		{
			#if ( S_DO_NOT_CAST_SHADOWS ) 
			discard;
			#endif

			#if( S_DETAILED_ALPHA_SHADOWS )
			{if ( result.a < 0.5 ) discard;}
			#endif
 			return 0;
		}
		#endif

		return result;
	}
}