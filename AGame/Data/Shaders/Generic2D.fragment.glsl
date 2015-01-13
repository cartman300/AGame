#version 130

in vec2 frag_uv;

out vec4 Color;

uniform sampler2D tex;

void main() {
	Color = texture(tex, frag_uv);
}