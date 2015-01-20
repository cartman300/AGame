#version 150

uniform float Time;
uniform sampler2D tex;
uniform int MultColor;
uniform vec4 ObjColor;
uniform vec2 Resolution;

in vec2 frag_uv;

out vec4 Color;

// Default 0.003
#define CHROMA_OFFSET 0.0

void main() {
	if (CHROMA_OFFSET != 0) {
		vec4 R = texture2D(tex, frag_uv - vec2(CHROMA_OFFSET, 0));
		vec4 G = texture2D(tex, frag_uv - vec2(CHROMA_OFFSET * 2, 0));
		vec4 B = texture2D(tex, frag_uv - vec2(CHROMA_OFFSET * 3, 0));
		Color = vec4(R.r, G.g, B.b, 1.0);
	} else
		Color = texture2D(tex, frag_uv);
}