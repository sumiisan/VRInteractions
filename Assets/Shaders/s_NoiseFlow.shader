Shader "Custom/s_NoiseFlow"
{
Properties
{
_Color ("Color", Color) = (1,0.5,0,1)
}
SubShader {
Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
Pass {
    Tags { "LightMode" = "ForwardBase" }
    Blend One DstColor
    Cull Front
    CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    #pragma multi_compile_fog
    
    #include "UnityCG.cginc"
    fixed4 _Color;
    
    float random(in float x) {
        return frac(sin(x)*1e4);
    }

    float random2(in fixed2 st) {
        return frac(sin(dot(st.xy, fixed2(12.9898,78.233)))* 43758.5453123);
    }
    
    fixed3 random3(fixed3 c) {
        fixed j = 4096.0*sin(dot(c,fixed3(17.0, 59.4, 15.0)));
        fixed3 r;
        r.z = frac(512.0*j);
        j *= .125;
        r.x = frac(512.0*j);
        j *= .125;
        r.y = frac(512.0*j);
        return r-0.5;
    }
    /*
    float pattern(fixed2 st, fixed2 v, float t) {
        fixed2 p = floor(st*0.5+v*0.5+(st*v*0.001));
        return step(t, 
            //random3(100.+fixed3(p.x,p.y,1.))*1.0
            random2(100.+p)*1.0
            + random(p.x)*0.3
           );
    }
    */
    fixed3 pattern(fixed2 st, fixed2 v, float t, fixed hue) {
        fixed2 p = floor(st*0.5+v*0.5+(st*v*0.001));
        fixed m = random2(100.+p)*1.0 + random(p.x)*0.3;
        return 
        lerp(
            step(t, fixed3(m,m,m)),
            fixed3(
                random2(100.+p)*1.0 + random(p.x)*0.3,
                random2(105.+p)*1.1 + random(p.x+0.1)*0.2,
                random2(110.+p)*1.2 + random(p.x+0.2)*0.25
            ),
            hue
        );
    }
    
    struct v2f {
        fixed4 pos : SV_POSITION;
        half2 uv : TEXCOORD0;
        fixed3 normal : NORMAL;
        UNITY_FOG_COORDS(1)
    };
     
    v2f vert (appdata_full v) {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord;
        o.normal = v.normal;
        UNITY_TRANSFER_FOG(o,o.pos);
        return o;
    }
    
    fixed4 frag (v2f i) : SV_Target {
        fixed skew = _SinTime.y; //_Color.z - 0.5;
        fixed mag = 0.93 - _Color.x * 0.9;
        fixed freq = _Color.y * _Color.y * 8. + 3.;
        fixed hue = _Color.z;
        freq *= freq;
        
        fixed2 resolution = _ScreenParams;
        fixed2 st = i.uv;
        st.x *= resolution.x / resolution.y;
        st.x += st.y * skew * 1.5;
        
        fixed2 grid = fixed2(freq,pow(2.4,3. * _Color.y + 2.)*7.0);
        st *= grid;

        fixed2 ipos = floor(st);  // integer
        fixed2 fpos = frac(st);  // fraction

        fixed v = _Time*2.*max(grid.x,grid.y);
        fixed2 vel = fixed2(v,v);
        vel *= fixed2(0.,-0.5) * random(2.0+ipos.x); // direction
        
        fixed3 m = pattern(st,vel,0.5+0.1/resolution.y,hue);
        m = clamp(m, 0., 1.);
        m *= step(mag,fpos.x);
        
        fixed3 col = ShadeSH9(fixed4(UnityObjectToWorldNormal(i.normal), 1.0)) * m;
        fixed4 c = fixed4(col,hue);
        UNITY_APPLY_FOG(i.fogCoord, c);
        return c;
    }
    ENDCG
}
}
}