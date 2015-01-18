#version 150

uniform sampler2D tex;
uniform int MultColor;
uniform vec4 ObjColor;
uniform vec2 Resolution;

in vec2 frag_uv;

out vec4 Color;

vec4 FXAA(sampler2D TEX, vec2 oUV) {
	float FXAA_SPAN_MAX = 8.0;
	float FXAA_REDUCE_MUL = 1.0 / 8.0;
	float FXAA_REDUCE_MIN = (1.0 / 128.0);
  
	vec2 texcoordOffset = vec2(1.0f / Resolution.x, 1.0f / Resolution.y);

	vec3 rgbNW = texture2D(TEX, oUV.xy + (vec2(-1.0, -1.0) * texcoordOffset)).xyz;
	vec3 rgbNE = texture2D(TEX, oUV.xy + (vec2(+1.0, -1.0) * texcoordOffset)).xyz;
	vec3 rgbSW = texture2D(TEX, oUV.xy + (vec2(-1.0, +1.0) * texcoordOffset)).xyz;
	vec3 rgbSE = texture2D(TEX, oUV.xy + (vec2(+1.0, +1.0) * texcoordOffset)).xyz;
	vec3 rgbM  = texture2D(TEX, oUV.xy).xyz;
	
	vec3 luma = vec3(0.299, 0.587, 0.114);
	float lumaNW = dot(rgbNW, luma);
	float lumaNE = dot(rgbNE, luma);
	float lumaSW = dot(rgbSW, luma);
	float lumaSE = dot(rgbSE, luma);
	float lumaM  = dot( rgbM, luma);
	
	float lumaMin = min(lumaM, min(min(lumaNW, lumaNE), min(lumaSW, lumaSE)));
	float lumaMax = max(lumaM, max(max(lumaNW, lumaNE), max(lumaSW, lumaSE)));
	
	vec2 dir = vec2(-((lumaNW + lumaNE) - (lumaSW + lumaSE)), ((lumaNW + lumaSW) - (lumaNE + lumaSE)));
	float dirReduce = max((lumaNW + lumaNE + lumaSW + lumaSE) * (0.25 * FXAA_REDUCE_MUL), FXAA_REDUCE_MIN);
	float rcpDirMin = 1.0 / (min(abs(dir.x), abs(dir.y)) + dirReduce);
	dir = min(vec2(FXAA_SPAN_MAX,  FXAA_SPAN_MAX), max(vec2(-FXAA_SPAN_MAX, -FXAA_SPAN_MAX), dir * rcpDirMin)) * texcoordOffset;
		
	vec4 rgbA = (1.0 / 2.0) * (texture2D(TEX, oUV + dir * (1.0 / 3.0 - 0.5)) + texture2D(TEX, oUV.xy + dir * (2.0 / 3.0 - 0.5)));
	vec4 rgbB = rgbA * (1.0 / 2.0) + (1.0 / 4.0) * (texture2D(TEX, oUV.xy + dir * (0.0/3.0 - 0.5)) +
		texture2D(TEX, oUV.xy + dir * (3.0 / 3.0 - 0.5)));
	float lumaB = dot(rgbB.rgb, luma);

	if ((lumaB < lumaMin) || (lumaB > lumaMax))
		return rgbA;
	else
		return rgbB;
}

void main() {
	Color = FXAA(tex, frag_uv);
	//Color = texture2D(tex, frag_uv);
}