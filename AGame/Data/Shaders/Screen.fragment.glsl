out vec4 Color;

vec4 FXAA(sampler2D, vec2, vec2);

void main() {
	Color = FXAA(Texture, __uv, Resolution);
}