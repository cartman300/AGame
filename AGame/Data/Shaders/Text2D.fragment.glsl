#version 130

in vec2 frag_uv;
out vec4 Color;
uniform sampler2D tex;

#define COLOR vec3(1.0, 1.0, 1.0)

void main() {
	float FntA = texture(tex, frag_uv).a;
	Color = vec4(COLOR, FntA);
}