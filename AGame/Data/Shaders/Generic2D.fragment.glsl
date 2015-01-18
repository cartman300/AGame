#version 150

uniform sampler2D tex;
uniform int MultColor;
uniform vec4 ObjColor;

in vec2 frag_uv;

out vec4 Color;

void main() {
	Color = texture(tex, frag_uv);
}