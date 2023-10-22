HEADER
{
	Description = "Screen Fog";
}

MODES
{
	Default();
    VrForward();
}

COMMON
{
	#include "postprocess/shared.hlsl"
}

struct PS_INPUT
{
	#if ( PROGRAM == VFX_PROGRAM_VS )
		float4 vPositionPs : SV_Position;
	#endif

	#if ( ( PROGRAM == VFX_PROGRAM_PS ) )
		float4 vPositionSs : SV_ScreenPosition;
	#endif
};

struct VS_INPUT
{
    float3 vPositionOs : POSITION < Semantic( PosXyz ); >;
};

VS
{
    PS_INPUT MainVs( VS_INPUT i )
    {
        PS_INPUT o;
        o.vPositionPs = float4(i.vPositionOs.xyz, 1.0f);
        return o;
    }
}

PS
{
    #include "postprocess/common.hlsl"

	float4 g_vColor < Attribute( "Color" ); Default4( 1, 1, 1, 1 ); >;
    float g_flMinDistance < Attribute( "MinDistance" ); Default( 250 ); >;
	float g_flMaxDistance < Attribute( "MaxDistance" ); Default( 1500 ); >;
    float g_flMaxOpacity < Attribute( "Opacity" ); Default( 1 ); >;
    
    CreateTexture2D( g_tFrameTexture ) < Attribute( "FrameTexture" ); SrgbRead( true ); Filter( POINT ); AddressU( MIRROR ); AddressV( MIRROR ); >;
    CreateTexture2DMS( g_tDepthTexture ) < Attribute( "DepthTexture" ); SrgbRead( false ); Filter( POINT ); AddressU( CLAMP ); AddressV( CLAMP ); >;
    
	float fetchDepth( float2 coords )
	{
		float projectedDepth = 1.0f;

        float2 texelSize = TextureDimensions2D(g_tFrameTexture, 0);
        projectedDepth = Tex2DMS(g_tDepthTexture, int2(coords.xy * texelSize), 0).r;
		projectedDepth = RemapValClamped(projectedDepth, g_flViewportMinZ, g_flViewportMaxZ, 0.0, 1.0);

		float depthRelativeToRay = 1.0 / ((projectedDepth * g_vInvProjRow3.z + g_vInvProjRow3.w));
		return depthRelativeToRay * 2.0f;
	}

    float4 MainPs( PS_INPUT i ) : SV_Target0
    { 		
        float3 col = Tex2D(g_tFrameTexture, i.vPositionSs.xy / g_vViewportSize.xy).rgb;
        float z = fetchDepth(i.vPositionSs.xy / g_vViewportSize.xy);
        float3 coords = float3((i.vPositionSs.xy / g_vViewportSize.xy * float2(g_vViewportSize.x / g_vViewportSize.y, 1.0f) - float2(g_vViewportSize.x / g_vViewportSize.y, 1.0f) / 2) * z, z);
        float dist = length(coords);

        float value = max(min((dist - g_flMinDistance) / (g_flMaxDistance + g_flMinDistance), g_flMaxOpacity), 0);
        float3 color = lerp(col.rgb, g_vColor.rgb * g_vColor.a, smoothstep(-0.075, 1, value));

        return float4( color.rgb, 1 );
    }
}