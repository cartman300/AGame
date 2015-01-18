#version 150

uniform sampler2D tex;
uniform vec4 ObjColor;
uniform int MultColor;

in vec2 frag_uv;
in vec4 frag_clr;

out vec4 Color;

#define COLOR vec3(1.0, 1.0, 1.0)

void main() {
	float FntA = texture(tex, frag_uv).a;
	vec4 Clr = vec4(COLOR, FntA) * frag_clr;

	if (MultColor == 1)
		Clr *= ObjColor;
	else
		Clr += ObjColor;

	Color = Clr;
}